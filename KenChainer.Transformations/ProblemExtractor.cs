using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using KenChainer.Core;

namespace KenChainer.Transformations
{
    public class ProblemExtractor : IProblemExtractor
    {
        private static int _count = 0;
        /// <summary>
        /// The OCR engine
        /// </summary>
        private Tesseract _ocr;

        private static string AsNewName(string imagePath, string newName)
        {
            return imagePath.Replace("Problems", "Results").Replace(".jpg", $"-{newName}.jpg");
        }

        /// <summary>
        /// Create a problem extractor
        /// </summary>
        /// <param name="dataPath">
        /// The datapath must be the name of the parent directory of tessdata and
        /// must end in / . Any name after the last / will be stripped.
        /// </param>
        public ProblemExtractor(string dataPath)
        {
            //create OCR engine
            _ocr = new Tesseract(dataPath, "eng", OcrEngineMode.TesseractCubeCombined);
            _ocr.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZ-1234567890");
        }

        public NumberChainProblem GetProblem(string imagePath)
        {
            var image = new Image<Bgr, byte>(imagePath);
            var gray = image.Convert<Gray, byte>();
#if DEBUG
            gray.Save(AsNewName(imagePath, "gray"));
#endif
            var blurred = gray.SmoothGaussian(5);
#if DEBUG
            blurred.Save(AsNewName(imagePath, "blurred"));
#endif
            var closed = blurred.Erode(11).Dilate(11);
#if DEBUG
            closed.Save(AsNewName(imagePath, "closed"));
#endif
            var normalized = closed;
            CvInvoke.Normalize(closed, normalized, 0, 255, NormType.MinMax);
#if DEBUG
            normalized.Save(AsNewName(imagePath, "normalized"));
#endif
            var thresholded = normalized.ThresholdAdaptive(new Gray(255), AdaptiveThresholdType.GaussianC,
                ThresholdType.Binary, 19, new Gray(7));
#if DEBUG
            thresholded.Save(AsNewName(imagePath, "thresholded"));
#endif
            var opened = thresholded.Erode(12).Dilate(6);
#if DEBUG
            opened.Save(AsNewName(imagePath, "opened"));
#endif
            var canny = new Mat(thresholded.Size, DepthType.Cv8U, 1);
            CvInvoke.Canny(opened, canny, 100, 50, 3, false);
#if DEBUG
            canny.Save(AsNewName(imagePath, "canny"));
#endif
            var contours = new VectorOfVectorOfPoint();
            int[,] hierachy = CvInvoke.FindContourTree(canny, contours, ChainApproxMethod.ChainApproxSimple);
            double maxArea = 0;
            var bestContours = new VectorOfVectorOfPoint();
            for (var idx = 0; idx >= 0; idx = hierachy[idx, 0])
            {
                using (VectorOfPoint contour = contours[idx])
                {
                    var area = CvInvoke.ContourArea(contour);
                    if (area > maxArea)
                    {
                        maxArea = area;
                        bestContours.Clear();
                        bestContours.Push(contour);
                    }
                }
            }
            var mask = new Matrix<byte>(image.Rows, image.Cols);
            mask.SetValue(0);
            try
            {
                CvInvoke.DrawContours(mask, bestContours, 0, new MCvScalar(255), -1);
                CvInvoke.DrawContours(mask, bestContours, 0, new MCvScalar(0), 2);
            }
            catch (Exception exception)
            {

            }
            var masked = mask;
            CvInvoke.BitwiseAnd(mask, gray, masked);
#if DEBUG
            masked.Save(AsNewName(imagePath, "masked"));
#endif
            // Get Rectangle
            RotatedRect box = CvInvoke.MinAreaRect(bestContours[0]);
            if (box.Angle < -45.0)
            {
                float tmp = box.Size.Width;
                box.Size.Width = box.Size.Height;
                box.Size.Height = tmp;
                box.Angle += 90.0f;
            }
            else if (box.Angle > 45.0)
            {
                float tmp = box.Size.Width;
                box.Size.Width = box.Size.Height;
                box.Size.Height = tmp;
                box.Angle -= 90.0f;
            }

            double whRatio = (double) box.Size.Width/box.Size.Height;

            // Filter it
            using (UMat tmp1 = new UMat())
            using (UMat tmp2 = new UMat())
            {
                PointF[] srcCorners = box.GetVertices();

                PointF[] destCorners = new PointF[]
                {
                    new PointF(0, box.Size.Height - 1),
                    new PointF(0, 0),
                    new PointF(box.Size.Width - 1, 0),
                    new PointF(box.Size.Width - 1, box.Size.Height - 1)
                };

                using (Mat rot = CameraCalibration.GetAffineTransform(srcCorners, destCorners))
                {
                    CvInvoke.WarpAffine(gray, tmp1, rot, Size.Round(box.Size));
                }

                //resize the license problem such that the front is ~ 10-12. This size of front results in better accuracy from tesseract
                Size approxSize = new Size(240, 180);
                double scale = Math.Min(approxSize.Width/box.Size.Width, approxSize.Height/box.Size.Height);
                Size newSize = new Size((int) Math.Round(box.Size.Width*scale), (int) Math.Round(box.Size.Height*scale));
                CvInvoke.Resize(tmp1, tmp2, newSize, 0, 0, Inter.Cubic);

                //removes some pixels from the edge
                int edgePixelSize = 2;
                Rectangle newRoi = new Rectangle(new Point(edgePixelSize, edgePixelSize),
                    tmp2.Size - new Size(2*edgePixelSize, 2*edgePixelSize));
                UMat problem = new UMat(tmp2, newRoi);
#if DEBUG
                problem.Save(AsNewName(imagePath, "problem"));
#endif
                UMat filteredProblem = FilterProblem(problem, imagePath);
#if DEBUG
                filteredProblem.Save(AsNewName(imagePath, "problem-filtered"));
#endif
                // image = image.Erode(2).Dilate(2);
                //image.Bitmap.Save(imagePath.Replace(".jpg", "-temp.jpg"));
                return new NumberChainProblem(0);
            }
        }

