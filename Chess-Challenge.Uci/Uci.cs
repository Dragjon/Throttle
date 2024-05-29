using System.Text;
using ChessChallenge.API;
using ChessChallenge.Chess;
using Board = ChessChallenge.API.Board;
using Move = ChessChallenge.API.Move;
using Timer = ChessChallenge.API.Timer;

namespace Chess_Challenge.Cli
{
    internal class Uci
    {
        private const string StartposFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        private IChessBot _bot;
        private Board _board;
        private int _hashSizeMB;
        private int _rfpMargin;
        private int _futilityMargin;
        private int _hardBoundTimeRatio;
        private int _softBoundTimeRatio;
        private int _aspDepth;
        private int _aspDelta;

        public Uci()
        {
            Reset();
        }

        private void Reset()
        {
            _bot = new MyBot();
            _board = Board.CreateBoardFromFEN(StartposFen);
            _hashSizeMB = 201;
            _rfpMargin = 55;
            _futilityMargin = 116;
            _hardBoundTimeRatio = 10;
            _softBoundTimeRatio = 40;
            _aspDepth = 3;
            _aspDelta = 10;
        }

        private void HandleUci()
        {
            Console.WriteLine("id name Throttle V3.2");
            Console.WriteLine("id author Chess123easy");
            Console.WriteLine("option name Hash type spin default 256 min 1 max 1024");
            Console.WriteLine("option name rfpMargin type spin default 55 min 0 max 200");
            Console.WriteLine("option name futilityMargin type spin default 116 min 0 max 400");
            Console.WriteLine("option name hardBoundTimeRatio type spin default 10 min 1 max 100");
            Console.WriteLine("option name softBoundTimeRatio type spin default 40 min 1 max 300");
            Console.WriteLine("option name aspDepth type spin default 3 min 0 max 10");
            Console.WriteLine("option name aspDelta type spin default 10 min 0 max 100");
            Console.WriteLine();
            Console.WriteLine("uciok");
        }

        private void HandleSetOption(IReadOnlyList<string> words)
        {
            if (words.Count < 5) return;

            string optionName = words[1];
            if (optionName == "name" && words[2] == "Hash" && words[3] == "value")
            {
                if (int.TryParse(words[4], out var hashValue))
                {
                    _hashSizeMB = hashValue;
                    Console.WriteLine($"info string Hash set to {_hashSizeMB}");
                }
            }
            else if (optionName == "name" && words[2] == "rfpMargin" && words[3] == "value")
            {
                if (int.TryParse(words[4], out var rfpValue))
                {
                    _rfpMargin = rfpValue;
                    Console.WriteLine($"info string rfpMargin set to {_rfpMargin}");
                }
            }
            else if (optionName == "name" && words[2] == "futilityMargin" && words[3] == "value")
            {
                if (int.TryParse(words[4], out var fpValue))
                {
                    _futilityMargin = fpValue;
                    Console.WriteLine($"info string futilityMargin set to {_futilityMargin}");
                }
            }
            else if (optionName == "name" && words[2] == "hardBoundTimeRatio" && words[3] == "value")
            {
                if (int.TryParse(words[4], out var hbtr))
                {
                    _hardBoundTimeRatio = hbtr;
                    Console.WriteLine($"info string hardBoundTimeRatio set to {_hardBoundTimeRatio}");
                }
            }
            else if (optionName == "name" && words[2] == "softBoundTimeRatio" && words[3] == "value")
            {
                if (int.TryParse(words[4], out var sbtr))
                {
                    _softBoundTimeRatio = sbtr;
                    Console.WriteLine($"info string softBoundTimeRatio set to {_softBoundTimeRatio}");
                }
            }

            else if (optionName == "name" && words[2] == "aspDepth" && words[3] == "value")
            {
                if (int.TryParse(words[4], out var aspdh))
                {
                    _aspDepth = aspdh;
                    Console.WriteLine($"info string aspDepth set to {_aspDepth}");
                }
            }

            else if (optionName == "name" && words[2] == "aspDelta" && words[3] == "value")
            {
                if (int.TryParse(words[4], out var aspda))
                {
                    _aspDelta = aspda;
                    Console.WriteLine($"info string aspDelta set to {_aspDelta}");
                }
            }

            MyBot.setMargins(_hashSizeMB, _rfpMargin, _futilityMargin, _hardBoundTimeRatio, _softBoundTimeRatio);

        }

