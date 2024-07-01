using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lab2Halma
{
    internal class Player
    {
        public int Id { get; private set;}
        public Halma.Move? LastMove { get; protected set;}
        public Player(int id) 
        {
            this.Id = id;

        }

        public override string ToString()
        {
            return "Player " + Id;
        }

        /// <summary>
        /// Player chooses move based on current board state. Sets its LastMove property:
        /// null -> used command
        /// (-1,-1,-1,-1) -> invalid move
        /// else -> valid move
        /// </summary>
        /// <param name="gameBoardState">current board state</param>
        /// <param name="alphabet"> alphabet dictionary</param>
        /// <returns>true if move was valid and succesfuly performed, otherwise false</returns>
        /// <exception cref="InvalidOperationException">Exception to close game</exception>
        internal virtual Halma.Move? ChooseMove(int[,] gameBoardState, ref Dictionary<char, int> alphabet, out Halma.Move? invalidMove, HashSet<Halma.Move>? possibleMoves = null)
        {
            int pawn = Id;
            Console.Write($"Player's {pawn} move (match format as '[col][row] to [col][row]'): ");
            string? move = Console.ReadLine();

            if ( move != null ) move = move.Trim();

            int rowCount = gameBoardState.GetLength(0);
            possibleMoves = possibleMoves == null ? Halma.GivePossibleMoves(gameBoardState, pawn) : possibleMoves;
            invalidMove = null;
            Halma.Move? chosenMove = null;
            if (!string.IsNullOrEmpty(move) && Regex.IsMatch(move, @"^[A-P](1[0-6]|[1-9]) to [A-P](1[0-6]|[1-9])$"))
            {
                chosenMove = ParseMove(move, ref alphabet, rowCount);
                invalidMove = ParseMove(move, ref alphabet, rowCount);
                if ((Halma.Move.IsValid(chosenMove) && Halma.ValidateMove(chosenMove, possibleMoves, out Halma.Move? realMove)))
                {
                    LastMove = realMove;
                    invalidMove = null;
                    return realMove;
                } else
                {
                    LastMove = null;
                    return null;
                }
                

            } else if(!string.IsNullOrEmpty(move))
            {
                var command = move.ToLower().Trim();
                switch (command)
                {
                    case "help":
                        Console.WriteLine("Possible moves: ");
                        int i = 1;
                        foreach (Halma.Move possibleMove in possibleMoves.OrderBy(m => m.From.GetHashCode()))
                        {
                            Console.WriteLine($"{i++}.{Halma.MoveToString(possibleMove)}");
                        }
                        Console.WriteLine();
                        break;
                    case "exit":
                        throw new InvalidOperationException("Command interruption");
                    default:
                        Console.WriteLine("UNKNOWN COMMAND");
                        break;
                }
            }

            LastMove = null;
            return null;
        }

        internal Halma.Move ParseMove(string move, ref Dictionary<char, int> alphabet, int xOffset = 0)
        {
            string[] toFrom = move.Split("to", StringSplitOptions.TrimEntries);
            List<int> res;
            if (toFrom[0].Equals(toFrom[1]))
                res = new List<int>(4) { -1, -1, -1, -1 };
            else
            {
                res = new List<int>(4);
                char letter;
                int coordX;
                int coordY;
                foreach (string position in toFrom)
                {
                    letter = position[0];
                    coordY = alphabet[letter];
                    coordX = xOffset - Convert.ToInt32(position[1..].Trim());
                    res.Add(coordX);
                    res.Add(coordY);
                }
            }

            return new Halma.Move(new Halma.PawnCoord(res[0], res[1]),new Halma.PawnCoord(res[2], res[3]));
        }
    }
}