        private static int GetNumberOfChildren(int[,] hierachy, int idx)
        {
            //first child
            idx = hierachy[idx, 2];
            if (idx < 0)
                return 0;

            int count = 1;
            while (hierachy[idx, 0] > 0)
            {
                count++;
                idx = hierachy[idx, 0];
            }
            return count;
        }

        private void FindNumberChainProblem(
            VectorOfVectorOfPoint contours, int[,] hierachy, int idx, IInputArray gray, IInputArray canny,
            List<IInputOutputArray> problemImagesList, List<IInputOutputArray> filteredProblemImagesList, List<RotatedRect> detectedProblemRegionList,
            List<string> licenses, string imagePath)
        {
            for (; idx >= 0; idx = hierachy[idx, 0])
            {
                int numberOfChildren = GetNumberOfChildren(hierachy, idx);
                //if it does not contains any children (charactor), it is not a license problem region
                if (numberOfChildren == 0) continue;

                using (VectorOfPoint contour = contours[idx])
                {
                    if (CvInvoke.ContourArea(contour) > 20)
                    {
                        if (numberOfChildren < 3)
                        {
                            //If the contour has less than 3 children, it is not a license problem (assuming license problem has at least 3 charactor)
                            //However we should search the children of this contour to see if any of them is a license problem
                            FindNumberChainProblem(contours, hierachy, hierachy[idx, 2], gray, canny, problemImagesList,
                                filteredProblemImagesList, detectedProblemRegionList, licenses, imagePath);
                            continue;
                        }

                        RotatedRect box = CvInvoke.MinAreaRect(contour);
                        if (box.Angle < -45.0)
                        {
                            float tmp = box.Size.Width;
                            box.Size.Width = box.Size.Height;
                            box.Size.Height = tmp;
                            box.Angle += 90.0f;
                        }
                        else if (box.Angle > 45.0)
                        {
                            float tmp = box.Size.Width;
                            box.Size.Width = box.Size.Height;
                            box.Size.Height = tmp;
                            box.Angle -= 90.0f;
                        }

                        double whRatio = (double)box.Size.Width / box.Size.Height;
                        if (!(3.0 < whRatio && whRatio < 10.0))
                            //if (!(1.0 < whRatio && whRatio < 2.0))
                        {
                            //if the width height ratio is not in the specific range,it is not a license problem 
                            //However we should search the children of this contour to see if any of them is a license problem
                            //Contour<Point> child = contours.VNext;
                            if (hierachy[idx, 2] > 0)
                                FindNumberChainProblem(contours, hierachy, hierachy[idx, 2], gray, canny, problemImagesList,
                                    filteredProblemImagesList, detectedProblemRegionList, licenses, imagePath);
                            continue;
                        }

                        using (UMat tmp1 = new UMat())
                        using (UMat tmp2 = new UMat())
                        {
                            PointF[] srcCorners = box.GetVertices();

                            PointF[] destCorners = new PointF[] {
                                new PointF(0, box.Size.Height - 1),
                                new PointF(0, 0),
                                new PointF(box.Size.Width - 1, 0),
                                new PointF(box.Size.Width - 1, box.Size.Height - 1)};

                            using (Mat rot = CameraCalibration.GetAffineTransform(srcCorners, destCorners))
                            {
                                CvInvoke.WarpAffine(gray, tmp1, rot, Size.Round(box.Size));
                            }

                            //resize the license problem such that the front is ~ 10-12. This size of front results in better accuracy from tesseract
                            Size approxSize = new Size(240, 180);
                            double scale = Math.Min(approxSize.Width / box.Size.Width, approxSize.Height / box.Size.Height);
                            Size newSize = new Size((int)Math.Round(box.Size.Width * scale), (int)Math.Round(box.Size.Height * scale));
                            CvInvoke.Resize(tmp1, tmp2, newSize, 0, 0, Inter.Cubic);

                            //removes some pixels from the edge
                            int edgePixelSize = 2;
                            Rectangle newRoi = new Rectangle(new Point(edgePixelSize, edgePixelSize),
                                tmp2.Size - new Size(2 * edgePixelSize, 2 * edgePixelSize));
                            UMat plate = new UMat(tmp2, newRoi);

                            UMat filteredProblem = FilterProblem(plate, imagePath);
                            filteredProblem.Save(imagePath.Replace(".jpg", $"-filteredproblem{++_count}.jpg"));

                            Tesseract.Character[] words;
                            StringBuilder strBuilder = new StringBuilder();
                            using (UMat tmp = filteredProblem.Clone())
                            {
                                _ocr.Recognize(tmp);
                                words = _ocr.GetCharacters();

                                if (words.Length == 0) continue;

                                for (int i = 0; i < words.Length; i++)
                                {
                                    strBuilder.Append(words[i].Text);
                                }
                            }

                            licenses.Add(strBuilder.ToString());
                            problemImagesList.Add(plate);
                            filteredProblemImagesList.Add(filteredProblem);
                            detectedProblemRegionList.Add(box);

                        }
                    }
                }
            }
        }

