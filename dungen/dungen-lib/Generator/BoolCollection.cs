using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace DunGen.Generator
{
  /// <summary>
  /// Shim data type used for the purpose of Data Contract serialization
  /// </summary>
  [CollectionDataContract(Name = "mask", ItemName = "row")]
  public class BoolCollection : List<BoolList> { }

  public static partial class Extensions
  {
    /// <summary>
    /// Converts this rectangular array into a jagged array.
    /// </summary>
    public static T[][] Jaggedize<T>(this T[,] input)
    {
      T[][] output = new T[input.GetLength(0)][];
      for (int i = 0; i < input.GetLength(0); i++)
      {
        output[i] = new T[input.GetLength(1)];
        for (int j = 0; j < input.GetLength(1); j++)
        {
          output[i][j] = input[i, j];
        }
      }
      return output;
    }

    /// <summary>
    /// If this jagged array is rectangular, converts it into a
    /// rectangular array.
    /// </summary>
    public static T[,] UnJaggedize<T>(this T[][] input)
    {
      foreach (T[] ary in input)
      {
        if (null == ary || input[0].Length != ary.Length)
        {
          throw new ArgumentException("Can't un-jaggedize a jagged jagged array");
        }
      }

      T[,] output = new T[input.Length, input[0].Length];
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
    /// Converts this rectangular array of bools into a jagged array, for the purposes
    /// of Data Contract (DC) serialization.
    /// </summary>
    public static BoolCollection Jaggedize_DC(this bool[,] input)
    {
      BoolCollection output = new BoolCollection();
      output.AddRange(new BoolList[input.GetLength(0)].AsEnumerable());
      for (int i = 0; i < input.GetLength(0); i++)
      {
        output[i] = new BoolList();
        output[i].AddRange(new bool[input.GetLength(1)].AsEnumerable());
        for (int j = 0; j < input.GetLength(1); j++)
        {
          output[i][j] = input[i, j];
        }
      }
      return output;
    }

    public static bool[,] UnJaggedize(this BoolCollection input)
    {
      foreach (BoolList ary in input)
      {
        if (null == ary || input[0].Count != ary.Count)
        {
          throw new ArgumentException("Can't un-jaggedize a jagged jagged array");
        }
      }

      bool[,] output = new bool[input.Count, input[0].Count];
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
