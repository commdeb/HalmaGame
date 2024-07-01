
using static Lab2Halma.Halma;

namespace Lab2Halma.Skynet
{
    internal class AlphaBeta : MinMax
    {
        public AlphaBeta(int id, int depth = 0, int width = 0, bool useAllStrategies = true) : base(id, depth, width, useAllStrategies)
        {
        }

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        protected override MoveContainer EvaluateGameState(GameStateTreeNode node)
        {
            return base.EvaluateGameState(node);
        }

        public MoveContainer MinMaxAlgorithmwithAlphaBetaPruning(GameStateTreeNode node, int depth, bool maximizingPlayer, int alpha, int beta)
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
                    MoveContainer eval = MinMaxAlgorithmwithAlphaBetaPruning(child.NextNode, depth - 1, false, alpha, beta);
                    if (eval.Priority > maxEval.Priority)
                    {
                        maxEval = eval;
                        maxEval.Move = child.Move;
                        child.NextNode.SourceMove = maxEval.Move;
                    }
                    alpha = Math.Max(alpha, eval.Priority);
                    if (beta <= alpha)
                        break; 
                }
                return maxEval;
            }
            else
            {
                MoveContainer minEval = new MoveContainer(int.MaxValue, null);
                foreach (var child in GameStateTree.Children(node))
                {
                    MoveContainer eval = MinMaxAlgorithmwithAlphaBetaPruning(child.NextNode, depth - 1, true, alpha, beta);
                    if (eval.Priority < minEval.Priority)
                    {
                        minEval = eval;
                        minEval.Move = child.Move;
                        child.NextNode.SourceMove = minEval.Move;
                    }
                    beta = Math.Min(beta, eval.Priority);
                    if (beta <= alpha)
                        break; 
                }
                return minEval;
            }
        }

        internal override Halma.Move? ChooseMove(int[,] gameBoardState, ref Dictionary<char, int> alphabet, out Halma.Move? invalidMove, HashSet<Halma.Move>? possibleMoves = null)
        {
            Console.WriteLine();
            var now = DateTime.Now;
            Console.WriteLine($"Start seeding game state tree of {this} at {now.ToShortTimeString()}:");
            SeedTree(gameBoardState, possibleMoves);
            var end = DateTime.Now;
            Console.WriteLine($"Seed completed at {end.ToShortTimeString()}: {(end - now).TotalSeconds}s");
            var root = GameStateTree.Root;
            

            MoveContainer moveContainer = MinMaxAlgorithmwithAlphaBetaPruning(root, MAX_DEPTH, true, 
                alpha: int.MinValue,
                beta: int.MaxValue
                );


            PrevMoves.Add(moveContainer);
            Console.WriteLine($"Selected move of {this} : {Halma.MoveToString(moveContainer.Move)}");

            invalidMove = null;
            this.LastMove = moveContainer.Move;
            return moveContainer.Move;
        }
    }
}
