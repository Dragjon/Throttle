using System;
using System.Linq;
using ChessChallenge.API;
using static ChessChallenge.API.BitboardHelper;

/*
| Throttle - A c# UCI chess engine |
  --------------------------------
Version: 1.2

Features:
    
    Search:
        - Fail-Soft Negamax Search
        - Principle Variation Search
        - Quiescence search
        - Pruning:
            - A/B Pruning
            - Null Move Pruning
            - Reverse Futility Pruning
            - Futility Pruning
            - Quiescence Search Standing Pat Pruning 
        - Reductions:
            - Budget Internal Iterative Reduction
        - Extensions:
            - Check Extensions
        - Ordering:
            - TT Moves
            - MVV-LVA (for good captures and quiets)
            - Killer moves (quiets)
        - Time management:
            - Hard and Soft time management

    Evaluation:
        - Material values (PeSTO)
        - Piece square tables (PeSTO)
        - Tapered Eval
    
*/


public class MyBot : IChessBot
{

    // PeSTO's Psqts

    int[,] mg_pst_values = {
    {
      0,   0,   0,   0,   0,   0,  0,   0,
     98, 134,  61,  95,  68, 126, 34, -11,
     -6,   7,  26,  31,  65,  56, 25, -20,
    -14,  13,   6,  21,  23,  12, 17, -23,
    -27,  -2,  -5,  12,  17,   6, 10, -25,
    -26,  -4,  -4, -10,   3,   3, 33, -12,
    -35,  -1, -20, -23, -15,  24, 38, -22,
      0,   0,   0,   0,   0,   0,  0,   0,

    },
    {
    -167, -89, -34, -49,  61, -97, -15, -107,
     -73, -41,  72,  36,  23,  62,   7,  -17,
     -47,  60,  37,  65,  84, 129,  73,   44,
      -9,  17,  19,  53,  37,  69,  18,   22,
     -13,   4,  16,  13,  28,  19,  21,   -8,
     -23,  -9,  12,  10,  19,  17,  25,  -16,
     -29, -53, -12,  -3,  -1,  18, -14,  -19,
    -105, -21, -58, -33, -17, -28, -19,  -23,
    },
    {
    -29,   4, -82, -37, -25, -42,   7,  -8,
    -26,  16, -18, -13,  30,  59,  18, -47,
    -16,  37,  43,  40,  35,  50,  37,  -2,
     -4,   5,  19,  50,  37,  37,   7,  -2,
     -6,  13,  13,  26,  34,  12,  10,   4,
      0,  15,  15,  15,  14,  27,  18,  10,
      4,  15,  16,   0,   7,  21,  33,   1,
    -33,  -3, -14, -21, -13, -12, -39, -21,
    },
    {
     32,  42,  32,  51, 63,  9,  31,  43,
     27,  32,  58,  62, 80, 67,  26,  44,
     -5,  19,  26,  36, 17, 45,  61,  16,
    -24, -11,   7,  26, 24, 35,  -8, -20,
    -36, -26, -12,  -1,  9, -7,   6, -23,
    -45, -25, -16, -17,  3,  0,  -5, -33,
    -44, -16, -20,  -9, -1, 11,  -6, -71,
    -19, -13,   1,  17, 16,  7, -37, -26,
    },
    {
    -28,   0,  29,  12,  59,  44,  43,  45,
    -24, -39,  -5,   1, -16,  57,  28,  54,
    -13, -17,   7,   8,  29,  56,  47,  57,
    -27, -27, -16, -16,  -1,  17,  -2,   1,
     -9, -26,  -9, -10,  -2,  -4,   3,  -3,
    -14,   2, -11,  -2,  -5,   2,  14,   5,
    -35,  -8,  11,   2,   8,  15,  -3,   1,
     -1, -18,  -9,  10, -15, -25, -31, -50,
    },
    {
    -65,  23,  16, -15, -56, -34,   2,  13,
     29,  -1, -20,  -7,  -8,  -4, -38, -29,
     -9,  24,   2, -16, -20,   6,  22, -22,
    -17, -20, -12, -27, -30, -25, -14, -36,
    -49,  -1, -27, -39, -46, -44, -33, -51,
    -14, -14, -22, -46, -44, -30, -15, -27,
      1,   7,  -8, -64, -43, -16,   9,   8,
    -15,  36,  12, -54,   8, -28,  24,  14,
    }
};

