using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Lab2Halma.Halma;


namespace Lab2Halma.Skynet
{
    internal class MinMax : Bot
    {
        
        public int MAX_DEPTH { get; set; }
        public  int MAX_WIDTH { get; set; }

        public HashSet<Halma.PawnCoord> MyBase { get; set; }
        public HashSet<Halma.PawnCoord> MyNearBase { get; set; }
        public HashSet<Halma.PawnCoord> EnemyBase { get; set; }

        public HashSet<Halma.PawnCoord> EnemyNearBase { get; set; }

        public bool NearBaseFlag = false;

        public HashSet<MoveContainer> PrevMoves { get; set; }


        public static readonly int NUMB_OF_PLAYERS = Halma.PLAYERS_COUNT;
        public MinMax(int id, int depth = 0, int width = 0, bool useAllStrategies = true) : base(id)
        {
            MAX_DEPTH = depth;
            MAX_WIDTH = width;
            MyBase = Halma.PlayerBaseCoords(id);
            EnemyBase = Halma.EnemyPlayerBaseCoords(id);
            PrevMoves = new HashSet<MoveContainer>();
            
            if(useAllStrategies)
            SetStrategies(HDistanceToEnemyBase,
                HDistanceToNearestFreeFieldInEnemyBase,
                HPrioritizeJumpMoves,
                HNumberOfPawnsInMyBase,
                HMaxTraverseDistance,
                HAvoidBlockingMovesInBase
                );
        }



        //maybe add width boundary ??
        //perform move filtering based on heuristics
        //public void SeedTree(int[,] gameBoardState, HashSet<Halma.Move> initPossibleMoves)
        //{
        //    NearBaseFlag = false;
        //    EnemyNearBase ??= NearBase(EnemyBase, gameBoardState);
        //    MyNearBase ??= NearBase(MyBase, gameBoardState);
        //    GameStateTree = new GameStateTree(new GameStateTreeNode(gameBoardState,Id,0));
        //    int[,] nextBoardState;
        //    GameStateTreeNode node;
        //    int currentDepth = 1;
        //    int currentWidth = 0;
        //    foreach (var moveContainer in AdaptMoves(initPossibleMoves, Id, gameBoardState))
        //    {
        //        if (MAX_WIDTH > 0 && currentWidth > MAX_WIDTH) break;
        //        nextBoardState = Halma.PerformMoveWithCopy(gameBoardState, moveContainer.Move, Id);
        //        node = new GameStateTreeNode(nextBoardState, Id, moveContainer.Priority);
        //        GameStateTree.AddEdge(GameStateTree.Root, node, moveContainer.Move);
        //        SeedRest(node, moveContainer.Move.From, Id, currentDepth);
        //        currentWidth++;
        //    }

        //}



        //private void SeedRest(GameStateTreeNode node, Halma.PawnCoord movedFrom, int pawn, int currentDepth)
        //{
        //    if (MAX_DEPTH > 0 && currentDepth > MAX_DEPTH)
        //        return;
        //    //Console.WriteLine($"CurrentDepth: {currentDepth} move of {pawn}");
        //    //PrintBoard(node);
        //    //Thread.Sleep(1000);
        //    pawn = NextPlayer(pawn);
        //    var possibleMoves = AdaptMoves(Halma.GivePossibleMoves(node.GameBoardState, pawn).Where(m => !m.To.Equals(movedFrom)), pawn, node.GameBoardState);
        //    //Console.WriteLine(possibleMoves.Count());
        //    //if (pawn == 1 && movedFrom.X == 1)
        //    //    Console.WriteLine();
        //    if (possibleMoves.Any(m => m.Move.To.Equals(movedFrom)))
        //        Console.WriteLine();
        //    int[,] nextBoardState;
        //    GameStateTreeNode nextNode;

        //    int currentWidth = 0;
        //    foreach (var moveContainer in possibleMoves)
        //    {
        //        if (MAX_WIDTH > 0 && currentWidth > MAX_WIDTH) break;

