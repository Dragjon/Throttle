using System;
using System.Linq;
using ChessChallenge.API;
using static ChessChallenge.API.BitboardHelper;

/*
| Throttle - A c# UCI chess engine | SSS Version
  --------------------------------
Version: 2.9

* Feature elo gain after 1.4
** Feature added at version after 1.4
Features:
    
    Search:
        - Aspiration window **2.4 *16.6 +/- 9.6
        - Fail-Soft Negamax Search
        - Principle Variation Search
            - Triple PVS **1.5 *21.3 +/- 11.1,
        - Quiescence search
        - Pruning:
            - Actual TT pruning **v2.1 *136.4 +/- 31.2
            - A/B Pruning
            - Null Move Pruning
            - Reverse Futility Pruning
            - Futility Pruning
            - Quiescence Search Standing Pat Pruning **v2.5 *11.7 +/- 7.7
            - Quiscence search delta pruning 
        - Reductions:
            - Budget Internal Iterative Reduction
        - Extensions:
            - Check Extensions
        - Ordering:
            - TT Moves
            - MVV-LVA (for good captures and quiets)
            - Killer moves (quiets)
            - History moves (quiets) **v2.0 *67.9 +/- 20.9
        - Time management:
            - Hard and Soft time management

    Evaluation:
        - SWAR compressed eval **v2.2 *5.7 +/- 4.6
        - Material values (PeSTO)
        - Piece square tables (PeSTO)
        - Tapered Eval
        - Tempo
        - Mobility
    
*/

/*
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

v2.6 - Correct ASP window search
Score of ASP vs Original: 3391 - 3179 - 2721  [0.511] 9291
...      ASP playing White: 2084 - 1259 - 1304  [0.589] 4647
...      ASP playing Black: 1307 - 1920 - 1417  [0.434] 4644
...      White vs Black: 4004 - 2566 - 2721  [0.577] 9291
Elo difference: 7.9 +/- 5.9, LOS: 99.6 %, DrawRatio: 29.3 %
SPRT: llr 2.95 (100.3%), lbound -2.94, ubound 2.94 - H1 was accepted

v2.7 - Tuned eval + changed mobility
(BLUESCREENED)
Around 7 elo

v2.8 - Speed up evaluation - extract only at the end
Score of SpeedUp vs Original: 902 - 733 - 544  [0.539] 2179
...      SpeedUp playing White: 538 - 284 - 268  [0.617] 1090
...      SpeedUp playing Black: 364 - 449 - 276  [0.461] 1089
...      White vs Black: 987 - 648 - 544  [0.578] 2179
Elo difference: 27.0 +/- 12.6, LOS: 100.0 %, DrawRatio: 25.0 %
SPRT: llr 2.95 (100.1%), lbound -2.94, ubound 2.94 - H1 was accepted

v2.9 - RFP changed to 55;
...      RFP55 playing White: 3188 - 1958 - 1745  [0.589] 6891
...      RFP55 playing Black: 2148 - 3124 - 1617  [0.429] 6889
...      White vs Black: 6312 - 4106 - 3362  [0.580] 13780
Elo difference: 6.4 +/- 5.0, LOS: 99.4 %, DrawRatio: 24.4 %
SPRT: llr 2.95 (100.1%), lbound -2.94, ubound 2.94 - H1 was accepted
*/
public class MyBot : IChessBot
{
    static int S(Int16 mg, Int16 eg)
    {
        return ((int)eg << 16) + (int)mg;
    }


    static Int16 unpack_mg(int packed)
    {
        return (Int16)packed;
    }

    static Int16 unpack_eg(int packed)
    {
        return (Int16)((packed + 0x8000) >> 16);
    }