    int[,] eg_pst_values = {
    {
      0,   0,   0,   0,   0,   0,   0,   0,
    178, 173, 158, 134, 147, 132, 165, 187,
     94, 100,  85,  67,  56,  53,  82,  84,
     32,  24,  13,   5,  -2,   4,  17,  17,
     13,   9,  -3,  -7,  -7,  -8,   3,  -1,
      4,   7,  -6,   1,   0,  -5,  -1,  -8,
     13,   8,   8,  10,  13,   0,   2,  -7,
      0,   0,   0,   0,   0,   0,   0,   0,
    },
    {
    -58, -38, -13, -28, -31, -27, -63, -99,
    -25,  -8, -25,  -2,  -9, -25, -24, -52,
    -24, -20,  10,   9,  -1,  -9, -19, -41,
    -17,   3,  22,  22,  22,  11,   8, -18,
    -18,  -6,  16,  25,  16,  17,   4, -18,
    -23,  -3,  -1,  15,  10,  -3, -20, -22,
    -42, -20, -10,  -5,  -2, -20, -23, -44,
    -29, -51, -23, -15, -22, -18, -50, -64,
    },
    {
    -14, -21, -11,  -8, -7,  -9, -17, -24,
     -8,  -4,   7, -12, -3, -13,  -4, -14,
      2,  -8,   0,  -1, -2,   6,   0,   4,
     -3,   9,  12,   9, 14,  10,   3,   2,
     -6,   3,  13,  19,  7,  10,  -3,  -9,
    -12,  -3,   8,  10, 13,   3,  -7, -15,
    -14, -18,  -7,  -1,  4,  -9, -15, -27,
    -23,  -9, -23,  -5, -9, -16,  -5, -17,
    },
    {
    13, 10, 18, 15, 12,  12,   8,   5,
    11, 13, 13, 11, -3,   3,   8,   3,
     7,  7,  7,  5,  4,  -3,  -5,  -3,
     4,  3, 13,  1,  2,   1,  -1,   2,
     3,  5,  8,  4, -5,  -6,  -8, -11,
    -4,  0, -5, -1, -7, -12,  -8, -16,
    -6, -6,  0,  2, -9,  -9, -11,  -3,
    -9,  2,  3, -1, -5, -13,   4, -20,
    },
    {
     -9,  22,  22,  27,  27,  19,  10,  20,
    -17,  20,  32,  41,  58,  25,  30,   0,
    -20,   6,   9,  49,  47,  35,  19,   9,
      3,  22,  24,  45,  57,  40,  57,  36,
    -18,  28,  19,  47,  31,  34,  39,  23,
    -16, -27,  15,   6,   9,  17,  10,   5,
    -22, -23, -30, -16, -16, -23, -36, -32,
    -33, -28, -22, -43,  -5, -32, -20, -41,
    },
    {
    -74, -35, -18, -18, -11,  15,   4, -17,
    -12,  17,  14,  17,  17,  38,  23,  11,
     10,  17,  23,  15,  20,  45,  44,  13,
     -8,  22,  24,  27,  26,  33,  26,   3,
    -18,  -4,  21,  24,  27,  23,   9, -11,
    -19,  -3,  11,  21,  23,  16,   7,  -9,
    -27, -11,   4,  13,  14,   4,  -5, -17,
    -53, -34, -21, -11, -28, -14, -24, -43
    }

};
    // Evaluation Weights
    int[] piece_values_mid = { 82, 337, 365, 477, 1025, 0 };
    int[] piece_values_end = { 94, 281, 297, 512, 936, 0 };
    int[] game_phase_inc = { 0, 1, 1, 2, 4, 0, };
    int[] mobility_weights = { 17, 5, 6, 5, 4, 6 };

    Move[] ttMove = new Move[16777216];

    // Variables for search
    int rfpMargin = 80;
    int futilityMargin = 337;
    int mateScore = -20000;
    int infinity = 30000;
    int hardBoundTimeRatio = 10;
    int softBoundTimeRatio = 40;