        //        nextBoardState = Halma.PerformMoveWithCopy(node.GameBoardState, moveContainer.Move, pawn);
        //        nextNode = new GameStateTreeNode(nextBoardState, pawn, moveContainer.Priority);
        //        //Console.WriteLine($"CurrentDepth: {currentDepth} move of {pawn}");
        //        //PrintBoard(nextNode);
        //        GameStateTree.AddEdge(node, nextNode, moveContainer.Move);
        //        SeedRest(nextNode, moveContainer.Move.From, pawn, currentDepth + 1);
        //        currentWidth++;
        //    }
        //}

        //optimized:
        //public void SeedTree(int[,] gameBoardState, HashSet<Halma.Move> initPossibleMoves)
        //{
        //    NearBaseFlag = false;
        //    EnemyNearBase ??= NearBase(EnemyBase, gameBoardState);
        //    MyNearBase ??= NearBase(MyBase, gameBoardState);
        //    GameStateTree = new GameStateTree(new GameStateTreeNode(gameBoardState, Id, 0));
        //    int currentDepth = 1;
        //    HashSet<MoveContainer> prevMoveContainers = new HashSet<MoveContainer>(MAX_WIDTH);
        //    foreach (var moveContainer in AdaptMoves(initPossibleMoves, Id, gameBoardState))
        //    {
        //        if (MAX_WIDTH > 0 && GameStateTree.ChildrenCount(GameStateTree.Root) > MAX_WIDTH)
        //            break;

        //        if (prevMoveContainers.Contains(moveContainer) || !ValidateMoveContainer(moveContainer,Id, gameBoardState)) continue;

        //        int[,] nextBoardState = Halma.PerformMoveWithCopy(gameBoardState, moveContainer.Move, Id);
        //        GameStateTreeNode? node = GameStateTree.CreateNode(gameBoardState, nextBoardState, Id, moveContainer.Priority, moveContainer.Move);
        //        GameStateTree.AddEdge(GameStateTree.Root, node, moveContainer.Move);
        //        prevMoveContainers.Add(moveContainer);
        //        SeedRest(node, moveContainer.Move.From, Id, currentDepth);
        //    }
        //}



        //private void SeedRest(GameStateTreeNode node, Halma.PawnCoord movedFrom, int pawn, int currentDepth)
        //{
        //    if ((MAX_DEPTH > 0 && currentDepth > MAX_DEPTH))
        //        return;

        //    pawn = NextPlayer(pawn);


        //    int currentWidth = 0;

        //    //lock(node)
        //    //{   
        //    var possibleMoves = AdaptMoves(Halma.GivePossibleMoves(node.GameBoardState, pawn).Where(m => !m.To.Equals(movedFrom)), pawn, node.GameBoardState);
        //    var prevMoveContainers = new HashSet<MoveContainer>(MAX_WIDTH);
        //    //lock (possibleMoves)
        //    Parallel.ForEach(possibleMoves, new ParallelOptions {MaxDegreeOfParallelism = -1}, (moveContainer, loopState) =>
        //    {

        //        if (MAX_WIDTH > 0 && currentWidth < MAX_WIDTH)
        //        {
        //            int[,] nextBoardState = Halma.PerformMoveWithCopy(node.GameBoardState, moveContainer.Move, pawn);
        //            GameStateTreeNode? nextNode = GameStateTree.CreateNode(node.GameBoardState, nextBoardState, pawn, moveContainer.Priority, moveContainer.Move);

        //            if (nextNode != null && GameStateTree.AddEdge(node, nextNode, moveContainer.Move))
        //            {
        //                SeedRest(nextNode, moveContainer.Move.From, pawn, currentDepth + 1);
        //                currentWidth++;
        //            }
        //        }
        //        else
        //        {
        //            loopState.Break();
        //            return;
        //        }
        //    });
        //    //}

        //}

