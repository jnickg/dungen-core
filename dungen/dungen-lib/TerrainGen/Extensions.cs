using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DunGen.TerrainGen
{
  public static partial class Extensions
  {
    private static void FloodFillSearch_Recursive(this bool[,] target, int y, int x, ref bool[,] maskLayer, ref bool[,] searched)
    {
      // Case 0: x,y not in bounds
      if (x < 0 || y < 0 || x >= target.GetLength(1) || y >= target.GetLength(0)) return;
      // Case 1: point is isolated due to already being masked out
      if (!target[y, x]) return;
      // Case 2: not masked out, but we've already been here
      if (maskLayer[y, x] || searched[y, x]) return;
      // Case 3: Recurse!
      maskLayer[y, x] = true;
      searched[y, x] = true;

      target.FloodFillSearch_Recursive(y - 1, x, ref maskLayer, ref searched);
      target.FloodFillSearch_Recursive(y, x + 1, ref maskLayer, ref searched);
      target.FloodFillSearch_Recursive(y + 1, x, ref maskLayer, ref searched);
      target.FloodFillSearch_Recursive(y, x - 1, ref maskLayer, ref searched);
    }

    private static void FloodFillSearch_Queue(bool[,] map, int x, int y, ref bool[,] floodLayer, ref bool[,] maskFromSearch)
    {
      if (x < 0 || y < 0 || x >= map.GetLength(1) || y >= map.GetLength(0)) return;
      //  1. Set Q to the empty queue.
      Queue<Point> nodes = new Queue<Point>();
      //  2. If the color of node is not equal to target-color, return.
      if (!map[y, x] || maskFromSearch[y, x]) return;
      //  3. Add node to the end of Q.
      nodes.Enqueue(new Point(x, y));
      //  4. For each element n of Q:
      while (nodes.Count > 0)
      {
        Point n = nodes.Dequeue();
        // 4.a If element n is not the target-color, skip
        if (!map[n.Y, n.X] || maskFromSearch[n.Y, n.X]) continue;
        //  5.  Set w and e equal to n.
        Point w = n,
              e = n;
        //  6.  Move w to the west until the color of the node to the west of w no longer matches target-color.
        while (w.X > 0 && map[w.Y, w.X]) --w.X;
        //  7.  Move e to the east until the color of the node to the east of e no longer matches target-color.
        while (e.X < map.GetLength(1)-1&& map[e.Y, e.X]) ++e.X;
        //  8.  For each node n between w and e:
        for (int scan_x = w.X; scan_x <= e.X; ++scan_x)
        {
          //  9.  Set the color of nodes between w and e to replacement-color.
          floodLayer[n.Y, scan_x] = true;
          maskFromSearch[n.Y, scan_x] = true;
          // 10.   If the color of the node to the north of n is target-color, add that node to the end of Q.
          if (n.Y > 0 && map[n.Y - 1, scan_x] && !maskFromSearch[n.Y - 1, scan_x]) nodes.Enqueue(new Point(scan_x, n.Y - 1));
          // 11.   If the color of the node to the south of n is target-color, add that node to the end of Q.
          if (n.Y < map.GetLength(0) - 1 && map[n.Y + 1, scan_x] && !maskFromSearch[n.Y + 1, scan_x]) nodes.Enqueue(new Point(scan_x, n.Y + 1));
        }
        // 12. Continue looping until Q is exhausted.
      }
      // 13. Return.
      return;
    }

    public static List<bool[,]> SplitByAdjacency(this bool[,] mask)
    {
      List<bool[,]> splitMasks = new List<bool[,]>();
      bool[,] searched = new bool[mask.GetLength(0), mask.GetLength(1)];

      for (int i = 0; i < mask.GetLength(0); ++i)
		  {
			  for (int j = 0; j < mask.GetLength(1); ++j)
			  {
          if (!mask[i, j] || searched[i, j]) continue;
          bool[,] maskLayer = new bool[mask.GetLength(0), mask.GetLength(1)];
          FloodFillSearch_Queue(mask, j, i, ref maskLayer, ref searched);
          splitMasks.Add(maskLayer);
        }
      }

      return splitMasks;
    }

    public static T PickRandomly<T>(this IEnumerable<T> t, Random r = null)
    {
      if (null == r) r = new Random();
      IList<T> t_list = new List<T>(t);
      if (t_list.Count == 0) return default(T);
      return t_list[r.Next(t_list.Count)];
    }

    public static T PullRandomly<T>(this ICollection<T> t, Random r = null)
    {
      if (null == r) r = new Random();
      T rando = t.PickRandomly(r);
      t.Remove(rando);
      return rando;
    }
  }
}
