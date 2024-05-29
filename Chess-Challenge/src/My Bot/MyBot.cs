using System;
using System.Linq;
using ChessChallenge.API;
using static ChessChallenge.API.BitboardHelper;

/*
| Throttle - A c# UCI chess engine | SSS Version
  --------------------------------
Version: 3.2.1

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
            - Mate Distancing pruning **v3.2
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

v3.0 - New futility pruning
Score of NewFP vs Original: 1646 - 1461 - 1002  [0.523] 4109
...      NewFP playing White: 953 - 603 - 499  [0.585] 2055
...      NewFP playing Black: 693 - 858 - 503  [0.460] 2054
...      White vs Black: 1811 - 1296 - 1002  [0.563] 4109
Elo difference: 15.7 +/- 9.2, LOS: 100.0 %, DrawRatio: 24.4 %
SPRT: llr 2.96 (100.5%), lbound -2.94, ubound 2.94 - H1 was accepted

v3.1 - Retuned eval with mobility
Score of ReplacedEval vs Original: 1807 - 1610 - 789  [0.523] 4206
...      ReplacedEval playing White: 1026 - 674 - 404  [0.584] 2104
...      ReplacedEval playing Black: 781 - 936 - 385  [0.463] 2102
...      White vs Black: 1962 - 1455 - 789  [0.560] 4206
Elo difference: 16.3 +/- 9.5, LOS: 100.0 %, DrawRatio: 18.8 %
SPRT: llr 2.96 (100.4%), lbound -2.94, ubound 2.94 - H1 was accepted

v3.2 - King cornering
Partial results
--------------------------------------------------
Results of MDP vs Original2:
Elo: 7.38 +/- 13.86, nElo: 9.08 +/- 17.02
LOS: 85.20 %, DrawRatio: 38.75 %, PairsRatio: 1.07
Games: 1600, Wins: 617, Losses: 583, Draws: 400, Points: 817.0 (51.06 %)
Ptnml(0-2): [86, 151, 310, 149, 104]
LLR: 0.44 (-2.94, 2.94) [0.00, 5.00]
--------------------------------------------------
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


    static int[,] pst = {
        {
                S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0),
                S(39,130), S(68,123), S(45,124), S(69,76), S(56,72), S(35,88), S(-21,127), S(-49,140),
                S(-22,64), S(-9,73), S(19,42), S(23,22), S(27,13), S(46,0), S(31,43), S(-7,38),
                S(-32,-3), S(-9,-13), S(-10,-30), S(-5,-38), S(13,-47), S(4,-44), S(9,-27), S(-12,-27),
                S(-39,-27), S(-16,-28), S(-15,-45), S(-2,-48), S(1,-50), S(-3,-50), S(-1,-37), S(-22,-45),
                S(-36,-34), S(-20,-30), S(-18,-47), S(-13,-37), S(-1,-43), S(-7,-47), S(11,-40), S(-13,-49),
                S(-36,-31), S(-18,-28), S(-25,-41), S(-23,-30), S(-12,-29), S(4,-42), S(20,-41), S(-22,-49),
                S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0)
        },
        {
                S(-125,-72), S(-110,-11), S(-57,1), S(-24,-8), S(9,-7), S(-47,-30), S(-93,-9), S(-80,-93),
                S(-19,-12), S(2,3), S(28,2), S(43,-1), S(21,-8), S(81,-18), S(-5,0), S(19,-32),
                S(-5,0), S(29,5), S(49,22), S(56,23), S(93,7), S(89,3), S(52,-5), S(19,-13),
                S(-5,9), S(9,20), S(31,32), S(53,33), S(32,35), S(56,30), S(19,19), S(28,0),
                S(-18,8), S(-1,12), S(17,32), S(20,32), S(29,34), S(22,25), S(16,13), S(-7,-1),
                S(-33,-8), S(-13,6), S(2,12), S(13,26), S(25,23), S(10,6), S(8,1), S(-16,-7),
                S(-39,-15), S(-29,-1), S(-16,5), S(-1,6), S(-1,6), S(3,0), S(-13,-11), S(-16,-8),
                S(-73,-20), S(-21,-23), S(-26,-7), S(-13,-4), S(-9,-4), S(-6,-14), S(-20,-18), S(-45,-31)
        },
        {
                S(-19,3), S(-43,8), S(-43,4), S(-80,13), S(-65,8), S(-51,-1), S(-26,-3), S(-40,-4),
                S(-9,-10), S(11,-3), S(0,-2), S(-19,1), S(11,-9), S(1,-6), S(5,0), S(-11,-10),
                S(-5,9), S(16,1), S(12,5), S(28,-7), S(15,-1), S(49,1), S(31,-1), S(23,2),
                S(-8,3), S(-2,8), S(16,4), S(25,16), S(24,7), S(16,8), S(-1,4), S(-7,2),
                S(-9,-4), S(-6,6), S(-6,11), S(20,8), S(14,10), S(-3,6), S(-6,3), S(2,-11),
                S(-3,-3), S(4,4), S(7,5), S(0,6), S(5,8), S(7,6), S(8,-1), S(12,-11),
                S(2,-1), S(8,-9), S(10,-13), S(-6,1), S(4,-3), S(17,-8), S(22,-2), S(8,-16),
                S(-2,-9), S(15,2), S(4,-3), S(-4,-2), S(6,-2), S(-5,5), S(17,-15), S(7,-21)
        },
        {
                S(15,7), S(-1,15), S(-3,22), S(-1,17), S(13,9), S(29,6), S(27,4), S(46,-1),
                S(0,10), S(-2,20), S(12,23), S(29,12), S(16,13), S(41,4), S(36,1), S(64,-11),
                S(-17,12), S(2,12), S(-3,13), S(-2,9), S(25,0), S(30,-5), S(72,-11), S(52,-17),
                S(-22,14), S(-15,10), S(-17,18), S(-12,13), S(-9,1), S(-1,-3), S(10,-5), S(14,-8),
                S(-29,6), S(-31,8), S(-25,9), S(-19,7), S(-17,3), S(-25,2), S(-3,-8), S(-12,-10),
                S(-30,1), S(-29,0), S(-22,-1), S(-20,1), S(-12,-5), S(-12,-11), S(15,-28), S(-3,-25),
                S(-30,-4), S(-25,-3), S(-11,-2), S(-11,-1), S(-5,-9), S(-2,-14), S(10,-22), S(-14,-17),
                S(-17,-6), S(-17,-6), S(-11,-2), S(-6,-7), S(-1,-13), S(-3,-10), S(4,-19), S(-14,-19)
        },
        {
                S(-19,-11), S(-32,9), S(-13,23), S(18,6), S(21,6), S(19,13), S(53,-38), S(8,-6),
                S(-2,-14), S(-20,10), S(-17,36), S(-26,51), S(-21,67), S(11,35), S(4,19), S(40,13),
                S(-2,-9), S(-6,-2), S(-11,28), S(-3,34), S(1,46), S(45,29), S(51,1), S(53,-1),
                S(-14,-3), S(-15,6), S(-14,17), S(-15,31), S(-14,44), S(-2,35), S(4,30), S(11,21),
                S(-13,-9), S(-16,9), S(-17,7), S(-12,26), S(-10,18), S(-12,20), S(-2,12), S(5,10),
                S(-11,-18), S(-8,-10), S(-9,0), S(-12,-3), S(-7,3), S(-3,3), S(8,-7), S(6,-14),
                S(-7,-26), S(-5,-25), S(1,-29), S(3,-19), S(1,-16), S(11,-39), S(17,-59), S(28,-77),
                S(-9,-28), S(-13,-29), S(-6,-30), S(-1,-27), S(-3,-29), S(-12,-28), S(10,-52), S(5,-55)
        },
        {
                S(26,-71), S(26,-27), S(48,-18), S(-82,26), S(-43,11), S(-9,14), S(31,5), S(127,-93),
                S(-95,19), S(-53,46), S(-84,54), S(15,36), S(-39,57), S(-32,68), S(-21,62), S(-51,32),
                S(-122,34), S(-20,50), S(-74,65), S(-92,75), S(-56,77), S(12,70), S(-26,71), S(-53,40),
                S(-91,22), S(-96,53), S(-110,69), S(-146,80), S(-136,80), S(-107,76), S(-108,69), S(-141,45),
                S(-83,10), S(-88,38), S(-114,59), S(-139,72), S(-140,72), S(-107,60), S(-113,50), S(-139,34),
                S(-45,1), S(-30,21), S(-79,40), S(-91,51), S(-86,50), S(-85,42), S(-45,24), S(-60,12),
                S(34,-20), S(-5,5), S(-17,17), S(-47,26), S(-50,29), S(-32,19), S(7,2), S(15,-15),
                S(32,-54), S(61,-38), S(41,-21), S(-49,-1), S(3,-20), S(-22,-6), S(41,-32), S(38,-59)
        }

    };

    // [PieceType - 3][attacks]
    static int[,] mobilityBonus = {
        {S(-19,1), S(-5,-5), S(1,0), S(4,-5), S(6,-2), S(5,3), S(3,4), S(2,4), S(4,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0)},
        {S(-33,-49), S(-25,-34), S(-18,-28), S(-16,-18), S(-10,-6), S(-3,6), S(3,9), S(6,15), S(7,22), S(8,20), S(12,18), S(16,18), S(12,24), S(40,4), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0)},
        {S(-36,-37), S(-25,-26), S(-19,-23), S(-17,-7), S(-16,-5), S(-11,-3), S(-7,-3), S(-1,0), S(3,9), S(9,11), S(14,12), S(21,14), S(23,19), S(29,23), S(34,16), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0), S(0,0)},
        {S(-62,-355), S(-11,-251), S(-24,-115), S(-25,-43), S(-26,-34), S(-23,-31), S(-21,-19), S(-22,2), S(-20,5), S(-17,7), S(-17,20), S(-17,27), S(-16,34), S(-16,41), S(-14,46), S(-14,51), S(-12,56), S(-13,64), S(-10,66), S(-8,69), S(2,63), S(6,65), S(9,63), S(16,62), S(27,52), S(73,32), S(114,13), S(141,10)}
    };

    // King cornering for checkmating lone king
    static int[] kingEdge = {
    S(-95, -95),  S(-95, -95),  S(-90, -90),  S(-90, -90), S(-90, -90),  S(-90, -90),  S(-95, -95),  S(-95, -95),
    S(-95, -95),  S(-50, -50),  S(-50, -50),  S(-50, -50),  S(-50, -50),  S(-50, -50),  S(-50, -50),  S(-95, -95),
    S(-90, -90),  S(-50, -50),  S(-20, -20),  S(-20, -20),  S(-20, -20),  S(-20, -20),  S(-50, -50),  S(-90, -90),
    S(-90, -90),  S(-50, -50),  S(-20, -20),    0,    0,  S(-20, -20),  S(-50, -50),  S(-90, -90),
    S(-90, -90),  S(-50, -50),  S(-20, -20),    0,    0,  S(-20, -20),  S(-50, -50),  S(-90, -90),
    S(-90, -90),  S(-50, -50),  S(-20, -20),  S(-20, -20),  S(-20, -20),  S(-20, -20),  S(-50, -50),  S(-90, -90),
    S(-95, -95),  S(-50, -50),  S(-50, -50),  S(-50, -50),  S(-50, -50),  S(-50, -50),  S(-50, -50),  S(-95, -95),
    S(-95, -95),  S(-95, -95),  S(-90, -90),  S(-90, -90), S(-90, -90),  S(-90, -90),  S(-95, -95),  S(-95, -95),
    };

    // Evaluation Weights
    static int tempo = S(18, 21);
    static int bishopPair = S(17, 58);
    static int[] piece_values = { S(81, 152), S(262, 330), S(280, 336), S(366, 602), S(773, 1100), S(0, 0) };
    static int[] game_phase_inc = { 0, 1, 1, 2, 4, 0, };

    static readonly double ttSlotSizeMB = 0.000024;
    static int hashSizeMB = 201;
    static int hashSize = Convert.ToInt32(hashSizeMB / ttSlotSizeMB);

    // this tuple is 24 bytes
    static (
        ulong, // hash 8 bytes
        ushort, // moveRaw 4 bytes
        int, // score 4 bytes
        int, // depth 4 bytes
        int // bound BOUND_EXACT=[1, 2147483647], BOUND_LOWER=2147483647, BOUND_UPPER=0 4 bytes
    )[] transpositionTable = new (ulong, ushort, int, int, int)[hashSize];

    // Variables for search
    static int rfpMargin = 55;
    static int futilityMargin = 116;
    static int mateScore = -20000;
    static int infinity = 30000;
    static int hardBoundTimeRatio = 10;
    static int softBoundTimeRatio = 40;


    enum ScoreType { upperbound, lowerbound, none };

    public static void setMargins(int VHashSizeMB = 201, int VrfpMargin = 55, int VfutilityMargin = 116, int VhardBoundTimeRatio = 10, int VsoftBoundTimeRatio = 40)
    {
        hashSizeMB = VHashSizeMB;
        hashSize = Convert.ToInt32(hashSizeMB / ttSlotSizeMB);
        transpositionTable = new (ulong, ushort, int, int, int)[hashSize];

        rfpMargin = VrfpMargin;
        futilityMargin = VfutilityMargin;
        hardBoundTimeRatio = VhardBoundTimeRatio;
        softBoundTimeRatio = VsoftBoundTimeRatio;
    }

    //Static Evaluation
    // Evaluation
    public static int Eval(Board board)
    {
        int phase = 0;
        int numPieceTypes = 0;
        int kingSq = 0;
        int score = tempo;
        int piece_mobility;

        foreach (bool isWhite in new[] { !board.IsWhiteToMove, board.IsWhiteToMove })
        {
            numPieceTypes = 0;
            score = -score;

            //       None (skipped)               King
            for (var pieceIndex = 0; ++pieceIndex <= 6;)
            {
                var bitboard = board.GetPieceBitboard((PieceType)pieceIndex, isWhite);

                if (bitboard != 0)
                {
                    numPieceTypes++;
                }
                // This and the following line is an efficient way to loop over each piece of a certain type.
                // Instead of looping each square, we can skip empty squares by looking at a bitboard of each piece,
                // and incrementally removing squares from it. More information: https://www.chessprogramming.org/Bitboards
                while (bitboard != 0)
                {
                    var sq = ClearAndGetIndexOfLSB(ref bitboard);

                    if (pieceIndex == 6)
                    {
                        if (isWhite)
                        {
                            kingSq = sq ^ 56;
                        }
                        else
                        {
                            kingSq = sq;
                        }
                    }
                    else if (pieceIndex == 3 && bitboard != 0)
                    {
                        score += bishopPair;
                    }
                    if (pieceIndex > 1 && pieceIndex < 6)
                    {
                        piece_mobility = mobilityBonus[pieceIndex - 2, GetNumberOfSetBits(GetPieceAttacks((PieceType)pieceIndex, new Square(sq), board, isWhite))];
                        score += piece_mobility;
                    }
                    if (isWhite) sq ^= 56;

                    phase += game_phase_inc[pieceIndex - 1];

                    score += pst[pieceIndex - 1, sq] + piece_values[pieceIndex - 1]; // PST + Material
                }
            }

            if (numPieceTypes == 1)
            {
                score += kingEdge[kingSq];
            }
        }

        // Tapered eval
        if (phase > 24)
            phase = 24;
        int eg_phase = 24 - phase;

        int mg_score = unpack_mg(score);
        int eg_score = unpack_eg(score);

        return ((mg_score * phase) + (eg_score * eg_phase)) / 24;

    }

    public Move Think(Board board, Timer timer)
    {
        int selDepth = 0;

        // Killer moves, 1 for each depth
        Move[] killers = new Move[4096];

        // History moves
        int[] history = new int[4096];

        int globalDepth = 1; // To be incremented for each iterative loop
        ulong nodes; // To keep track of searched positions in 1 iterative loop
        Move rootBestMove = Move.NullMove;

        void printInfo(int score, ScoreType scoreType)
        {
            string scoreTypeStr = scoreType == ScoreType.upperbound ? "upperbound " : scoreType == ScoreType.lowerbound ? "lowerbound " : "";

            bool isMateScore = score < mateScore + 100 || score > -mateScore - 100;

            if (!isMateScore)
            {
                Console.WriteLine($"info depth {globalDepth} seldepth {selDepth} time {timer.MillisecondsElapsedThisTurn} nodes {nodes} nps {Convert.ToInt32(1000 * nodes / ((ulong)timer.MillisecondsElapsedThisTurn + 0.001))} hashfull {1000 * nodes / (ulong)hashSize} score cp {score} {scoreTypeStr}pv {ChessChallenge.Chess.MoveUtility.GetMoveNameUCI(new(rootBestMove.RawValue))}");
            }
            else
            {
                bool sideIsMated = score < 0;
                int mateIn;
                if (sideIsMated)
                {
                    mateIn = (mateScore + score) / 2;
                }
                else
                {
                    mateIn = (-mateScore - score) / 2;
                }
                Console.WriteLine($"info depth {globalDepth} seldepth {selDepth} time {timer.MillisecondsElapsedThisTurn} nodes {nodes} nps {Convert.ToInt32(1000 * nodes / ((ulong)timer.MillisecondsElapsedThisTurn + 0.001))} hashfull {1000 * nodes / (ulong)hashSize} score mate {mateIn} {scoreTypeStr}pv {ChessChallenge.Chess.MoveUtility.GetMoveNameUCI(new(rootBestMove.RawValue))}");
            }
        }



        // Quiescence Search
        int qSearch(int alpha, int beta, int ply)
        {
            // Hard bound time management
            if (timer.MillisecondsElapsedThisTurn >= timer.MillisecondsRemaining / hardBoundTimeRatio) throw null;

            selDepth = Math.Max(ply, selDepth);

            int mating_value = -mateScore - ply;

            if (mating_value < beta)
            {
                beta = mating_value;
                if (alpha >= mating_value) return mating_value;
            }

            mating_value = mateScore + ply;

            if (mating_value > alpha)
            {
                alpha = mating_value;
                if (beta <= mating_value) return mating_value;
            }

            int standPat = Eval(board);

            int bestScore = standPat;

            // Terminal nodes
            if (board.IsInCheckmate())
                return mateScore + ply;
            if (board.IsDraw())
                return 0;

            ref var tt = ref transpositionTable[board.ZobristKey & (ulong)(hashSize - 1)];
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
                score = -qSearch(-beta, -alpha, ply + 1);
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

            selDepth = Math.Max(ply, selDepth);

            int mating_value = -mateScore - ply;

            if (mating_value < beta)
            {
                beta = mating_value;
                if (alpha >= mating_value) return mating_value;
            }

            mating_value = mateScore + ply;

            if (mating_value > alpha)
            {
                alpha = mating_value;
                if (beta <= mating_value) return mating_value;
            }

            bool isRoot = ply == 0;
            bool nonPv = alpha + 1 >= beta;

            ref var tt = ref transpositionTable[board.ZobristKey & (ulong)(hashSize - 1)];
            var (ttHash, ttMoveRaw, score, ttDepth, ttBound) = tt;

            bool ttHit = ttHash == board.ZobristKey;
            int oldAlpha = alpha;

            // Terminal nodes
            if (board.IsInCheckmate() && !isRoot)
                return mateScore + ply;
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
                return qSearch(alpha, beta, ply);

            // Static eval needed for RFP and NMP
            int eval = Eval(board);

            // Index for killers
            int killerIndex = ply & 4095;

            // Reverse futility pruning
            if (nonPv && eval - rfpMargin * depth >= beta && !board.IsInCheck()) return eval;

            // Null move pruning
            if (nonPv && eval >= beta && board.TrySkipTurn())
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
                if (nonPv && depth <= 4 && !move.IsCapture && (eval + futilityMargin * depth * depth < alpha) && bestScore > mateScore + 100)
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

                selDepth = 0;

                int alpha = -infinity;
                int beta = infinity;

                int delta = 0;

                if (globalDepth > 3)
                {
                    delta = 10;
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

                        printInfo(alpha, ScoreType.upperbound);
                    }
                    else if (newScore >= beta)
                    {
                        beta = Math.Min(newScore + delta, infinity);

                        printInfo(alpha, ScoreType.lowerbound);
                    }
                    else
                        break;

                    if (delta <= 500)
                        delta += delta;
                    else
                        delta = infinity;
                }

                score = newScore;

                printInfo(score, ScoreType.none);
            }
        }
        catch { }
        return rootBestMove;
    }
}