        bool ValidateMoveContainer(MoveContainer moveContainer, int pawn, int[,] gameBoardState)
        {
            return GameStateTree.ValidateMoveToBoardState(moveContainer.Move, pawn, gameBoardState);
        }

        //public void SeedTree(int[,] gameBoardState, HashSet<Halma.Move> initPossibleMoves)
        //{
        //    NearBaseFlag = false;
        //    EnemyNearBase ??= NearBase(EnemyBase, gameBoardState);
        //    MyNearBase ??= NearBase(MyBase, gameBoardState);
        //    GameStateTree = new GameStateTree(new GameStateTreeNode(gameBoardState, Id, 0));
        //    int currentDepth = 1;
        //    int currentWidth = 0;
        //    int nodesAtCurrentLevel = 1; // Start with the root node
        //    int nodesAtNextLevel = 0;
        //    int pawn = 0;
            

        //    ConcurrentQueue<GameStateTreeNode> queue = new ConcurrentQueue<GameStateTreeNode>();
        //    queue.Enqueue(GameStateTree.Root);

        //    while (queue.Count > 0 && currentDepth <= MAX_DEPTH)
        //    {
                
        //        Parallel.ForEach(queue, node =>
        //        {
        //            pawn = node.Equals(GameStateTree.Root) ? Id : NextPlayer(pawn);
        //            var prevMoveContainers = new HashSet<MoveContainer>(MAX_WIDTH);
        //            var possibleMoves = AdaptMoves
        //            (
        //                Halma.GivePossibleMoves(node.GameBoardState, pawn), 
        //                pawn, 
        //                node.GameBoardState
        //            );

        //            foreach (var moveContainer in possibleMoves)
        //            {
        //                if (prevMoveContainers.Contains(moveContainer) ||
        //                    !ValidateMoveContainer(moveContainer, pawn, node.GameBoardState))
        //                {
        //                    continue;
        //                }

        //                int[,] nextBoardState = Halma.PerformMoveWithCopy(node.GameBoardState, moveContainer.Move, pawn);
        //                GameStateTreeNode? nextNode = GameStateTree.CreateNode(node.GameBoardState, nextBoardState, pawn, moveContainer.Priority, moveContainer.Move);

        //                if (nextNode != null && GameStateTree.AddEdge(node, nextNode, moveContainer.Move))
        //                {
        //                    prevMoveContainers.Add(moveContainer);
        //                    queue.Enqueue(nextNode);
        //                    Interlocked.Increment(ref nodesAtNextLevel);

        //                    if (Interlocked.Increment(ref currentWidth) >= MAX_WIDTH || Interlocked.CompareExchange(ref nodesAtNextLevel, 0, 0) >= MAX_WIDTH)
        //                    {
        //                        currentDepth = MAX_DEPTH + 1; // Exit loop
        //                        break;
        //                    }
        //                }
        //            }
        //        });

        //        nodesAtCurrentLevel = nodesAtNextLevel;
        //        nodesAtNextLevel = 0;
        //        currentWidth = 0;
        //        currentDepth++;
        //    }
        //}

