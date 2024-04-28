using ChessChallenge.API;
using static System.Math;
using System.Linq;
namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {

        //int nodes;

        Move[] TT = new Move[8388608];
        Move rootBestMove;
        public Move Think(Board board, Timer timer)
        {
            var globalDepth = 0;

            // Negamax Search Algorithm
            int Negamax(int depth, int ply, int alpha, int beta)
            {
                // Hard bound time management
                if (timer.MillisecondsElapsedThisTurn > timer.MillisecondsRemaining / 20) throw null;

                // PSQTs and piece values tuned using texel tuner and scaled to 0-255
                var (key, tScore, moves, isQuise, packedVals) = (board.ZobristKey % 8388608, 0, 0, depth < 1, new[] {
                1664105450252474134ul, 5714874758031429454ul, // files then rank
                6148914691236517205ul, 9332163775400411521ul,
                18374403900871474685ul, 72339069014704384ul,
                1666359449072571927ul, 5498703075429273422ul,
                6076858201005184340ul, 9476562637459325056ul,
                18374405000383036925ul, 566261389983744ul});

                // Visualization of tuned PST (scaled 0 - 255)
                /*

                  Files
                   A   B   C   D   E   F   G   H
                { 22, 23, 23, 23, 23, 23, 24, 23 }, // Pawn
                { 78, 79, 79, 80, 80, 80, 79, 79 }, // Knight
                { 85, 85, 85, 85, 85, 85, 85, 85 }, // Bishop
                { 129, 129, 130, 130, 130, 129, 130, 129 }, // Rook
                { 253, 253, 254, 254, 254, 254, 254, 254 }, // Queen
                { 0, 1, 1, 0, 0, 0, 1, 1 }, // King

                 Ranks
                   1   2   3   4   5   6   7   8
                { 23, 22, 22, 22, 23, 25, 32, 23 }, // Pawn
                { 78, 79, 79, 80, 80, 81, 79, 76 }, // Knight
                { 84, 85, 85, 85, 86, 86, 85, 84 }, // Bishop
                { 128, 128, 128, 129, 130, 131, 131, 131 }, // Rook
                { 253, 253, 253, 254, 254, 255, 254, 254 }, // Queen
                { 0, 0, 0, 1, 3, 3, 2, 0 }, // King

                */

                foreach (bool isWhite in new[] { !board.IsWhiteToMove, board.IsWhiteToMove })
                {
                    tScore = -tScore;
                    ulong bitboard = isWhite ? board.WhitePiecesBitboard : board.BlackPiecesBitboard;

                    while (bitboard != 0)
                    {
                        int sq = BitboardHelper.ClearAndGetIndexOfLSB(ref bitboard),
                            pieceIndex = (int)board.GetPiece(new(sq)).PieceType;
                        tScore += ((byte)(packedVals[pieceIndex + 5] >> ((isWhite ? sq : sq ^ 56) & 0b111000))
                               + (byte)(packedVals[pieceIndex - 1] >> sq % 8 * 8)); //* 6;


                    }
                }


                if (isQuise)
                {
                    // Stand Pat pruning
                    if (tScore >= beta) return beta;

                    if (alpha < tScore) alpha = tScore;
                }

                // Reverse Futility Pruning
                else if (tScore - 100 * depth >= beta) return tScore;

                // Check Extension
                if (board.IsInCheck()) depth++;

                foreach (Move move in board.GetLegalMoves(isQuise).OrderByDescending(move => (move == TT[key], move.CapturePieceType, 0 - move.MovePieceType)))
                {
                    moves++;
                    //nodes++;
                    board.MakeMove(move);

                    // Late Move Reduction
                    int score = board.IsInCheckmate() ? 30000 - ply : board.IsDraw() ? 0 : -Negamax(depth - (moves > 4 && depth > 3 ? 2 : 1), ply + 1, -beta, -alpha);
                    board.UndoMove(move);


                    if (score > alpha)
                    {
                        // If a move raises the alpha, add it to TT
                        TT[key] = move;
                        alpha = score;
                        if (ply == 0) rootBestMove = move;
                    }

                    // Apha/Beta Pruning
                    if (alpha >= beta) break;

                }

                return alpha;
            }

            do

                try
                {
                    for (; ; ++globalDepth)
                    {

                        // Aspiration window search
                        // Pawn value is ~56 so half of that would be ~23 which is our starting window
                        int score = Negamax(globalDepth, 0, -23, 23); ;

                        if (-23 >= score || score >= 23)
                            // Immediately abandon aspiration window search
                            Negamax(globalDepth, 0, -100000, 100000);

                    }

                }
                //Console.WriteLine($"info depth {globalDepth} time {timer.MillisecondsElapsedThisTurn} score cp {score} nodes {nodes} nps {Convert.ToInt32(1000 * (ulong)nodes / ((ulong)timer.MillisecondsElapsedThisTurn + 1))} pv {rootBestMove.ToString().Substring(7, rootBestMove.ToString().Length - 8)}");
                catch { }
            while (timer.MillisecondsElapsedThisTurn <= timer.MillisecondsRemaining / 40);

            return rootBestMove;
        }

    }
}