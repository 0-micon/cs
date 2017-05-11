using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussianDraughts
{
    class Program
    {
        static void Main(string[] args)
        {
            /*//
            var board = new Board();
            Game.StartingPosition(board);

            System.Console.WriteLine(board.ToString());

            List<Move> moves = new List<Move>(Game.NextMoves(board));

            Random rnd = new Random();
            int i = rnd.Next(moves.Count);

            Move m = moves[i];

            Console.WriteLine(m.ToString());

            var tmp = board[m.start];
            board[m.start] = board[m.end];
            board[m.end] = tmp;

            Console.WriteLine(board.ToString());

            board.SwapSides();

            Console.WriteLine(board.ToString());
            moves = new List<Move>(Game.NextMoves(board));
            m = moves[rnd.Next(moves.Count)];
            Console.WriteLine(m.ToString());

            tmp = board[m.start];
            board[m.start] = board[m.end];
            board[m.end] = tmp;

            Console.WriteLine(board.ToString());
            //*/

            var sol = new Solution();

            /*//
            sol.FirstIteration();
            sol.NextIteration();
            sol.NextIteration();
            sol.NextIteration();
            sol.NextIteration();
            sol.NextIteration();
            sol.NextIteration();
            sol.NextIteration();
            sol.NextIteration();
            sol.NextIteration();
            sol.NextIteration();
            sol.NextIteration();
            sol.NextIteration();

            sol.Save("boards");
            //*/
            sol.Load("boards", 13);

            var estimations = new SortedDictionary<int, List<Board>>();
            var last_ring = sol.Rings.Last();
            foreach (var board in last_ring)
            {
                //int key = board.Estimation;
                int key = Solution.Estimate(board, Pieces.White);
                if (!estimations.ContainsKey(key))
                {
                    estimations[key] = new List<Board>();
                }
                estimations[key].Add(board);
            }

            for (var pair = estimations.First(); pair.Key < 0; pair = estimations.First())
            {
                estimations.Remove(pair.Key);
                last_ring.RemoveItems(pair.Value);
            }

            for (int i = sol.Rings.Count - 2; i > 0; i -= 2)
            {
                // remove boards from the previous ring
                var prev_ring = sol.Rings[i];
                var items_to_del = new List<Board>(
                    //from board in prev_ring
                    //from next_board in Solution.NextBoards(board, Pieces.Black)
                    //where last_ring.BinarySearch(next_board) < 0 && sol.GetRing(next_board, Pieces.White) < 0
                    //select board
                    );
                foreach (var board in prev_ring)
                {
                    foreach (var next_board in Solution.NextBoards(board, Pieces.Black))
                    {
                        if (last_ring.BinarySearch(next_board) < 0 && sol.GetRing(next_board, Pieces.White) < 0)
                        {
                            items_to_del.Add(board);
                        }
                    }
                }

                items_to_del.DistinctValues();
                prev_ring.RemoveItems(items_to_del);

                // now, remove isolated boards
                var prev_prev_ring = sol.Rings[i - 1];
                items_to_del.Clear();

                foreach (var board in prev_prev_ring)
                {
                    bool alone = true;

                    foreach (var next_board in Solution.NextBoards(board, Pieces.White))
                    {
                        if (prev_ring.BinarySearch(next_board) >= 0)
                        {
                            alone = false;
                        }
                    }

                    if (alone)
                    {
                        items_to_del.Add(board);
                    }
                }

                items_to_del.DistinctValues();
                prev_prev_ring.RemoveItems(items_to_del);
            }

            //sol.NextIteration();
            //sol.NextIteration();

            sol.Save("boards.whites");

            var x = Console.ReadKey();
        }
    }
}
