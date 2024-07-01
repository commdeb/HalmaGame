using Microsoft.VisualBasic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Lab2Halma.Halma;
using static Lab2Halma.Skynet.MinMax;


namespace Lab2Halma.Skynet
{
   
        public class GameStateTreeNode
        {
            public int[,] GameBoardState { get; }
            internal int CurrentPlayer { get; }
            public int Cost { get; }

            internal Move? SourceMove { get; set; }

            internal int Id { get; set; }
        

            internal GameStateTreeNode(int id, int[,] gameBoardState, int currentPlayer, int cost)
            {
                GameBoardState = gameBoardState;
                CurrentPlayer = currentPlayer;
                Cost = cost;
                Id = id;
            }

            public override bool Equals(object? obj)
            {
                return obj is GameStateTreeNode node &&
                       EqualityComparer<int[,]>.Default.Equals(GameBoardState, node.GameBoardState) &&
                       CurrentPlayer == node.CurrentPlayer &&
                       Cost == node.Cost &&
                       EqualityComparer<Move?>.Default.Equals(SourceMove, node.SourceMove);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(GameBoardState, CurrentPlayer, Cost, SourceMove);
            }
        }

        public class GameStateTreeEdge
        {
            internal Move Move { get; }
            internal GameStateTreeNode NextNode { get; }

            internal GameStateTreeEdge(Move move, GameStateTreeNode nextNode)
            {
                Move = move;
                NextNode = nextNode;
            }

        public override bool Equals(object? obj)
        {
            return obj is GameStateTreeEdge edge &&
                   EqualityComparer<Move>.Default.Equals(Move, edge.Move) &&
                   EqualityComparer<GameStateTreeNode>.Default.Equals(NextNode, edge.NextNode);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Move, NextNode);
        }
    }

    public class GameStateTree
    {
        public GameStateTreeNode Root { get; }

        internal int IdCounter = 0;
        private ConcurrentDictionary<GameStateTreeNode, List<GameStateTreeEdge>> AdjacencyList { get; }

        public GameStateTree(GameStateTreeNode root)
        {
            Root = root;
            AdjacencyList = new ConcurrentDictionary<GameStateTreeNode, List<GameStateTreeEdge>>();
            AdjacencyList[root] = new List<GameStateTreeEdge>();
            IdCounter = 1;
        }

        public void AddNode(GameStateTreeNode node)
        {
            AdjacencyList.TryAdd(node, new List<GameStateTreeEdge>());
        }

        internal bool AddEdge(GameStateTreeNode source, GameStateTreeNode destination, Move move)
        {
            var edge = new GameStateTreeEdge(move, destination);

            if (AdjacencyList.TryGetValue(source, out List<GameStateTreeEdge>? edges))
            {
                lock (edges)
                    if (edges.Contains(edge)) return false;
            }

            AdjacencyList.AddOrUpdate(source, new List<GameStateTreeEdge> { edge }, (key, oldValue) =>
            {
                lock (oldValue)
                {
                    oldValue.Add(edge);
                }
                return oldValue;
            });

            // If destination node doesn't exist, add it to the tree
            AddNode(destination);
            return true;
        }
        internal static bool ValidateMoveToBoardState(Move move, int pawn, int[,] gameBoardState)
        {
            if (move == null) return false;

            //return gameBoardState[move.From.X, move.From.Y] == pawn;
            return true;
        }

        internal IEnumerable<GameStateTreeEdge> Children(GameStateTreeNode node)
        {
            return AdjacencyList.TryGetValue(node, out var edges) ? edges : Enumerable.Empty<GameStateTreeEdge>();
        }

        public int ChildrenCount(GameStateTreeNode node)
        {
            return Children(node).Count();
        }

        internal GameStateTreeNode? CreateNode(int[,] prevGameBoardState, int[,] gameBoardState, int currentPlayer, int priority, Halma.Move move)
        {
            if (!ValidateMoveToBoardState(move, currentPlayer, prevGameBoardState)) return null;
            var newNode = new GameStateTreeNode(IdCounter++,gameBoardState, currentPlayer, priority);
            newNode.SourceMove = move;
            AddNode(newNode);
            return newNode;
        }
    }
}
