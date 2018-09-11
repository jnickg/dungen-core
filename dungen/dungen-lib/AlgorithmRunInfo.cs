using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DunGen
{
  [CollectionDataContract(Name = "runList", ItemName = "algRunInfo")]
  public class AlgorithmRunInfoList : List<AlgorithmRunInfo>
  {

  }

  [DataContract(Name = "algRunInfo")]
  public class AlgorithmRunInfo
  {
    private string _fullName = string.Empty;

    [DataMember(Name = "algType", Order = 1)]
    public string TypeName
    {
      get => _fullName;
      set
      {
        if (null == FindAlgorithm(value))
        {
          throw new AlgorithmTypeNotFoundException("Unable to initialize AlgorithmRunInfo - TypeName is not an available Algorithm type")
          {
            TypeName = value
          };
        }
        _fullName = value;
      }
    }
    [DataMember(Name = "params", Order = 3)]
    public AlgorithmParams Parameters { get; set; } = new AlgorithmParams();

    [DataMember(Name = "mask", Order = 4)]
    public BoolCollection Mask { get; set; } = null;

    [DataMember(Name = "r", Order = 2)]
    public int RandomSeed { get; set; } = default(int);

    public AlgorithmRunInfo()
    { }

    public AlgorithmRun ReconstructRun()
    {
      return new AlgorithmRun()
      {
        Alg = RecreateAlgorithmInstance(this),
        Context = new AlgorithmContextBase()
        {
          R = new AlgorithmRandom(RandomSeed),
          Mask = this.Mask.UnJaggedize(),
          D = null // Generator should set this
        },
      };
    }

    public static AlgorithmRunInfo CreateFrom(AlgorithmRun run)
    {
      return new AlgorithmRunInfo()
      {
        TypeName = run.Alg.GetType().FullName,
        Parameters = run.Alg.TakesParameters ? run.Alg.Parameters : new AlgorithmParams(),
        Mask = run.Context.Mask.Jaggedize_DC(),
        RandomSeed = run.Context.R.Seed
      };
    }

    private static IAlgorithm RecreateAlgorithmInstance(AlgorithmRunInfo info)
    {
      IAlgorithm alg = FindAlgorithm(info.TypeName);

      if (null != alg)
      {
        alg.Parameters = info.Parameters;
      }

      return alg;
    }

    private static IAlgorithm FindAlgorithm(string typeName)
    {
      return AlgorithmPluginManager.GetAlgorithm(typeName);
    }
  }

  /// <summary>
  /// Shim data type used for the purpose of Data Contract serialization
  /// </summary>
  [CollectionDataContract(Name = "row", ItemName = "mask")]
  public class BoolList : List<bool> { }

  /// <summary>
  /// Shim data type used for the purpose of Data Contract serialization
  /// </summary>
  [CollectionDataContract(Name = "mask", ItemName = "row")]
  public class BoolCollection : List<BoolList> { }

  public static partial class Extensions
  {
    public static AlgorithmRunInfo ToInfo(this AlgorithmRun run)
    {
      return AlgorithmRunInfo.CreateFrom(run);
    }

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