        public void SeedTree(int[,] gameBoardState, HashSet<Halma.Move> initPossibleMoves)
        {
            NearBaseFlag = false;
            EnemyNearBase ??= NearBase(EnemyBase, gameBoardState);
            MyNearBase ??= NearBase(MyBase, gameBoardState);
            GameStateTree = new GameStateTree(new GameStateTreeNode(0, gameBoardState, Id, 0));
            int currentDepth = 1;
            int nodesAtCurrentLevel = 0; // Start with the root node
            int nodesAtNextLevel = 0;
            int pawn = 0;
             

            ConcurrentQueue<GameStateTreeNode> queue = new ConcurrentQueue<GameStateTreeNode>();
            queue.Enqueue(GameStateTree.Root);

            while (queue.Count > 0 && currentDepth <= MAX_DEPTH)
            {
                Parallel.ForEach(queue, node =>
                {
                    pawn = node.Equals(GameStateTree.Root) ? Id : NextPlayer(pawn);
                    NearBaseFlag = false;
                    var prevMoveContainers = new HashSet<MoveContainer>(MAX_WIDTH);
                    var possibleMoves = AdaptMoves
                    (
                        Halma.GivePossibleMoves(node.GameBoardState, pawn),
                        pawn,
                        node.GameBoardState
                    );

                    int nodeMaxWidth = nodesAtCurrentLevel == 0 ? MAX_WIDTH : (MAX_WIDTH / nodesAtCurrentLevel); // Adjusted max width for the current node
                    //Console.WriteLine(nodesAtCurrentLevel);
                    int currentWidth = 0; // Reset current width for each node

                    foreach (var moveContainer in possibleMoves)
                    {
                        if (prevMoveContainers.Contains(moveContainer) ||
                            !ValidateMoveContainer(moveContainer, pawn, node.GameBoardState)) //||
                            //PrevMoves.Contains(moveContainer))
                        {
                            continue;
                        }

                        if (currentWidth >= nodeMaxWidth)
                        {
                            break; // Break loop if the current node reaches its max width
                        }

                        int[,] nextBoardState = Halma.PerformMoveWithCopy(node.GameBoardState, moveContainer.Move, pawn);
                        GameStateTreeNode? nextNode = GameStateTree.CreateNode(node.GameBoardState, nextBoardState, pawn, moveContainer.Priority, moveContainer.Move);

                        if (nextNode != null && GameStateTree.AddEdge(node, nextNode, moveContainer.Move))
                        {
                            prevMoveContainers.Add(moveContainer);
                            queue.Enqueue(nextNode);
                            Interlocked.Increment(ref nodesAtNextLevel);
                            Interlocked.Increment(ref currentWidth); // Increment current width for each added child
                        }
                    }
                });

                nodesAtCurrentLevel = nodesAtNextLevel;
                nodesAtNextLevel = 0;
                currentDepth++;
            }
        }


        public class MoveContainer : IComparable<MoveContainer>
        {
            public int Priority { get; set; }
            public Halma.Move Move { get; set; }

            public MoveContainer(int priority, Halma.Move move)
            {
                Priority = priority;
                Move = move;
            }

            public override bool Equals(object? obj)
            {
                return obj is MoveContainer container &&
                       Priority == container.Priority &&
                       EqualityComparer<Halma.Move>.Default.Equals(Move, container.Move);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Priority, Move);
            }

            int IComparable<MoveContainer>.CompareTo(MoveContainer? other)
            {
                if (other == null) throw new ArgumentNullException("MoveContainer compared to null");
                return this.Priority.CompareTo(other.Priority);
            }
        }

        //values from 0 to 100
        //percentage of maximal manhattan distance for board between enemy base and pawns position...
        public int HDistanceToEnemyBase(Halma.Move move, int pawn, int[,]? gameBoardState = null)
        {
            var gameBase = pawn == Id ? EnemyBase : MyBase;

            if (IsBaseFull(gameBase, gameBoardState) || (gameBase.Contains(move.To) && gameBase.Contains(move.From))) return 0;

            int maxPosition = Halma.BOARD_SIZE - 1;
            Halma.PawnCoord? baseFirstField = gameBase.FirstOrDefault(p => (p.X == 0 && p.Y == maxPosition) || (p.X == maxPosition && p.Y == 0));
            var positionTo = move.To;
            return 100 - positionTo.CalcDistance(baseFirstField).PercantageWithMultiplier(MAX_DISTANCE_MANHATTAN);
        }

        public int HMaxTraverseDistance(Halma.Move move, int pawn, int[,]? gameBoardState = null)
        {
            if (gameBoardState == null) return 0;

            int distance = move.From.CalcDistance(move.To);

            return distance.PercantageWithMultiplier(MAX_DISTANCE_MANHATTAN, 3);
        }

