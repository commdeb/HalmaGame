using Lab2Halma.Interfaces;
using System.Text;


namespace Lab2Halma
{
    internal class Halma : IDisposable
    {
        public static readonly int PLAYERS_COUNT = 2;
        public static readonly int BOARD_SIZE = 16;
        public static readonly int PAWNS_NUMBER = 19;
        public static readonly int HASH_SET_SIZE_DEFAULT = SetHashMoveInit(); 

        public static readonly int[][] Directions = {
        new int[] { 0, 1 },   // Right
        new int[] { 0, -1 },  // Left
        new int[] { 1, 0 },   // Down
        new int[] { -1, 0 },  // Up
        new int[] { 1, 1 },   // Down-right
        new int[] { 1, -1 },  // Down-left
        new int[] { -1, 1 },  // Up-right
        new int[] { -1, -1 }  // Up-left
        };

        private static Dictionary<char, int> Alphabet = InitializeAlphabet();
        private static char[] AlphabetArray = Alphabet.Keys.ToArray();
        
        public int[,] GameBoardState { get;  set; }
        public List<Player> Players { get; private set; }
        public static int RoundCounter { get; private set; }

        public Halma() : this(new List<Player>() { new (1), new (2) }) { }

        public Halma(List<Player> players) : this(new int[BOARD_SIZE, BOARD_SIZE], players) { }
        
        public Halma(int[,] board, List<Player> players)
        {
            this.GameBoardState = board;
            this.Players = players;
        }

        public static Dictionary<char, int> InitializeAlphabet()
        {
            Dictionary<char, int> alphabet = new Dictionary<char, int>(BOARD_SIZE);

            
            char letter = 'A';
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                alphabet.Add(letter, i);
                letter++;
            }

            return alphabet;
        }

        public void Init(string? input = null)

        {
            if(input != null) LoadState(input);

            if (Players == null || Players.Count == 0 || Players.Count > PLAYERS_COUNT) throw new Exception("Invalid number of players");
            if (GameBoardState == null || GameBoardState.Length != BOARD_SIZE * BOARD_SIZE) throw new Exception("Invalid init board state");


            var boardEnum = GameBoardState.ToEnumerable();
            //var twoDimBoard = GameBoardState.ToTwoDimEnumerable();

            if(!boardEnum.Any(i => i != 0))
            {
                InitBoard();
            } 
        }