    int[,] pst = {
    {
        S(0, 0), S(0, 0), S(0, 0), S(0, 0), S(0, 0), S(0, 0), S(0, 0), S(0, 0),
        S(74, 178), S(97, 171), S(74, 171), S(104, 122), S(87, 117), S(70, 128), S(1, 174), S(-25, 189),
        S(-7, 117), S(7, 126), S(41, 90), S(46, 69), S(49, 60), S(72, 45), S(52, 93), S(8, 91),
        S(-23, 45), S(3, 34), S(6, 14), S(8, 6), S(30, -4), S(21, -1), S(25, 19), S(1, 19),
        S(-33, 20), S(-5, 17), S(-6, -1), S(10, -4), S(11, -6), S(3, -5), S(11, 7), S(-12, -0),
        S(-35, 13), S(-10, 16), S(-10, -2), S(-8, 10), S(7, 3), S(-5, -0), S(26, 5), S(-5, -4),
        S(-34, 18), S(-9, 20), S(-13, 6), S(-23, 13), S(-3, 17), S(12, 5), S(35, 5), S(-12, -3),
        S(0, 0), S(0, 0), S(0, 0), S(0, 0), S(0, 0), S(0, 0), S(0, 0), S(0, 0),
                },
            {
        S(-162, -72), S(-129, -13), S(-62, 1), S(-28, -7), S(5, -4), S(-54, -28), S(-110, -6), S(-103, -97),
        S(-26, -20), S(-9, 2), S(17, 11), S(33, 10), S(17, 1), S(81, -14), S(-10, -2), S(13, -36),
        S(-12, -4), S(23, 13), S(40, 29), S(52, 30), S(89, 14), S(90, 8), S(47, 1), S(14, -15),
        S(-16, 9), S(-3, 30), S(22, 41), S(44, 43), S(26, 44), S(50, 38), S(8, 29), S(18, 1),
        S(-30, 11), S(-14, 19), S(1, 43), S(2, 44), S(11, 47), S(6, 36), S(4, 21), S(-19, 1),
        S(-49, -5), S(-26, 14), S(-12, 24), S(-9, 37), S(2, 36), S(-8, 20), S(-5, 9), S(-32, -5),
        S(-63, -15), S(-50, 1), S(-33, 11), S(-21, 15), S(-20, 14), S(-18, 8), S(-32, -9), S(-35, -5),
        S(-107, -22), S(-52, -35), S(-65, -5), S(-51, -2), S(-46, -0), S(-33, -13), S(-49, -28), S(-76, -36),
        },
            {
        S(-42, 9), S(-58, 18), S(-49, 17), S(-93, 29), S(-80, 24), S(-62, 14), S(-34, 10), S(-70, 4),
        S(-25, -7), S(-1, 14), S(-7, 18), S(-25, 22), S(6, 11), S(5, 12), S(-4, 18), S(-14, -8),
        S(-15, 23), S(9, 18), S(9, 30), S(33, 17), S(19, 24), S(52, 24), S(29, 17), S(15, 16),
        S(-23, 18), S(-10, 35), S(13, 30), S(23, 46), S(21, 35), S(17, 34), S(-9, 31), S(-22, 18),
        S(-31, 14), S(-17, 33), S(-11, 41), S(10, 37), S(7, 38), S(-9, 35), S(-16, 29), S(-22, 4),
        S(-19, 13), S(-13, 25), S(-12, 32), S(-9, 33), S(-8, 36), S(-13, 33), S(-11, 16), S(-6, 3),
        S(-17, 8), S(-16, 8), S(-5, 7), S(-27, 22), S(-19, 23), S(-6, 12), S(0, 15), S(-13, -12),
        S(-40, -10), S(-19, 9), S(-35, -12), S(-44, 10), S(-39, 7), S(-40, 7), S(-15, -6), S(-29, -23),
        },
            {
        S(34, 25), S(24, 34), S(30, 42), S(36, 38), S(55, 29), S(73, 18), S(53, 21), S(76, 15),
        S(14, 26), S(14, 37), S(33, 42), S(53, 33), S(39, 33), S(68, 18), S(56, 14), S(87, 0),
        S(-7, 26), S(15, 29), S(16, 30), S(19, 28), S(48, 15), S(49, 9), S(88, 0), S(65, -5),
        S(-23, 28), S(-8, 26), S(-7, 36), S(1, 32), S(8, 16), S(9, 10), S(18, 7), S(20, 0),
        S(-42, 21), S(-40, 26), S(-30, 28), S(-16, 26), S(-17, 22), S(-32, 20), S(-8, 6), S(-16, 1),
        S(-49, 16), S(-39, 16), S(-31, 15), S(-32, 21), S(-26, 16), S(-28, 8), S(7, -13), S(-15, -13),
        S(-52, 11), S(-39, 14), S(-25, 15), S(-28, 18), S(-23, 9), S(-21, 5), S(-3, -6), S(-34, 0),
        S(-33, 6), S(-31, 15), S(-21, 24), S(-16, 22), S(-12, 14), S(-22, 9), S(-7, 5), S(-31, -3),
        },
            {
        S(-39, 24), S(-33, 39), S(-1, 56), S(33, 42), S(32, 40), S(37, 32), S(57, -14), S(4, 17),
        S(-1, -12), S(-24, 33), S(-18, 68), S(-26, 86), S(-19, 104), S(19, 62), S(-2, 44), S(43, 18),
        S(-1, -1), S(-4, 18), S(-6, 61), S(11, 63), S(15, 78), S(57, 57), S(59, 18), S(57, 5),
        S(-18, 10), S(-14, 34), S(-10, 50), S(-11, 72), S(-9, 87), S(5, 72), S(4, 56), S(11, 34),
        S(-17, 8), S(-19, 38), S(-19, 46), S(-11, 67), S(-12, 64), S(-13, 55), S(-2, 36), S(1, 23),
        S(-19, -4), S(-12, 14), S(-17, 37), S(-18, 33), S(-15, 37), S(-8, 30), S(5, 7), S(-1, -6),
        S(-21, -9), S(-15, -4), S(-5, -8), S(-5, 2), S(-7, 5), S(2, -21), S(8, -51), S(19, -81),
        S(-24, -14), S(-34, -7), S(-27, -4), S(-11, -13), S(-20, -8), S(-34, -9), S(-9, -41), S(-18, -40),
        },
            {
        S(36, -103), S(11, -51), S(46, -42), S(-101, 10), S(-44, -11), S(7, -8), S(62, -17), S(157, -123),
        S(-88, -7), S(-45, 23), S(-89, 36), S(20, 16), S(-33, 38), S(-29, 50), S(12, 40), S(-11, 7),
        S(-107, 9), S(-2, 28), S(-71, 48), S(-91, 59), S(-50, 58), S(28, 50), S(8, 49), S(-29, 19),
        S(-74, -2), S(-86, 34), S(-101, 52), S(-148, 65), S(-135, 65), S(-96, 58), S(-95, 50), S(-120, 24),
        S(-65, -14), S(-78, 18), S(-108, 43), S(-136, 58), S(-134, 57), S(-96, 44), S(-100, 31), S(-125, 14),
        S(-21, -25), S(-6, -0), S(-64, 21), S(-77, 34), S(-71, 33), S(-68, 24), S(-21, 4), S(-38, -8),
        S(70, -47), S(27, -19), S(12, -6), S(-23, 5), S(-24, 9), S(-6, -1), S(44, -20), S(54, -39),
        S(65, -83), S(89, -63), S(62, -43), S(-42, -24), S(25, -50), S(-16, -26), S(69, -53), S(70, -83)
        }
};

