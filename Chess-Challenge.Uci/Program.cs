using ChessChallenge.Application;

namespace Chess_Challenge.Cli
{
    internal class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Sebastian Lague's Chess Challenge UCI interface by Gediminas Masaitis");
            var uci = new Uci();
            uci.Run();
        }
    }
}