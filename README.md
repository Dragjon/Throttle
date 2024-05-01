# Throttle Chess Engine
<img src="https://github.com/Dragjon/Throttle/blob/main/images/throttle.png" width="300" height="300">
Throttle is a UCI chess engine developed using Sebastian Lague's chess framework.

## Version
2.1

## Rating
```
Rank Name                          Elo     +/-   Games   Score    Draw
   1 polaris1.4.1popcnt            338     199      36   87.5%    8.3%
   2 throttlev2.1                  244     146      38   80.3%    7.9%
   3 grekov6.25                    143     127      36   69.4%    5.6%
   4 madchess                      -19     114      36   47.2%    5.6%
   5 shallowblue                  -114     123      38   34.2%    0.0%
   6 infrared                     -218     146      36   22.2%    5.6%
   7 winterv0.1.1                 -417     nan      36    8.3%    0.0%

Estimate: 2438 - 2538
```

## Features
### Search:
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

### Reductions:
- Budget Internal Iterative Reduction

### Extensions:
- Check Extensions

### Ordering:
- TT Moves
- MVV-LVA (for good captures and quiets)
- Killer moves (quiets)
- History moves (quiets) ```v2.0``` ```67.9 +/- 20.9```

### Time Management:
- Hard and Soft Time Management

### Evaluation:
- Material values (PeSTO)
- Piece square tables (PeSTO)
- Tapered Eval
- Tempo
- Mobility

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