        public int HAvoidBlockingMovesInBase(Halma.Move move, int pawn, int[,]? gameBoardState = null)
        {
            if (gameBoardState == null) return 0;

            int enemyPawn = NextPlayer(pawn);
            var baseBoard = pawn == Id ? MyBase : EnemyBase;

            if(NumberOfPawnsInBase(baseBoard,pawn,gameBoardState) < NumberOfPawnsInBase(baseBoard, enemyPawn, gameBoardState))
            {
                if (baseBoard.Contains(move.From)) return 100;
                return -100;
                
            }

            return 0;
        }

        public int HDistanceToNearestFreeFieldInEnemyBase(Halma.Move move, int pawn, int[,]? gameBoardState = null)
        {
            var gameBase = pawn == Id ? EnemyBase : MyBase;
            if (IsBaseFull(gameBase, gameBoardState))
                return 0;

            if(gameBase.Contains(move.To) && gameBase.Contains(move.To) && NearBaseFlag)
            {
                return 100;
            }

            if(IsNearBase(move, gameBoardState)  && gameBase.Contains(move.To) && gameBoardState[move.To.X, move.To.Y] == 0)
                return 99;
            

            var bestField = gameBase.Where(p => gameBoardState[p.X, p.Y] == 0).OrderBy(p => p.CalcDistance(move.To)).First();
            
            return 100 - bestField.CalcDistance(move.To).PercantageWithMultiplier(MAX_DISTANCE_MANHATTAN);
        }

        public int HPrioritizeJumpMoves(Halma.Move move, int pawn, int[,]? gameBoardState = null)
        {
            if (IsJumpMove(move, gameBoardState))
            {
                return 99;
            }
            else
            {             
                int score = 0;
                int availableMoves = 1;

                foreach (var neighbor in move.To.Neighbors())
                {
                 
                    if (gameBoardState != null && gameBoardState[neighbor.X, neighbor.Y] != 0)
                    {
                       availableMoves++;
                        var jumpDestination = neighbor + (neighbor - move.To);
                        if (Halma.IsInsideBoard(jumpDestination) && gameBoardState[jumpDestination.X, jumpDestination.Y] == 0)
                        {
                           
                            score++; 
                        }
                    }
                }

                return score.PercantageWithMultiplier(availableMoves == 0 ? 8 : availableMoves);
            }
        }

        private bool IsJumpMove(Halma.Move move, int[,]? gameBoardState)
        {
          
            return move.From.CalcDistance(move.To) > 1; 
        }

        public int NumberOfPawnsInBase(HashSet<Halma.PawnCoord> boardBase, int pawn, int[,]? gameBoardState, bool restPawnsResult = false)
        {
            int acc = 0;
            foreach (var item in boardBase)
            {
                if (!restPawnsResult)
                {
                    if (gameBoardState[item.X, item.Y] == pawn)
                        acc++;
                } else
                {
                    if (gameBoardState[item.X, item.Y] != pawn)
                        acc++;
                }
                
            }
            return acc;
        }

        public int HNumberOfPawnsInMyBase(Halma.Move? move, int pawn, int[,]? gameBoardState = null)
        {
            var boardBase = pawn == Id ? MyBase : EnemyBase;
            if (boardBase == null || gameBoardState == null) return 0;

            
            int prevPawnCount = NumberOfPawnsInBase(boardBase,pawn,gameBoardState, restPawnsResult: false);

            if (move != null)
            {
                if (boardBase.Contains(move.To) && boardBase.Contains(move.From)) return 100;

                int[,] afterMove = Halma.PerformMoveWithCopy(gameBoardState, move, pawn);

                int currentPawnCount = NumberOfPawnsInBase(boardBase, pawn, afterMove, restPawnsResult: false);

                if (currentPawnCount < prevPawnCount) return 99;
            }

            return -prevPawnCount.PercantageWithMultiplier(PAWNS_NUMBER,3);
        }

        public bool IsNearBase(Halma.Move move, int[,]? gameBoardState)
        {
            int pawn = gameBoardState[move.From.X, move.From.Y];
            var nearBase = pawn == Id ? EnemyNearBase : MyNearBase;
            bool result = nearBase.Contains(move.From);
            if (result && !NearBaseFlag) NearBaseFlag = true;
            return result;
        }

