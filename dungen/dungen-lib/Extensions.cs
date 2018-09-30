using System;
using System.Collections.Generic;
using System.Text;

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
  }
}
