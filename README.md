# Throttle Chess Engine
<img src="https://github.com/Dragjon/Throttle/blob/main/images/throttle.png" width="300" height="300">
Throttle is a UCI chess engine developed using Sebastian Lague's chess framework.

## Features

### Search:
- Fail-Soft Negamax Search
- Principle Variation Search
- Quiescence search

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

### Time Management:
- Hard and Soft Time Management

### Evaluation:
- Tempo of 15
- Monility bonus
- Material values (PeSTO)
- Piece square tables (PeSTO)
- Tapered Evaluation
