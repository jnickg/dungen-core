using DunGen.Algorithm;
using DunGen.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DunGen.Algorithm
{
  [DataContract(Name = "algInfo")]
  public class AlgorithmInfo
  {
    [DataMember(Name = "type", Order = 1, IsRequired = true)]
    public Type AlgorithmType { get; set; } = null;

    [DataMember(Name = "params", Order = 2, IsRequired = true)]
    public AlgorithmParams Parameters { get; set; } = new AlgorithmParams();

    public IAlgorithm CreateInstance()
    {
      IAlgorithm alg = AlgorithmPluginEnumerator.GetAlgorithm(AlgorithmType.FullName);

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
        AlgorithmType = alg.GetType(),
        Parameters = alg.TakesParameters ? alg.Parameters : new AlgorithmParams()
      };
    }
  }
}