        private void HandlePosition(IReadOnlyList<string> words)
        {
            var writingFen = false;
            var writingMoves = false;
            var fenBuilder = new StringBuilder();

            for (var wordIndex = 0; wordIndex < words.Count; wordIndex++)
            {
                var word = words[wordIndex];

                if (word == "startpos")
                {
                    _board = Board.CreateBoardFromFEN(StartposFen);
                }

                if (word == "fen")
                {
                    writingFen = true;
                    continue;
                }

                if (word == "moves")
                {
                    if (writingFen)
                    {
                        fenBuilder.Length--;
                        var fen = fenBuilder.ToString();
                        _board = Board.CreateBoardFromFEN(fen);
                    }
                    writingFen = false;
                    writingMoves = true;
                    continue;
                }

                if (writingFen)
                {
                    fenBuilder.Append(word);
                    fenBuilder.Append(' ');
                }

                if (writingMoves)
                {
                    var move = new Move(word, _board);
                    _board.MakeMove(move);
                }
            }

            if (writingFen)
            {
                fenBuilder.Length--;
                var fen = fenBuilder.ToString();
                _board = Board.CreateBoardFromFEN(fen);
            }
        }

        private static string GetMoveName(Move move)
        {
            if (move.IsNull)
            {
                return "Null";
            }

            string startSquareName = BoardHelper.SquareNameFromIndex(move.StartSquare.Index);
            string endSquareName = BoardHelper.SquareNameFromIndex(move.TargetSquare.Index);
            string moveName = startSquareName + endSquareName;
            if (move.IsPromotion)
            {
                switch (move.PromotionPieceType)
                {
                    case PieceType.Rook:
                        moveName += "r";
                        break;
                    case PieceType.Knight:
                        moveName += "n";
                        break;
                    case PieceType.Bishop:
                        moveName += "b";
                        break;
                    case PieceType.Queen:
                        moveName += "q";
                        break;
                }
            }
            return moveName;
        }

        private void HandleGo(IReadOnlyList<string> words)
        {
            var ms = 60000;

            for (var wordIndex = 0; wordIndex < words.Count; wordIndex++)
            {
                var word = words[wordIndex];
                if (words.Count > wordIndex + 1)
                {
                    var nextWord = words[wordIndex + 1];
                    if (word == "wtime" && _board.IsWhiteToMove)
                    {
                        if (int.TryParse(nextWord, out var wtime))
                        {
                            ms = Math.Abs(wtime);
                        }
                    }
                    if (word == "btime" && !_board.IsWhiteToMove)
                    {
                        if (int.TryParse(nextWord, out var btime))
                        {
                            ms = Math.Abs(btime);
                        }
                    }
                }

                if (word == "infinite")
                {
                    ms = int.MaxValue;
                }
            }

            var timer = new Timer(ms);
            var move = _bot.Think(_board, timer);
            var moveStr = GetMoveName(move);
            Console.WriteLine($"bestmove {moveStr}");
        }

        private void HandleLine(string line)
        {
            var words = line.Split(' ');
            if (words.Length == 0)
            {
                return;
            }

            var firstWord = words[0];
            switch (firstWord)
            {
                case "uci":
                    HandleUci();
                    return;
                case "ucinewgame":
                    Reset();
                    return;
                case "position":
                    HandlePosition(words);
                    return;
                case "isready":
                    Console.WriteLine("readyok");
                    return;
                case "go":
                    HandleGo(words);
                    return;
                case "setoption":
                    HandleSetOption(words);
                    return;
                case "seval":
                    Console.WriteLine(MyBot.Eval(_board));
                    return;
            }
        }

        public void Run()
        {
            while (true)
            {
                var line = Console.ReadLine();
                if (line == "quit" || line == "exit")
                {
                    return;
                }

                HandleLine(line);
            }
        }
    }
}
