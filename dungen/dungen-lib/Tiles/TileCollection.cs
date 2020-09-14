using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace DunGen.Tiles
{
  /// <summary>
  /// Shim data type used for the purpose of Data Contract serialization
  /// </summary>
  [CollectionDataContract(Name = "tiles", ItemName = "row", IsReference = true)]
  public class TileCollection : List<TileList> { }

  public static partial class Extensions
  {
    /// <summary>
    /// Converts this rectangular array of Tiles into a jagged array, for the purposes
    /// of serialization
    /// </summary>
    public static Tile[][] Jaggedize(this Tile[,] input)
    {
      Tile[][] output = new Tile[input.GetLength(0)][];
      for (int i = 0; i < input.GetLength(0); i++)
      {
        output[i] = new Tile[input.GetLength(1)];
        for (int j = 0; j < input.GetLength(1); j++)
        {
          output[i][j] = input[i, j];
        }
      }
      return output;
    }

    /// <summary>
    /// If this jagged array of Tiles is rectangular, converts it into a
    /// rectangular array, for the purposes of serialization
    /// </summary>
    public static Tile[,] UnJaggedize(this Tile[][] input)
    {
      foreach (Tile[] ary in input)
      {
        if (null == ary || input[0].Length != ary.Length)
        {
          throw new ArgumentException("Can't un-jaggedize a jagged jagged array");
        }
      }

      Tile[,] output = new Tile[input.Length, input[0].Length];
      for (int i = 0; i < input.Length; i++)
      {
        for (int j = 0; j < input[0].Length; j++)
        {
          output[i, j] = input[i][j];
        }
      }

      return output;
    }

    /// <summary>
    /// Converts this rectangular array of Tiles into a jagged array, for the purposes
    /// of Data Contract (DC) serialization.
    /// </summary>
    public static TileCollection Jaggedize_DC(this Tile[,] input)
    {
      TileCollection output = new TileCollection();
      output.AddRange(new TileList[input.GetLength(0)].AsEnumerable());
      for (int i = 0; i < input.GetLength(0); i++)
      {
        output[i] = new TileList();
        output[i].AddRange(new Tile[input.GetLength(1)].AsEnumerable());
        for (int j = 0; j < input.GetLength(1); j++)
        {
          output[i][j] = input[i, j];
        }
      }
      return output;
    }

    /// <summary>
    /// If this TileCollection is rectangular in form, converts it to a literal
    /// rectangular array of Tiles, for the purposes of serialization.
    /// </summary>
    public static Tile[,] UnJaggedize(this TileCollection input)
    {
      foreach (TileList ary in input)
      {
        if (null == ary || input[0].Count != ary.Count)
        {
          throw new ArgumentException("Can't un-jaggedize a jagged jagged array");
        }
      }

      Tile[,] output = new Tile[input.Count, input[0].Count];
      for (int i = 0; i < input.Count; i++)
      {
        for (int j = 0; j < input[0].Count; j++)
        {
          output[i, j] = input[i][j];
        }
      }

      return output;
    }
  }
}