    // Evaluation Weights
    int tempo = 10;
    int bishopPair = S(20, 66);
    int[] piece_values = { S(68, 116), S(295, 349), S(310, 353), S(394, 643), S(815, 1219), S(0, 0) };
    int[] game_phase_inc = { 0, 1, 1, 2, 4, 0, };
    int[] mobility_weights = { S(16, 17), S(4, 5), S(5, 6), S(4, 5), S(3, 4), S(5, 6), };

    // this tuple is 24 bytes, so the transposition table is precisely 192MiB (~201 MB)
    readonly (
        ulong, // hash
        ushort, // moveRaw
        int, // score
        int, // depth
        int // bound BOUND_EXACT=[1, 2147483647), BOUND_LOWER=2147483647, BOUND_UPPER=0
    )[] transpositionTable = new (ulong, ushort, int, int, int)[0x800000];

    // Variables for search
    int rfpMargin = 55;
    int futilityMargin = 337;
    int mateScore = -20000;
    int infinity = 30000;
    int hardBoundTimeRatio = 10;
    int softBoundTimeRatio = 40;


    public Move Think(Board board, Timer timer)
    {
        // Killer moves, 1 for each depth
        Move[] killers = new Move[4096];

        // History moves
        int[] history = new int[4096];

        int globalDepth = 1; // To be incremented for each iterative loop
        ulong nodes; // To keep track of searched positions in 1 iterative loop
        Move rootBestMove = Move.NullMove;

        // Evaluation
        int Eval()
        {
            int phase = 0;
            int score = 0;
            int piece_mobility;

            foreach (bool isWhite in new[] { !board.IsWhiteToMove, board.IsWhiteToMove })
            {
                score = -score;

                //       None (skipped)               King
                for (var pieceIndex = 0; ++pieceIndex <= 6;)
                {
                    var bitboard = board.GetPieceBitboard((PieceType)pieceIndex, isWhite);

                    // This and the following line is an efficient way to loop over each piece of a certain type.
                    // Instead of looping each square, we can skip empty squares by looking at a bitboard of each piece,
                    // and incrementally removing squares from it. More information: https://www.chessprogramming.org/Bitboards
                    while (bitboard != 0)
                    {
                        var sq = ClearAndGetIndexOfLSB(ref bitboard);

                        if (pieceIndex == 3 && bitboard != 0)
                        {
                            score += bishopPair;
                        }

                        piece_mobility = GetNumberOfSetBits(GetPieceAttacks((PieceType)pieceIndex, new Square(sq), board, isWhite));

                        if (isWhite) sq ^= 56;
                        phase += game_phase_inc[pieceIndex - 1];

                        score += pst[pieceIndex - 1, sq] + piece_values[pieceIndex - 1] + piece_mobility * mobility_weights[pieceIndex - 1]; // PST + Material
                    }
                }
            }

            // Tapered eval
            if (phase > 24)
                phase = 24;
            int eg_phase = 24 - phase;

            int mg_score = unpack_mg(score);
            int eg_score = unpack_eg(score);

            return tempo + ((mg_score * phase) + (eg_score * eg_phase)) / 24;

        }

        // Quiescence Search
        int qSearch(int alpha, int beta)
        {
            // Hard bound time management
            if (timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / hardBoundTimeRatio) throw null;

            int standPat = Eval();

            int bestScore = standPat;

            // Terminal nodes
            if (board.IsInCheckmate())
                return mateScore + board.PlyCount;
            if (board.IsDraw())
                return 0;

            ref var tt = ref transpositionTable[board.ZobristKey & 0x7FFFFF];
            var (ttHash, ttMoveRaw, score, ttDepth, ttBound) = tt;

            bool ttHit = ttHash == board.ZobristKey;
            int oldAlpha = alpha;

            if (ttHit)
            {
                if (ttBound switch
                {
                    2147483647 /* BOUND_LOWER */ => score >= beta,
                    0 /* BOUND_UPPER */ => score <= alpha,
                    // exact cutoffs at pv nodes causes problems, but need it in qsearch for matefinding
                    _ /* BOUND_EXACT */ => true,
                })
                    return score;
            }

            // Standing Pat Pruning
            if (standPat >= beta)
                return standPat;

            if (alpha < standPat)
                alpha = standPat;

            Move bestMove = Move.NullMove;

            // TT + MVV-LVA ordering
            foreach (Move move in board.GetLegalMoves(true).OrderByDescending(move => ttHit && move.RawValue == ttMoveRaw ? 9_000_000_000_000_000_000
                                          : 1_000_000_000_000_000_000 * (long)move.CapturePieceType - (long)move.MovePieceType))
            {
                if (standPat + (0b1_0100110100_1011001110_0110111010_0110000110_0010110100_0000000000 >> (int)move.CapturePieceType * 10 & 0b1_11111_11111) <= alpha)
                    break;
                nodes++;
                board.MakeMove(move);
                score = -qSearch(-beta, -alpha);
                board.UndoMove(move);

                if (score > alpha)
                    alpha = score;

                if (score > bestScore)
                {
                    bestMove = move;
                    bestScore = score;
                }

                // A/B pruning
                if (score >= beta)
                    break;

            }

            tt = (
            board.ZobristKey,
                    alpha > oldAlpha // don't update best move if upper bound
                    ? bestMove.RawValue
                    : ttMoveRaw,
                    Math.Clamp(bestScore, -20000, 20000),
                    0,
                    bestScore >= beta
                    ? 2147483647 /* BOUND_LOWER */
                    : alpha - oldAlpha /* BOUND_UPPER if alpha == oldAlpha else BOUND_EXACT */
            );

            return bestScore;
        }

        // Fail-Soft Negamax Search
        int search(int depth, int ply, int alpha, int beta)
        {

            // Hard time limit
            if (depth > 1 && timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / hardBoundTimeRatio) throw null;

            bool isRoot = ply == 0;
            bool nonPv = alpha + 1 >= beta;

            ref var tt = ref transpositionTable[board.ZobristKey & 0x7FFFFF];
            var (ttHash, ttMoveRaw, score, ttDepth, ttBound) = tt;

            bool ttHit = ttHash == board.ZobristKey;
            int oldAlpha = alpha;

            // Terminal nodes
            if (board.IsInCheckmate() && !isRoot)
                return mateScore + board.PlyCount;
            if (board.IsDraw() && !isRoot)
                return 0;

            if (nonPv && ttHit)
            {
                if (ttDepth >= depth && ttBound switch
                {
                    2147483647 /* BOUND_LOWER */ => score >= beta,
                    0 /* BOUND_UPPER */ => score <= alpha,
                    // exact cutoffs at pv nodes causes problems, but need it in qsearch for matefinding
                    _ /* BOUND_EXACT */ => true,
                })
                    return score;
            }
            else if (nonPv && depth > 3)
                // Internal iterative reduction
                depth--;

            // Start Quiescence Search
            if (depth < 1)
                return qSearch(alpha, beta);

            // Static eval needed for RFP and NMP
            int eval = Eval();

            // Index for killers
            int killerIndex = ply & 4095;

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

            Move bestMove = Move.NullMove;
            Move[] legals = board.GetLegalMoves();
            foreach (Move move in legals.OrderByDescending(move => ttHit && move.RawValue == ttMoveRaw ? 9_000_000_000_000_000_000
                                          : move.IsCapture ? 1_000_000_000_000_000_000 * (long)move.CapturePieceType - (long)move.MovePieceType
                                          : move == killers[killerIndex] ? 500_000_000_000_000_000
                                          : history[move.RawValue & 4095]))
            {
                moveCount++;

                nodes++;

                int reduction = moveCount > 3 && nonPv && !move.IsCapture ? 1 : 0;

                if (nonPv && depth <= 4 && !move.IsCapture && (eval + futilityMargin * depth < alpha))
                    reduction++;

                board.MakeMove(move);

                // Check extension
                int moveExtension = board.IsInCheck() ? 1 : 0;

                score = 0;


                // Principle variation search
                if (moveCount == 1)
                {
                    score = -search(depth - 1 + moveExtension, ply + 1, -beta, -alpha);
                }
                else
                {
                    score = -search(depth - 1 + moveExtension - reduction, ply + 1, -alpha - 1, -alpha);

                    if (reduction > 0 && score > alpha)
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
                    bestMove = move;
                    if (isRoot)
                        rootBestMove = move;

                    // A/B pruning
                    if (score >= beta)
                    {

                        if (!move.IsCapture)
                        {
                            history[move.RawValue & 4095] += depth * depth;

                            // Keep track of the first killers for each ply
                            if (killers[killerIndex] == Move.NullMove)
                                killers[killerIndex] = move;
                        }
                        break;
                    }
                }


                // Extended futility pruning
                if (nonPv && depth <= 4 && !move.IsCapture && (eval + futilityMargin * depth < alpha) && bestScore > mateScore + 100)
                    break;

            }

            tt = (
                    board.ZobristKey,
                    alpha > oldAlpha // don't update best move if upper bound
                    ? bestMove.RawValue
                    : ttMoveRaw,
                    Math.Clamp(bestScore, -20000, 20000),
                    depth,
                    bestScore >= beta
                    ? 2147483647 /* BOUND_LOWER */
                    : alpha - oldAlpha /* BOUND_UPPER if alpha == oldAlpha else BOUND_EXACT */
            );

            return bestScore;
        }

        try
        {

            nodes = 0;

            int score = 0;

            // Soft time limit
            for (; timer.MillisecondsElapsedThisTurn < timer.MillisecondsRemaining / softBoundTimeRatio; ++globalDepth)
            {
                int alpha = -infinity;
                int beta = infinity;

                int delta = 0;

                if (globalDepth > 3)
                {
                    delta = 20;
                    alpha = score - delta;
                    beta = score + delta;
                }

                killers = new Move[4096];

                int newScore;

                while (true)
                {
                    newScore = search(globalDepth, 0, alpha, beta);

                    if (newScore <= alpha)
                    {
                        beta = (newScore + beta) / 2;
                        alpha = Math.Max(newScore - delta, -infinity);

                        Console.WriteLine($"info depth {globalDepth} time {timer.MillisecondsElapsedThisTurn} nodes {nodes} nps {Convert.ToInt32(1000 * nodes / ((ulong)timer.MillisecondsElapsedThisTurn + 0.001))} score cp {alpha} upperbound pv {ChessChallenge.Chess.MoveUtility.GetMoveNameUCI(new(rootBestMove.RawValue))}");
                    }
                    else if (newScore >= beta)
                    {
                        beta = Math.Min(newScore + delta, infinity);

                        Console.WriteLine($"info depth {globalDepth} time {timer.MillisecondsElapsedThisTurn} nodes {nodes} nps {Convert.ToInt32(1000 * nodes / ((ulong)timer.MillisecondsElapsedThisTurn + 0.001))} score cp {beta} lowerbound pv {ChessChallenge.Chess.MoveUtility.GetMoveNameUCI(new(rootBestMove.RawValue))}");
                    }
                    else
                        break;

                    if (delta <= 500)
                        delta += delta;
                    else
                        delta = infinity;
                }

                score = newScore;

                Console.WriteLine($"info depth {globalDepth} time {timer.MillisecondsElapsedThisTurn} nodes {nodes} nps {Convert.ToInt32(1000 * nodes / ((ulong)timer.MillisecondsElapsedThisTurn + 0.001))} score cp {score} pv {ChessChallenge.Chess.MoveUtility.GetMoveNameUCI(new(rootBestMove.RawValue))}");
            }
        }
        catch { }
        return rootBestMove;
    }
}