    public Move Think(Board board, Timer timer)
    {
        // Killer moves, 1 for each depth
        Move[] killers = new Move[4096];

        int globalDepth = 1; // To be incremented for each iterative loop
        ulong nodes; // To keep track of searched positions in 1 iterative loop
        Move rootBestMove = Move.NullMove;

        // Evaluation
        int Eval()
        {
            int turn = board.IsWhiteToMove ? 1 : -1;
            int mg_score = 0;
            int eg_score = 0;
            int game_phase = 0;
            int mg_phase = 0;
            int eg_phase = 0;
            int tempo = 15;
            foreach (var pl in board.GetAllPieceLists())
            {
                foreach (var piece in pl)
                {

                    int piece_type = (int)piece.PieceType - 1;
                    int color = pl.IsWhitePieceList ? 1 : -1;
                    bool color_bool = pl.IsWhitePieceList;
                    game_phase += game_phase_inc[piece_type];
                    int color_sq = (piece.Square.Rank * 8 + piece.Square.File) ^ (pl.IsWhitePieceList ? 56 : 0); // Flip sqauare for white
                    mg_score += color * (mg_pst_values[piece_type, color_sq] + piece_values_mid[piece_type] + GetNumberOfSetBits(GetPieceAttacks(piece.PieceType, piece.Square, board, color_bool)) * mobility_weights[piece_type]); // PST + Material
                    eg_score += color * (eg_pst_values[piece_type, color_sq] + piece_values_end[piece_type] + GetNumberOfSetBits(GetPieceAttacks(piece.PieceType, piece.Square, board, color_bool)) * mobility_weights[piece_type]);

                }
            }


            // Tapered eval
            mg_phase = game_phase;
            if (mg_phase > 24)
                mg_phase = 24;
            eg_phase = 24 - mg_phase;


            return tempo + turn * (((mg_score * mg_phase) + (eg_score * eg_phase)) / 24);
        }

        // Quiescence Search
        int qSearch(int alpha, int beta)
        {
            // Hard bound time management
            if (timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / hardBoundTimeRatio) throw null;

            int standPat = Eval();

            // Key to index TT
            int key = (int)(board.ZobristKey % 16777216);
            int bestScore = standPat;

            // Terminal nodes
            if (board.IsInCheckmate())
                return mateScore + board.PlyCount;
            if (board.IsDraw())
                return 0;

            // Standing Pat Pruning
            if (standPat >= beta)
                return standPat;

            if (alpha < standPat)
                alpha = standPat;

            // TT + MVV-LVA ordering
            foreach (Move move in board.GetLegalMoves(true).OrderByDescending(move => (ttMove[key] == move, move.CapturePieceType, 0 - move.MovePieceType)))
            {
                nodes++;
                board.MakeMove(move);
                int score = -qSearch(-beta, -alpha);
                board.UndoMove(move);

                if (score > alpha)
                    alpha = score;

                if (score > bestScore)
                {
                    ttMove[key] = move;
                    bestScore = score;
                }

                // A/B pruning
                if (score >= beta)
                    break;

            }

            return bestScore;
        }

        // Fail-Soft Negamax Search
        int search(int depth, int ply, int alpha, int beta)
        {

            // Hard time limit
            if (depth > 1 && timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / hardBoundTimeRatio) throw null;

            bool isRoot = ply == 0;
            bool nonPv = alpha + 1 == beta;

            // Terminal nodes
            if (board.IsInCheckmate() && !isRoot)
                return mateScore + board.PlyCount;
            if (board.IsDraw() && !isRoot)
                return 0;

            // Start Quiescence Search
            if (depth < 1)
                return qSearch(alpha, beta);

            // Static eval needed for RFP and NMP
            int eval = Eval();

            // Key to index TT
            int key = (int)(board.ZobristKey & 16777215);

            // Index for killers
            int killerIndex = ply & 4095;

            // Budget Internal Iterative Reductions
            if (ttMove[key] == Move.NullMove)
                depth--;

            // Reverse futility pruning
            if (eval - rfpMargin * depth >= beta && !board.IsInCheck()) return eval;

            // Null move pruning
            if (eval >= beta && board.TrySkipTurn())
            {
                eval = -search(depth - 4, ply + 1, 1 - beta, -beta);
                board.UndoSkipTurn();
                if (eval >= beta) return eval;
            }

            int bestScore = -30000;
            int moveCount = 0;

            // orderVariable(priority)
            // TT(0),  MVV-LVA ordering(1),  Killer Moves(2)
            foreach (Move move in board.GetLegalMoves().OrderByDescending(move => (ttMove[key] == move, move.CapturePieceType, killers[killerIndex] == move, 0 - move.MovePieceType)))
            {
                moveCount++;
                nodes++;
                int moveExtension = 0;
                board.MakeMove(move);

                // Check extension
                if (board.IsInCheck()) moveExtension++;

                int score = 0;

                // Principle variation search
                if (moveCount == 1)
                {
                    score = -search(depth - 1 + moveExtension, ply + 1, -beta, -alpha);
                }
                else
                {

                    score = -search(depth - 1 + moveExtension, ply + 1, -alpha - 1, -alpha);

                    if (score > alpha && score < beta)
                    {
                        score = -search(depth - 1 + moveExtension, ply + 1, -beta, -alpha);
                    }
                }

                board.UndoMove(move);

                // Updating stuff
                if (score > alpha)
                    alpha = score;

                if (score > bestScore)
                {
                    bestScore = score;
                    ttMove[key] = move;
                    if (isRoot)
                        rootBestMove = move;
                }

                // A/B pruning
                if (score >= beta)
                {
                    // Keep track of the first killers for each ply
                    if ((killers[killerIndex] == Move.NullMove) && !move.IsCapture)
                        killers[killerIndex] = move;
                    break;
                }


                // Futility pruning
                if (nonPv && depth == 1 && !move.IsCapture && (eval + futilityMargin < alpha))
                    break;

            }
            return bestScore;
        }

        try
        {
            // Soft time limit
            for (; timer.MillisecondsElapsedThisTurn < timer.MillisecondsRemaining / softBoundTimeRatio; ++globalDepth)
            {
                int alpha = -infinity;
                int beta = -alpha;
                killers = new Move[4096];

                nodes = 0;

                int score = search(globalDepth, 0, alpha, beta);

                bool isMateScore = score > -mateScore - 500 || score < mateScore + 500;

                Console.WriteLine($"info depth {globalDepth} time {timer.MillisecondsElapsedThisTurn} nodes {nodes} nps {Convert.ToInt32(1000 * nodes / ((ulong)timer.MillisecondsElapsedThisTurn + 0.001))} score cp {score} pv {ChessChallenge.Chess.MoveUtility.GetMoveNameUCI(new(rootBestMove.RawValue))}");

                if (isMateScore) break; // Give up when mate is inevitable or stop searching when mate is found
            }
        }
        catch { }
        return rootBestMove;
    }
}