# Engine
Throttle is a UCI chess engine created using Sebastian Lague's chess framework.
# Features
***Search:
    * Fail-Soft Negamax Search
    * Principle Variation Search
    * Quiescence search
    * Pruning:
        * A/B Pruning
        * Null Move Pruning
        * Reverse Futility Pruning
        * Futility Pruning
        * Quiescence Search Standing Pat Pruning 
    * Reductions:
        * Budget Internal Iterative Reduction
    * Extensions:
        * Check Extensions
    * Ordering:
        * TT Moves
        * MVV-LVA (for good captures and quiets)
        * Killer moves (quiets)
    * Time management:
        * Hard and Soft time management

**Evaluation:
    * Material values (PeSTO)
    * Piece square tables (PeSTO)
    * Tapered Eval
