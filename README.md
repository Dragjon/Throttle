# Throttle Chess Engine
<img src="https://github.com/Dragjon/Throttle/blob/main/images/throttle.png" width="300" height="300">
Throttle is a UCI chess engine developed using Sebastian Lague's chess framework.

## Version
```2.5```

## Rating
```
Testing for v2.5
Rank Name                          Elo     +/-   Games   Score    Draw
   1 polaris1.4.1popcnt            475     108     180   93.9%    3.3%
   2 throttlev2.5                  290      68     180   84.2%    5.0%
   3 tantabusv1.0.2                129      53     180   67.8%    4.4%
   4 madchess                      -41      51     180   44.2%    0.6%
   5 shallowblue                   -83      52     180   38.3%    2.2%
   6 infrared                     -223      61     180   21.7%    3.3%
   7 winterv0.1.1                 -inf     nan     180    0.0%    0.0%

Estimate: 2447 - 2521
```

| Version | Estimate Elo | CCRL Blitz | CCRL Bullet | CEDR |
|---------|--------------|------------|-------------|------|
| 2.1     | 2488         | -          | -           | -    |
| 2.5     | 2484         | -          | -           | -    |

## Features
### Search:
- Budget Aspiration window ```v2.4``` ```16.6 +/- 9.6```
- Fail-Soft Negamax Search
- Principle Variation Search
- Triple PVS ```v1.5``` ```21.3 +/- 11.1```

### Pruning:
- Actual TT pruning ```v2.1``` ```136.4 +/- 31.2```
- A/B Pruning
- Null Move Pruning
- Reverse Futility Pruning
- Futility Pruning
- Quiescence Search Standing Pat Pruning
- Quiescence Search Delta Pruning ```v2.5``` ```11.7 +/- 7.7```

### Reductions:
- Internal Iterative Reduction
- Late move reductions with triple pvs

### Extensions:
- Check Extensions

### Ordering:
- TT Moves
- MVV-LVA (for good captures)
- Killer moves (quiets)
- History moves (quiets) ```v2.0``` ```67.9 +/- 20.9```

### Time Management:
- Hard and Soft Time Management

### Evaluation:
- SWAR compression ```v2.2``` ``` 5.7 +/- 4.6```
- Material values (PeSTO)
- Piece square tables (PeSTO)
- Tapered Eval
- Tempo
- Mobility

## Log of feature addition
```
Log after v2.1
---------------
v2.2 - SWAR test for compressed eval for speedup
Score of SWARtest vs Original: 5383 - 5132 - 4808  [0.508] 15323
...      SWARtest playing White: 3347 - 1892 - 2423  [0.595] 7662
...      SWARtest playing Black: 2036 - 3240 - 2385  [0.421] 7661
...      White vs Black: 6587 - 3928 - 4808  [0.587] 15323
Elo difference: 5.7 +/- 4.6, LOS: 99.3 %, DrawRatio: 31.4 %
SPRT: llr 2.95 (100.3%), lbound -2.94, ubound 2.94 - H1 was accepted

v2.3  - Check for non pv before pruning or reducing in tt
(COMPUTER BLUESCREENED HALFWAY SO THIS IS JUST PARTIAL RESULTS)
Score of NoTTCutoffsInPv vs Original: 2204 - 2080 - 1824  [0.510] 6108
...      NoTTCutoffsInPv playing White: 1359 - 788 - 908  [0.593] 3055
...      NoTTCutoffsInPv playing Black: 845 - 1292 - 916  [0.427] 3053
...      White vs Black: 2651 - 1633 - 1824  [0.583] 6108
Elo difference: 7.1 +/- 7.3, LOS: 97.1 %, DrawRatio: 29.9 %
SPRT: llr 1.64 (55.8%), lbound -2.94, ubound 2.94

v2.4 - Budget aspiration window search
Score of ASP vs Original: 1307 - 1139 - 1072  [0.524] 3518
...      ASP playing White: 810 - 433 - 516  [0.607] 1759
...      ASP playing Black: 497 - 706 - 556  [0.441] 1759
...      White vs Black: 1516 - 930 - 1072  [0.583] 3518
Elo difference: 16.6 +/- 9.6, LOS: 100.0 %, DrawRatio: 30.5 %
SPRT: llr 2.96 (100.4%), lbound -2.94, ubound 2.94 - H1 was accepted

v2.5 - Delta pruning
Score of DeltaPrune vs Original: 2000 - 1817 - 1615  [0.517] 5432
...      DeltaPrune playing White: 1252 - 668 - 796  [0.608] 2716
...      DeltaPrune playing Black: 748 - 1149 - 819  [0.426] 2716
...      White vs Black: 2401 - 1416 - 1615  [0.591] 5432
Elo difference: 11.7 +/- 7.7, LOS: 99.8 %, DrawRatio: 29.7 %
SPRT: llr 2.95 (100.2%), lbound -2.94, ubound 2.94 - H1 was accepted
```

## Special thanks to (in no particular order)
- Ciekce (For helping with a lot of engine-related stuff)
- cj5716 (For helping with a lot of engine-related stuff)
- arandomnoob (For helping with a lot of engine-related stuff)
- Toanth (For hosting the 400 token challenge to really get me into chess programming)
- jw (nnue stuff and bullet)
- and many other people in sebastian lague's chess programming server

## Engines I took inspiration from (in no particular order)
- Boychesser
- Smol.cs
- 200 token monstrosity
- NOSPRT
- Atadofanoobbot
- King Gambot IV
- Viridithas (wiki)

## Resources I found useful
- https://minuskelvin.net/chesswiki/
- https://www.chessprogramming.org/
- https://github.com/cosmobobak/viridithas/blob/master/wiki.md
- https://discord.gg/S3MMh67q

## TODO
- Make it stronger 
