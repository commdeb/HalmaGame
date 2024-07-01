using Lab2Halma.Skynet;
using System;
using System.Text;

namespace Lab2Halma
{
    internal class Program
    {
        public static void Main(string[] args) 
        {
            
            Halma halma = new();

            Player player = new Player(1);
            MinMax alphaBeta = new AlphaBeta(1, depth: 6, width: 5) ;
            MinMax minMax = new MinMax(2, depth: 6, width: 5);

            //Info 
            // - depth 6 - <1s for move
            // - depth 10 - 1s for move

            minMax.SetStrategies
                (
                minMax.HAvoidBlockingMovesInBase,
                minMax.HDistanceToEnemyBase,
                minMax.HMaxTraverseDistance
                );

            alphaBeta.SetStrategies
                (
                alphaBeta.HDistanceToNearestFreeFieldInEnemyBase,
                alphaBeta.HNumberOfPawnsInMyBase,
                alphaBeta.HPrioritizeJumpMoves
                );


            //halma = new Halma(new List<Player> { player, minMax});
            halma = new Halma(new List<Player> { alphaBeta, minMax });
            halma.Init();
           
            try
            {
                halma.Start();
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine();
                Console.WriteLine(e.Message);
                Console.WriteLine("END GAME");
            } finally
            {
                halma.Dispose();
            }
            
            // Console.WriteLine(halma.ToString());

        }
        
        private static string ReadGameStateFromSTI(bool staticData = false)
        {
            string input = @"   | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 2 | 2 | 2 | 2 | 2 |
                                | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 2 | 2 | 2 | 0 | 2 |
                                | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 2 | 2 | 2 | 2 |
                                | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 2 | 2 |
                                | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 2 | 2 |
                                | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
                                | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
                                | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
                                | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
                                | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
                                | 0 | 0 | 0 | 0 | 0 | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
                                | 1 | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
                                | 1 | 1 | 2 | 2 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
                                | 1 | 1 | 1 | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
                                | 1 | 1 | 1 | 1 | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |
                                | 1 | 1 | 1 | 1 | 1 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 | 0 |";

            string secondInput = @"0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2
                                    0,0,0,0,0,0,0,0,0,0,0,2,2,2,2,2
                                    0,0,0,0,0,0,0,0,0,0,0,0,2,2,2,2
                                    0,0,0,0,0,0,0,0,0,0,0,0,0,2,2,2
                                    0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,2
                                    0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
                                    0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
                                    0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
                                    0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
                                    0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
                                    0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0
                                    1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0
                                    1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0
                                    1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0
                                    1,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0
                                    1,1,1,1,1,0,0,0,2,0,0,0,0,0,0,0";

            if(staticData) return input;

            Console.WriteLine("Write board state:");
            StringBuilder stringBuilder = new StringBuilder();
            int i = 0;
            do
            {
                i = 0;
                while (i < Halma.BOARD_SIZE)
                {
                    string? line = Console.ReadLine();
                    if (string.IsNullOrEmpty(line))
                    {
                        Console.WriteLine("Invalid input line. Try again!");
                        break;
                    }
                    else
                    {
                        stringBuilder.Append(line).Append(Environment.NewLine);
                    }
                    i++;
                }

            } while (i < Halma.BOARD_SIZE);

            //return stringBuilder.ToString();
            //return secondInput;
            return input;
        }
    }
}
