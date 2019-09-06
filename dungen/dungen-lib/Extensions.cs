using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using DunGen.Tiles;

namespace DunGen
{
  public static partial class Extensions
  {
    public static bool ContainsTrue(this bool[,] mask)
    {
      for (int y = 0; y < mask.GetLength(0); ++y)
      {
        for (int x = 0; x < mask.GetLength(1); ++x)
        {
          if (mask[y, x]) return true;
        }
      }
      return false;
    }

    public static T Random<T>(this IEnumerable<T> enumerable, Random r = null)
    {
      if (enumerable == null)
      {
        throw new ArgumentNullException(nameof(enumerable));
      }

      if (null == r) r = new Random();

      var list = enumerable as IList<T> ?? enumerable.ToList();
      return list.Count == 0 ? default(T) : list[r.Next(0, list.Count)];
    }

    public static bool IsIn(this Point p, bool[,] mask)
    {
      if (null == mask) return false;

      if (p.X < 0 || p.Y < 0 || p.X >= mask.GetLength(1) || p.Y >= mask.GetLength(0)) return false;

      return mask[p.Y, p.X];
    }

    public static bool ContainsAny(this bool[,] mask, IEnumerable<Tile> tiles)
    {
      return tiles.Any(t => t.Location.IsIn(mask));
    }

    public static bool ContainsAll(this bool[,] mask, IEnumerable<Tile> tiles)
    {
      return tiles.All(t => t.Location.IsIn(mask));
    }
  }
}