        /// <summary>
        /// Filter the problem image to remove noise
        /// </summary>
        /// <param name="problem">The problem image</param>
        /// <returns>Problem image without the noise</returns>
        private static UMat FilterProblem(UMat problem, string imagePath)
        {
            UMat thresh = new UMat();
            //CvInvoke.Normalize(problem, problem, 0, 255, NormType.MinMax);
#if DEBUG
            problem.Save(AsNewName(imagePath, "problem-normalized"));
#endif
            CvInvoke.Threshold(problem, thresh, 120, 255, ThresholdType.BinaryInv);
#if DEBUG
            thresh.Save(AsNewName(imagePath, "problem-thresholded"));
#endif
            //Image<Gray, Byte> thresh = problem.ThresholdBinaryInv(new Gray(120), new Gray(255));

            Size plateSize = problem.Size;
            using (Mat plateMask = new Mat(plateSize.Height, plateSize.Width, DepthType.Cv8U, 1))
            using (Mat plateCanny = new Mat())
            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
            {
                plateMask.SetTo(new MCvScalar(255.0));
                CvInvoke.Canny(problem, plateCanny, 100, 50);
                CvInvoke.FindContours(plateCanny, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);

                int count = contours.Size;
                for (int i = 1; i < count; i++)
                {
                    using (VectorOfPoint contour = contours[i])
                    {

                        Rectangle rect = CvInvoke.BoundingRectangle(contour);
                        if (rect.Height > (plateSize.Height >> 1))
                        {
                            rect.X -= 1; rect.Y -= 1; rect.Width += 2; rect.Height += 2;
                            Rectangle roi = new Rectangle(Point.Empty, problem.Size);
                            rect.Intersect(roi);
                            CvInvoke.Rectangle(plateMask, rect, new MCvScalar(), -1);
                            //plateMask.Draw(rect, new Gray(0.0), -1);
                        }
                    }

                }

                thresh.SetTo(new MCvScalar(), plateMask);
            }

            CvInvoke.Erode(thresh, thresh, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);
            CvInvoke.Dilate(thresh, thresh, null, new Point(-1, -1), 1, BorderType.Constant, CvInvoke.MorphologyDefaultBorderValue);

            return thresh;
        }
    }
}