        public void InitBoard()
        {
            int prevIndex = 0;
            int index = 5;
            int rowLength = GameBoardState.GetLength(0);

            (int pawnSymbol1, int pawnSymbol2) = (Players[0].Id, Players[1].Id);

            for (int i = 0; i < rowLength; i++)
            {
                for(int j = 0;  j < rowLength; j++)
                {
                    if (i < 5)
                    {
                        if (j >= rowLength - index)
                        {
                            GameBoardState[i, j] = pawnSymbol2; 
                            GameBoardState[j, i] = pawnSymbol1;

                        }

                    }

                }
                if (prevIndex++ != 0) index--;
            }
        }
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            int rowLength = GameBoardState.GetLength(0);
            string rowNum;
            for (int i = 0; i < rowLength; i++)
            {
                if(i == 0) stringBuilder.Append("       { ").Append(string.Join(" | ", Alphabet.Keys)).Append(" }").Append(Environment.NewLine);
                for (int j = 0; j < rowLength; j++)
                {
                    if (j == 0)
                    {
                        rowNum = Convert.ToString(rowLength - i);
                        stringBuilder.Append("{ ").Append(rowNum).Append(" } ");
                        if (rowNum.Length == 1)
                            stringBuilder.Append(' ');

                    }
                        stringBuilder.Append("| ").Append(GameBoardState[i, j]).Append(' ');
                }
                stringBuilder.Append('|').Append(Environment.NewLine);
            }
            return stringBuilder.ToString();
        }
        public static int[,] ConvertTo2DArray(string input)
        {
            if (input == null || input.Length == 0) throw new Exception("Invalid input string");

            string[] lines = input.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            int rowCount = lines.Length;
            int colCount = lines[0].Split(new char[] { ' ', '\t' , ',', '|' }, StringSplitOptions.RemoveEmptyEntries).Length;
            int[,] result = new int[rowCount, colCount];

            for (int i = 0; i < rowCount; i++)
            {
                string[] values = lines[i].Split(new char[] { ' ', '\t', ',', '|'}, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < colCount; j++)
                {
                    result[i, j] = int.Parse(values[j]);
                }
            }

            return result;
        }

        internal void LoadState(string input, bool check = true)
        {
            GameBoardState = ConvertTo2DArray(input);

            if (!check) return;

            int pawnCount = PAWNS_NUMBER * Players.Count;
            if (pawnCount <= PAWNS_NUMBER) throw new Exception($"Invalid number of players to load state from input: {input}");
            int countedPawns = 0;
            foreach (Player player in Players)
            {
                countedPawns += FindAllPawnPositions(GameBoardState, player.Id).Count;
            }

            if(countedPawns != pawnCount)
            {
                GameBoardState = null;
                throw new Exception($"Wrong number of pawns on board: is total {countedPawns}, should be {pawnCount}");
            }
        }

        public static bool ValidateMove(int fromX, int fromY, int toX, int toY, HashSet<Move> possibleMoves, out Move? move) 
        {
            
            return possibleMoves.TryGetValue(new Move(new PawnCoord(fromX, fromY), new PawnCoord(toX, toY)), out move); ; 
        }

        public static bool ValidateMove(Move move, HashSet<Move> possibleMoves, out Move? actualMove)
        {
             
            return possibleMoves.TryGetValue(move, out actualMove);
        }

        public static int SetHashMoveInit()
        {
            int freeFields = (BOARD_SIZE ^ 2 - PAWNS_NUMBER);
            int factorial = PAWNS_NUMBER;
            freeFields = freeFields > factorial ? factorial / 2 : PAWNS_NUMBER;
            return 100; //factorial.FactorialWithBoundaries(freeFields);
        }

        public static HashSet<Move> GivePossibleMoves(int[,] gameBoardState, int pawn)
        {
            List<PawnCoord> currentPawnsCoord;
            HashSet<Move> possibleMoves;
            int estimatedSize;
            lock (gameBoardState) // Lock access to the gameBoardState
            {
                currentPawnsCoord = FindAllPawnPositions(gameBoardState, pawn);
                estimatedSize = HASH_SET_SIZE_DEFAULT / ((RoundCounter == 0 ? 4 : RoundCounter) / 2);
                possibleMoves = new HashSet<Move>(estimatedSize);
            }
   
            foreach (var pawnCoord in currentPawnsCoord)
            {
                    HashSet<Move> movesFromPawn;
                    lock (gameBoardState)
                    {
                        movesFromPawn = FindPossibleMovesFromPosition(gameBoardState, pawnCoord, estimatedSize / currentPawnsCoord.Count);
                    }
                    lock (possibleMoves)
                    {
                        possibleMoves.UnionWith(movesFromPawn);
                    }
            }

            return possibleMoves;
        }



        public static string MoveToString(Move move)
        {
            char from = AlphabetArray[move.From.Y];  //Alphabet.First((pair => pair.Value == move.From.X)).Key;
            char to = AlphabetArray[move.To.Y];  //Alphabet.First((pair => pair.Value == move.To.X)).Key;
            return $"Move from {from}{BOARD_SIZE - move.From.X} to {to}{BOARD_SIZE - move.To.X}";
        }

        //IMPORTANT!!
        //pawns cannot move backwards
        //if player is blocked it passes its turn

        private static HashSet<Move> FindPossibleMovesFromPosition(int[,] gameBoardState, PawnCoord pawnCoord, int setsSize = 100, bool isJump = false, HashSet<PawnCoord> visited = null)
        {


                    HashSet<Move> possibleMoves = new HashSet<Move>(setsSize);

                    int rowLenght = gameBoardState.GetLength(0);
                    int pawn = gameBoardState[pawnCoord.X, pawnCoord.Y];

                    if (visited == null)
                        visited = new HashSet<PawnCoord>() { pawnCoord };
                    else
                        visited.Add(pawnCoord);

                    foreach (var direction in Directions)
                    {
                        int newX = pawnCoord.X + direction[0];
                        int newY = pawnCoord.Y + direction[1];

                        if (visited.Contains(new PawnCoord(newX, newY))) continue;

                        if (CheckBoundaries(newX, newY, rowLenght))
                        {
                            if (gameBoardState[newX, newY] == 0 && !isJump && CheckIfNotLeavingEnemyBase(gameBoardState, pawn, pawnCoord, new(newX, newY)))
                            {
                                possibleMoves.Add(new Move(pawnCoord, new PawnCoord(newX, newY)));


                            }
                            else if (gameBoardState[newX, newY] != 0)
                            {
                                newX += direction[0];
                                newY += direction[1];
                                if (visited.Contains(new PawnCoord(newX, newY))) continue;


                                if (CheckBoundaries(newX, newY, rowLenght) && gameBoardState[newX, newY] == 0 &&
                                    CheckIfNotLeavingEnemyBase(gameBoardState, pawn, pawnCoord, new(newX, newY)))
                                {
                                    possibleMoves.Add(new Move(pawnCoord, new PawnCoord(newX, newY)));
                                    possibleMoves.UnionWith(FindPossibleMovesFromPosition(gameBoardState, new PawnCoord(newX, newY), isJump: true, visited: visited)
                                        .Select(m => new Move(pawnCoord, m.To)));
                                }


                            }

                        }
                    }


                    return possibleMoves;
        }

        public static bool CheckIfNotLeavingEnemyBase(int[,] gameBoardState, int pawn, PawnCoord from, PawnCoord to)
        {
            var enemyBase = EnemyPlayerBaseCoords(pawn, gameBoardState);

            if (enemyBase.Contains(from) && !enemyBase.Contains(to)) return false;

            return true;
        }

        public static HashSet<PawnCoord> EnemyPlayerBaseCoords(int pawn, int[,]? gameBoardState = null)
        {
            return PlayerBaseCoords(pawn % Halma.PLAYERS_COUNT + 1, gameBoardState);
        }
        public static HashSet<PawnCoord> PlayerBaseCoords(int pawn, int[,]? gameBoardState = null)
        {
            if(gameBoardState == null)
            using (Halma halma = new Halma())
            {
                halma.Init();
                gameBoardState = halma.GameBoardState;
                    gameBoardState = halma.GameBoardState;
            }
                
            int prevIndex = 0;
            int index = 5;
            int rowLength = gameBoardState.GetLength(0);
            int field;
            HashSet<PawnCoord> baseCoords = new HashSet<PawnCoord>(19);
            for (int i = 0; i < rowLength; i++)
            {
                for (int j = 0; j < rowLength; j++)
                {
                    if (i < 5)
                    {
                        if (j >= rowLength - index)
                        {
                            switch (pawn)
                            {
                                case 1:
                                    baseCoords.Add(new PawnCoord(j, i));
                                    break;
                                case 2:
                                    baseCoords.Add(new PawnCoord(i, j));
                                    break;
                                default: throw new Exception("Invalid pawn for player count during base check!");
                            }
                            

                        }

                    }

                }
                if (prevIndex++ != 0) index--;
            }
            return baseCoords;

        }

        public static bool CheckBoundaries(int x, int y, int rowLenght)
        {
            return x >= 0 && x < rowLenght && y >= 0 && y < rowLenght;
        }

        public class PawnCoord : Interfaces.IValidatable
        {
            public int X; 
            public int Y;

            public PawnCoord(int x, int y)
            {
                X = x;
                Y = y;
            }

            public static bool IsValid(IValidatable? obj)
            {
                PawnCoord? pawnCoord = obj as PawnCoord;
                return pawnCoord != null && pawnCoord.X >= 0 && pawnCoord.Y >= 0;
            }

            public override bool Equals(object? obj)
            {
                return obj is PawnCoord coord &&
                       X == coord.X &&
                       Y == coord.Y;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(X, Y);
            }

            public int CalcDistance(PawnCoord pawnCoord) //Manhattan dist
            {
                if (pawnCoord == null) return -1;
                int deltaX = Math.Abs(X - pawnCoord.X);
                int deltaY = Math.Abs(Y - pawnCoord.Y);
                return deltaX + deltaY;
            }

            public HashSet<PawnCoord> Neighbors()
            {
                PawnCoord pawnCoord = this;
                HashSet<PawnCoord> result = new HashSet<PawnCoord>();

                foreach (var direction in Directions)
                {
                    int newX = pawnCoord.X + direction[0];
                    int newY = pawnCoord.Y + direction[1];
                    if (CheckBoundaries(newX, newY, BOARD_SIZE))
                        result.Add(new PawnCoord(newX, newY));
                }

                return result;
            }
            public static PawnCoord operator +(PawnCoord a, PawnCoord b)
            {
                return new PawnCoord(a.X + b.X, a.Y + b.Y);
            }

            public static PawnCoord operator -(PawnCoord a, PawnCoord b)
            {
                return new PawnCoord(a.X - b.X, a.Y - b.Y);
            }
        }

        public class Move : Interfaces.IValidatable
        {
            public PawnCoord From;
            public PawnCoord To;

            public Move(PawnCoord from, PawnCoord to)
            {
                From = from;
                To = to;
            }

            public static bool IsValid(IValidatable? obj)
            {
                Move? move = obj as Move;
                return move != null && PawnCoord.IsValid(move.From) && PawnCoord.IsValid(move.To);
            }

            public override bool Equals(object? obj)
            {
                return obj is Move move &&
                       EqualityComparer<PawnCoord>.Default.Equals(From, move.From) &&
                       EqualityComparer<PawnCoord>.Default.Equals(To, move.To);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(From, To);
            }

            internal (int fromX, int fromY, int toX, int toY) ToTupleQuadruple()
            {
                return (From.X, From.Y, To.X, To.Y);
            }
        }

        public static List<PawnCoord> FindAllPawnPositions(int[,] gameBoardState, int pawn)
        {
            int rowLength = gameBoardState.GetLength(0);
            List<PawnCoord> result = new List<PawnCoord>(PAWNS_NUMBER);
            for (int i = 0; i < rowLength; i++)
            {
                for (int j = 0; j < rowLength; j++)
                {
                    if (gameBoardState[i, j] == pawn)
                        result.Add(new PawnCoord(x: i,y: j));
                }
                if (result.Count == PAWNS_NUMBER) break;
            }
            return result;
        }

        public void MakeMove(Move move, int pawn)
        {
            PerformMove(GameBoardState, move, pawn);
        }

        public static void PerformMove(int[,] gameBoardState, Move move, int pawn)
        {
            (int fromX, int fromY, int toX, int toY) = move.ToTupleQuadruple();
            gameBoardState[fromX, fromY] = 0;
            gameBoardState[toX, toY] = pawn;
        }

        public static int[,] PerformMoveWithCopy(int[,] gameBoardState, Move move, int pawn)
        {
            int[,] newGameBoardState = new int[gameBoardState.GetLength(0), gameBoardState.GetLength(1)];
            Array.Copy(gameBoardState, newGameBoardState, gameBoardState.Length);

            PerformMove(newGameBoardState, move, pawn);
            return newGameBoardState;
        }

        internal void Start()
        {
            RoundCounter = 1;
            WinCondition winCondition = CheckBoardForWin;//(int[,] b, int p) => false;
            Player? currPlayer = null;
            Queue<Player> playersQueue = new Queue<Player>(Players);
            Move? currentMove = null;
            HashSet<Move> possibleMoves = new HashSet<Move>();
            int pawn = 0;
            do
            {
                if (Move.IsValid(currentMove) || pawn == 0 || possibleMoves.Count == 0)
                {
                    currPlayer = playersQueue.Dequeue();
                    pawn = currPlayer.Id;
                    RoundCounter++;
                }
                Console.WriteLine(ToString());
                Console.WriteLine("Round: " + RoundCounter / 2);
                possibleMoves = GivePossibleMoves(GameBoardState, pawn);
                if (Move.IsValid(currentMove = currPlayer.ChooseMove(GameBoardState, ref Alphabet, out Move? invalidMove, possibleMoves)))
                {
                    MakeMove(currentMove, pawn);
                    playersQueue.Enqueue(currPlayer);
                }
                else if(invalidMove != null && possibleMoves.Count > 0)
                {
                    string info = invalidMove.To.X != -1 ? MoveToString(invalidMove) : "SAME PAWN'S POSITION";
                    Console.WriteLine($"Invalid move of Player {pawn}: {info}");
                } else if(possibleMoves.Count == 0)
                {
                    Console.WriteLine($"Player {pawn} does not have any possible moves!");
                    playersQueue.Enqueue(currPlayer);
                }

            } while (!winCondition(GameBoardState, pawn));

            Console.WriteLine(ToString());
            Console.WriteLine();
            Console.WriteLine("-----------------------");
            Console.WriteLine($"Player {pawn} has won!");
            Console.WriteLine("-----------------------");

        }

        internal delegate bool WinCondition(int[,] boardState, int pawn);

        internal bool CheckBoardForWin(int[,] boardState, int pawn)
        {
            int prevIndex = 0;
            int index = 5;
            int rowLength = GameBoardState.GetLength(0);
            int field;
            for (int i = 0; i < rowLength; i++)
            {
                for (int j = 0; j < rowLength; j++)
                {
                    if (i < 5)
                    {
                        if (j >= rowLength - index)
                        {
                            switch (pawn)
                            {
                                case 1:
                                    field = GameBoardState[i, j]; 
                                    break;
                                case 2:
                                    field = GameBoardState[j, i];
                                    break;
                                default: throw new Exception("Invalid pawn for player count during win condition check!");
                            }
                            if(field != pawn) return false;

                        }

                    }

                }
                if (prevIndex++ != 0) index--;
            }
            return true;
        }

        public void Dispose()
        {
            //GameBoardState = null;
            //AlphabetArray = null;
            Players.Clear();
            //Alphabet.Clear();
            
        }

        internal static bool IsInsideBoard(PawnCoord pawnCoord)
        {
            return CheckBoundaries(pawnCoord.X, pawnCoord.Y, BOARD_SIZE);
        }
    }

    public static class IntegerExtension
    {
        public static int FactorialWithBoundaries(this int value, int lowerBand = 1)
        {
            lowerBand = lowerBand <= 1 || lowerBand >= value ? 1 : lowerBand;

            for (int i = lowerBand; i <= value; i++)
                value *= i;

            return value;
        }
        public static int Factorial(this int value)
        {
            return value.FactorialWithBoundaries();
        }
    }

    public static class ArrayExtensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this T[,] target)
        {
            foreach (var item in target)
                yield return item;
        }

        public static IEnumerable<IEnumerable<T>> ToTwoDimEnumerable<T>(this T[,] target)
        {
            var result = new List<List<T>>((int) Math.Sqrt((double)target.Length));    
            for(int i = 0; i < target.GetLength(0); i++)
            {
                if (result.Count < target.GetLength(0))
                    result.Add(new List<T>(result.Count));
                for (int j = 0; j < target.GetLength(0); j++)
                {
                    result[i].Add(target[i,j]);   
                }
                
            }
            return result;
                
        }
    }

    
}
