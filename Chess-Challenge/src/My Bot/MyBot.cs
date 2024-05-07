using System;
using System.Linq;
using ChessChallenge.API;
using static ChessChallenge.API.BitboardHelper;

/*
| Throttle - A c# UCI chess engine | SSS Version
  --------------------------------
Version: 2.5

* Feature elo gain after 1.4
** Feature added at version after 1.4
Features:
    
    Search:
        - Budget aspiration window **2.4 *16.6 +/- 9.6
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


    // PeSTO's Psqts

    int[,] pst = {
    {
        0, 0, 0, 0, 0, 0, 0, 0,
        11665506, 11337862, 10354749, 8781919, 9633860, 8650878, 10813474, 12255221,
        6160378, 6553607, 5570586, 4390943, 3670081, 3473464, 5373977, 5505004,
        2097138, 1572877, 851974, 327701, -131049, 262156, 1114129, 1114089,
        851941, 589822, -196613, -458740, -458735, -524282, 196618, -65561,
        262118, 458748, -393220, 65526, 3, -327677, -65503, -524300,
        851933, 524287, 524268, 655337, 851953, 24, 131110, -458774,
        0, 0, 0, 0, 0, 0, 0, 0,

    },
    {
        -3801255, -2490457, -852002, -1835057, -2031555, -1769569, -4128783, -6488171,
        -1638473, -524329, -1638328, -131036, -589801, -1638338, -1572857, -3407889,
        -1572911, -1310660, 655397, 589889, -65452, -589695, -1245111, -2686932,
        -1114121, 196625, 1441811, 1441845, 1441829, 720965, 524306, -1179626,
        -1179661, -393212, 1048592, 1638413, 1048604, 1114131, 262165, -1179656,
        -1507351, -196617, -65524, 983050, 655379, -196591, -1310695, -1441808,
        -2752541, -1310773, -655372, -327683, -131073, -1310702, -1507342, -2883603,
        -1900649, -3342357, -1507386, -983073, -1441809, -1179676, -3276819, -4194327,
    },
    {
        -917533, -1376252, -720978, -524325, -458777, -589866, -1114105, -1572872,
        -524314, -262128, 458734, -786445, -196578, -851909, -262126, -917551,
        131056, -524251, 43, -65496, -131037, 393266, 37, 262142,
        -196612, 589829, 786451, 589874, 917541, 655397, 196615, 131070,
        -393222, 196621, 851981, 1245210, 458786, 655372, -196598, -589820,
        -786432, -196593, 524303, 655375, 851982, 196635, -458734, -983030,
        -917500, -1179633, -458736, -65536, 262151, -589803, -983007, -1769471,
        -1507361, -589827, -1507342, -327701, -589837, -1048588, -327719, -1114133,
    },
    {
        852000, 655402, 1179680, 983091, 786495, 786441, 524319, 327723,
        720923, 852000, 852026, 720958, -196528, 196675, 524314, 196652,
        458747, 458771, 458778, 327716, 262161, -196563, -327619, -196592,
        262120, 196597, 851975, 65562, 131096, 65571, -65544, 131052,
        196572, 327654, 524276, 262143, -327671, -393223, -524282, -720919,
        -262189, -25, -327696, -65553, -458749, -786432, -524293, -1048609,
        -393260, -393232, -20, 131063, -589825, -589813, -720902, -196679,
        -589843, 131059, 196609, -65519, -327664, -851961, 262107, -1310746,
    },
    {
        -589852, 1441792, 1441821, 1769484, 1769531, 1245228, 655403, 1310765,
        -1114136, 1310681, 2097147, 2686977, 3801072, 1638457, 1966108, 54,
        -1310733, 393199, 589831, 3211272, 3080221, 2293816, 1245231, 589881,
        196581, 1441765, 1572848, 2949104, 3735551, 2621457, 3735550, 2359297,
        -1179657, 1834982, 1245175, 3080182, 2031614, 2228220, 2555907, 1507325,
        -1048590, -1769470, 983029, 393214, 589819, 1114114, 655374, 327685,
        -1441827, -1507336, -1966069, -1048574, -1048568, -1507313, -2359299, -2097151,
        -2162689, -1835026, -1441801, -2818038, -327695, -2097177, -1310751, -2687026,
    },
    {
        -4849729, -2293737, -1179632, -1179663, -720952, 983006, 262146, -1114099,
        -786403, 1114111, 917484, 1114105, 1114104, 2490364, 1507290, 720867,
        655351, 1114136, 1507330, 983024, 1310700, 2949126, 2883606, 851946,
        -524305, 1441772, 1572852, 1769445, 1703906, 2162663, 1703922, 196572,
        -1179697, -262145, 1376229, 1572825, 1769426, 1507284, 589791, -720947,
        -1245198, -196622, 720874, 1376210, 1507284, 1048546, 458737, -589851,
        -1769471, -720889, 262136, 851904, 917461, 262128, -327671, -1114104,
        -3473423, -2228188, -1376244, -720950, -1835000, -917532, -1572840, -2818034,
    }
};

    // Evaluation Weights
    int tempo = 10;
    int[] piece_values = { 6160466, 18415953, 19464557, 33554909, 61342721, 0 };
    int[] game_phase_inc = { 0, 1, 1, 2, 4, 0, };
    int[] mobility_weights = { 1114129, 327685, 393222, 327685, 262148, 393222 };

    // this tuple is 24 bytes, so the transposition table is precisely 192MiB (~201 MB)
    readonly (
        ulong, // hash
        ushort, // moveRaw
        int, // score
        int, // depth
        int // bound BOUND_EXACT=[1, 2147483647), BOUND_LOWER=2147483647, BOUND_UPPER=0
    )[] transpositionTable = new (ulong, ushort, int, int, int)[0x800000];

    // Variables for search
    int rfpMargin = 65;
    int futilityMargin = 337;
    int mateScore = -20000;
    int infinity = 30000;
    int hardBoundTimeRatio = 10;
    int softBoundTimeRatio = 40;
    int lastScore = 0;


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
            int mg_score = 0;
            int eg_score = 0;
            int turn = board.IsWhiteToMove ? 1 : -1;
            int piece_mobility;

            foreach (bool isWhite in new[] { !board.IsWhiteToMove, board.IsWhiteToMove })
            {
                mg_score = -mg_score;
                eg_score = -eg_score;

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

                        piece_mobility = GetNumberOfSetBits(GetPieceAttacks((PieceType)pieceIndex, new Square(sq), board, isWhite));

                        if (isWhite) sq ^= 56;
                        phase += game_phase_inc[pieceIndex - 1];

                        mg_score += unpack_mg(pst[pieceIndex - 1, sq]) + unpack_mg(piece_values[pieceIndex - 1]) + piece_mobility * unpack_mg(mobility_weights[pieceIndex - 1]); // PST + Material
                        eg_score += unpack_eg(pst[pieceIndex - 1, sq]) + unpack_eg(piece_values[pieceIndex - 1]) + piece_mobility * unpack_eg(mobility_weights[pieceIndex - 1]);
                    }
                }
            }

            // Tapered eval
            if (phase > 24)
                phase = 24;
            int eg_phase = 24 - phase;


            return tempo + ((mg_score * phase) + (eg_score * eg_phase)) / 24;

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

            return lastScore = bestScore;
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

            // Key to index TT
            int key = (int)(board.ZobristKey & 16777215);

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
                if (nonPv && depth <= 4 && !move.IsCapture && (eval + futilityMargin * depth < alpha))
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

            return lastScore = bestScore;
        }

        try
        {
            nodes = 0;

            // Soft time limit
            for (; timer.MillisecondsElapsedThisTurn < timer.MillisecondsRemaining / softBoundTimeRatio; ++globalDepth)
            {
                int alpha = -infinity;
                int beta = -alpha;
                int score;
                killers = new Move[4096];

                if (Math.Abs(lastScore - (score = search(globalDepth, 0, lastScore - 41, lastScore + 41))) >= 41)
                    score = search(globalDepth, 0, alpha, beta);

                Console.WriteLine($"info depth {globalDepth} time {timer.MillisecondsElapsedThisTurn} nodes {nodes} nps {Convert.ToInt32(1000 * nodes / ((ulong)timer.MillisecondsElapsedThisTurn + 0.001))} score cp {score} pv {ChessChallenge.Chess.MoveUtility.GetMoveNameUCI(new(rootBestMove.RawValue))}");
            }
        }
        catch { }
        return rootBestMove;
    }
}