        public HashSet<Halma.PawnCoord> NearBase(HashSet<Halma.PawnCoord> gameBase, int[,]? gameBoardState)
        {
            var nearBase = new HashSet<Halma.PawnCoord>(11);
            //int maxPosition = Halma.BOARD_SIZE - 1;
            //Halma.PawnCoord? baseFirstField = gameBase.FirstOrDefault(p => (p.X == 0 && p.Y == maxPosition) || (p.X == maxPosition && p.Y == 0));

            foreach (var p in gameBase) //gameBoardState[pc.X, pc.Y] == 0
            {
                var neighbours = p.Neighbors().Where(pc => !gameBase.Contains(pc));
                nearBase.UnionWith(neighbours);
            }

            //gameBase.ToList().ForEach(p =>
            //{
            //    var neighbours = p.Neighbors().Where(pc => gameBoardState[pc.X, pc.Y] == 0);
            //    nearBase.Union(neighbours);
            //});
            return nearBase;
        }


        public bool IsBaseFull(HashSet<Halma.PawnCoord> gameBase, int[,]? gameBoardState)
        {
            return gameBoardState != null && !gameBase.Any(p => gameBoardState[p.X, p.Y] == 0);
        }

        private IEnumerable<MoveContainer> AdaptMoves(IEnumerable<Halma.Move> possibleMoves, int pawn, int[,]? gameBoardState = null)
        {
            var adaptedMoves = possibleMoves.
                Where(m => CheckIfNotGoingBack(m, pawn, gameBoardState));
            adaptedMoves = adaptedMoves.Count() == 0 ? possibleMoves : adaptedMoves; 

            var adaptedMovesContainer = adaptedMoves.Select(m => new MoveContainer(CalcPriority(m, pawn, gameBoardState), m));

            adaptedMovesContainer = adaptedMovesContainer.OrderByDescending(m => m.Priority);
            //maybe prioritize here?? not inside strategies
            //if (pawn == Id && pawn == 2 && IsNearBase(adaptedMoves.First().Move, gameBoardState))
            //    Console.WriteLine();

            return adaptedMovesContainer;
        }

        public bool CheckIfNotGoingBack(Halma.Move move, int pawn, int[,] gameBoardState)
        {
            if (this.LastMove == null || gameBoardState == null) return true;

            var lastDistance = HDistanceToEnemyBase(this.LastMove, pawn, gameBoardState);
            var currDistance = HDistanceToEnemyBase(move, pawn, gameBoardState);

            return !move.To.Equals(this.LastMove.From) && !move.From.Equals(this.LastMove.From) 
                && (pawn == Id ? (currDistance > lastDistance) : true);
        } 

        private int CalcPriority(Halma.Move move, int pawn, int[,]? gameBoardState = null)
        {
            int acc = 0;
            Strategies.ForEach(strategy => { acc += strategy(move, pawn, gameBoardState); });
            return acc;
        }

        private void PrintBoard(GameStateTreeNode node)
        {
            InnerHalma.GameBoardState = node.GameBoardState;
            Console.WriteLine(InnerHalma.ToString());
            Console.WriteLine();
        }
         
        public int NextPlayer(int id)
        {
            return id % Halma.PLAYERS_COUNT + 1;
        }

        public override string ToString()
        {
            return $"Player ({this.GetType().Name}) {Id}";
        }



