using DunGen.Algorithm;
using DunGen.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DunGen.Algorithm
{
  /// <summary>
  /// Stores information about an Algorithm in a way that can be loaded/saved without
  /// needing the assembly immediately. This allows users to pass information about
  /// groups of Algorithms, not all of which may be loaded, without special care for
  /// handling errors. Only if the user attempts to actually instantiate & run an
  /// un-loaded Algorithm will failures occur.
  /// </summary>
  [DataContract(Name = "algInfo")]
  public class AlgorithmInfo : ICloneable
  {
    [DataMember(Name = "type", Order = 1, IsRequired = true)]
    public AlgorithmType Type { get; set; } = null;

    [DataMember(Name = "params", Order = 2, IsRequired = true)]
    public AlgorithmParams Parameters { get; set; } = new AlgorithmParams();

    public object Clone()
    {
      return new AlgorithmInfo()
      {
        Type = new AlgorithmType(this.Type.ConvertToType(false)),
        Parameters = (AlgorithmParams)this.Parameters.Clone()
      };
    }

    public IAlgorithm CreateInstance()
    {
      IAlgorithm alg = AlgorithmPluginEnumerator.GetAlgorithm(Type.ConvertToType(true));

      if (null != alg && alg.TakesParameters)
      {
        alg.Parameters = this.Parameters;
      }

      return alg;
    }
  }

  public static partial class Extensions
  {
    public static IAlgorithm ToInstance(this AlgorithmInfo info)
    {
      return info.CreateInstance();
    }

    public static AlgorithmInfo ToInfo(this IAlgorithm alg)
    {
      return new AlgorithmInfo()
      {
        Type = new AlgorithmType(alg.GetType()),
        Parameters = alg.TakesParameters ? alg.Parameters : new AlgorithmParams()
      };
    }
  }
}
