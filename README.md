# Throttle Chess Engine
<img src="https://github.com/Dragjon/Throttle/blob/main/images/throttle.png" width="300" height="300">
Throttle is a UCI chess engine developed using Sebastian Lague's chess framework.

## Version
2.0

## Features
###
* Feature elo gain after 1.4
###
** Feature added at version after 1.4
### Search:
#### Fail-Soft Negamax Search
#### Principle Variation Search
- Triple PVS **1.5 21.3 +/- 11.1,

### Pruning:
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
- History moves (quiets) **v2.0 *67.9 +/- 20.9

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