        //adding async loader when round diff exceeds depth?
        internal override Halma.Move? ChooseMove(int[,] gameBoardState, ref Dictionary<char, int> alphabet, out Halma.Move? invalidMove, HashSet<Halma.Move>? possibleMoves = null)
        {
            Console.WriteLine();
            var now = DateTime.Now;
            Console.WriteLine($"Start seeding game state tree of {this} at {now.ToShortTimeString()}:");
            SeedTree(gameBoardState, possibleMoves);
            var end = DateTime.Now;
            Console.WriteLine($"Seed completed at {end.ToShortTimeString()}: {(end - now).TotalSeconds}s");
            var root = GameStateTree.Root;

            //HashSet<Move> prevMoves = new HashSet<Move>(100000);

            MoveContainer moveContainer = MinMaxAlgorithm(root, MAX_DEPTH, true);


            PrevMoves.Add(moveContainer);
            Console.WriteLine($"Selected move of {this} : {Halma.MoveToString(moveContainer.Move)}");

            invalidMove = null;
            this.LastMove = moveContainer.Move;
            return moveContainer.Move;
        }


        protected virtual MoveContainer MinMaxAlgorithm(GameStateTreeNode node, int depth, bool maximizingPlayer)
        {

            if (depth == 0 || GameStateTree.Children(node).Count() == 0)
            {
                return EvaluateGameState(node);
            }


            if (maximizingPlayer)
            {
                MoveContainer maxEval = new MoveContainer(int.MinValue, null);
                
                foreach (var child in GameStateTree.Children(node))
                {
                    MoveContainer eval = MinMaxAlgorithm(child.NextNode, depth - 1, false);
                    if(eval.Priority > maxEval.Priority) 
                    {
                        maxEval = eval;
                        maxEval.Move = child.Move;
                        child.NextNode.SourceMove = maxEval.Move;
                    }
                    
                }
                return maxEval;
            }
            else
            {
                MoveContainer minEval = new MoveContainer(int.MaxValue, null);
                foreach (var child in GameStateTree.Children(node))
                {
                    MoveContainer eval = MinMaxAlgorithm(child.NextNode, depth - 1, true);
                    if (eval.Priority < minEval.Priority) 
                    {
                        minEval = eval;
                        minEval.Move = child.Move;
                        child.NextNode.SourceMove = minEval.Move;
                    }
                }
                return minEval;
            }
        }
        protected virtual MoveContainer EvaluateGameState(GameStateTreeNode node)
        {
            int heuristicPriority = 0;
            int pawn = node.CurrentPlayer;
            
            if (InnerHalma.CheckBoardForWin(node.GameBoardState, node.CurrentPlayer))
            {
                heuristicPriority = 100; 
            }
            else
            { 
                
                int myDistance = CalculateAverageDistanceToEnemyBase(node.GameBoardState, pawn);
                //int enemyDistance = CalculateAverageDistanceToBase(node.GameBoardState, NextPlayer(pawn));

                // Adjust the priority based on the difference in average distance
                heuristicPriority += myDistance.PercantageWithMultiplier(MAX_DISTANCE_MANHATTAN) * 
                    (Strategies.Count == 0 ? 1 : Strategies.Count);

                heuristicPriority += HNumberOfPawnsInMyBase(null, node.CurrentPlayer, node.GameBoardState);
                //int myMovesCount = GivePossibleMoves(node.GameBoardState, pawn).Count();
                //int enemyMovesCount = GivePossibleMoves(node.GameBoardState, NextPlayer(pawn)).Count();

                //heuristicPriority += (myMovesCount - enemyMovesCount);



            }

            return new MoveContainer(heuristicPriority, node.SourceMove); ; //node.SourceMove
        }

        private int CalculateAverageDistanceToEnemyBase(int[,] gameBoardState, int playerId)
        {
            int totalDistance = 0;
            int pawnsCount = 0;
            var playerBase = playerId == Id ? EnemyBase : MyBase;
            foreach (var pawnCoord in playerBase)
            {
                for (int i = 0; i < Halma.BOARD_SIZE; i++)
                {
                    for (int j = 0; j < Halma.BOARD_SIZE; j++)
                    {
                        if (gameBoardState[i, j] == playerId)
                        {
                            totalDistance += pawnCoord.CalcDistance(new PawnCoord(i, j));
                            pawnsCount++;
                        }
                    }
                }
            }
            return (pawnsCount == 0 ? 0 : totalDistance / pawnsCount);
        }
    }
}
