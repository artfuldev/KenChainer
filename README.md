# KenChainer
* Author(s): Sudarsan Balaji
* License: *The MIT License (MIT)* (see LICENSE)
* Application Version: 1.1.0
* ReadMe Version: 1.1.0

## Number Chain Solver
Starting with a number, find a way in which if you insert all the four arithmetical signs - two of them twice - in the blanks and carry out all the operations in sequence to obtain the answer given.

## Sample Problem

The Times of India

Chennai Times

Page 4

September 6, 2015

Result = 5

Sequence = 4`_`1`_`9`_`1`_`7`_`6`_`5

## Solution

### Brute Force Method

First Solution: `14ms`

4`+`1`+`9`-`1`-`7`/`6`x`5

All Solutions:  `2ms`

4`+`1`+`9`-`1`-`7`/`6`x`5

4`+`1`+`9`/`1`-`7`-`6`x`5

4`-`1`+`9`+`1`-`7`/`6`x`5

4`-`1`x`9`+`1`/`7`+`6`-`5

4`x`1`+`9`/`1`-`7`-`6`+`5

4`x`1`+`9`/`1`-`7`/`6`x`5

4`/`1`+`9`+`1`-`7`-`6`x`5

4`/`1`+`9`x`1`-`7`-`6`+`5

4`/`1`+`9`x`1`-`7`/`6`x`5

Based on the benchmarks, I think Brute Force is just doing fine. No need to think of any more intelligent alternatives to cut down processing time.