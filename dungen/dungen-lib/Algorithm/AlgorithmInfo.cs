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
    public SerializableType Type { get; set; } = null;

    [DataMember(Name = "params", Order = 2, IsRequired = true)]
    public AlgorithmParams Parameters { get; set; } = new AlgorithmParams();

    public virtual object Clone()
    {
      return new AlgorithmInfo()
      {
        Type = new SerializableType(this.Type.ConvertToType(false)),
        Parameters = (AlgorithmParams)this.Parameters.Clone()
      };
    }

    public virtual IAlgorithm CreateInstance()
    {
      IAlgorithm alg = AlgorithmPluginEnumerator.GetAlgorithm(Type.ConvertToType(true));

      if (null != alg && alg.TakesParameters)
      {
        alg.Parameters = this.Parameters;
      }

      return alg;
    }

    public override bool Equals(object obj)
    {
      AlgorithmInfo other = obj as AlgorithmInfo;
      if (null == other) return false;

      return this.Type == other.Type && 
             this.Parameters.Equals(other.Parameters);
    }
  }

  [CollectionDataContract(Name = "algorithmList", ItemName = "algorithm")]
  public class AlgorithmInfoList : List<AlgorithmInfo>
  {

  }

  [DataContract(Name = "compositeAlgInfo")]
  public class CompositeAlgorithmInfo : AlgorithmInfo
  {
    [DataMember(Name = "algInfos", IsRequired = true, Order = 1)]
    public AlgorithmInfoList Algorithms { get; set; } = new AlgorithmInfoList();

    [DataMember(Name = "name", IsRequired = true, Order = 0)]
    public string CompositeName { get; set; } = string.Empty;

    public override object Clone()
    {
      return null;
    }

    public override IAlgorithm CreateInstance()
    {
      CompositeAlgorithm alg = AlgorithmPluginEnumerator.GetAlgorithm(Type.ConvertToType(true)) as CompositeAlgorithm;

      if (null == alg)
      {
        throw new Exception("Failed to create composite algorithm from composite algorithm info");
      }

      alg.Algorithms = new AlgorithmList();
      alg.Algorithms.AddRange(this.Algorithms.Select(info => info.ToInstance()));

      alg.CompositeName = this.CompositeName;

      return alg;
    }

    public override bool Equals(object obj)
    {
      return base.Equals(obj);
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
      CompositeAlgorithm composite = alg as CompositeAlgorithm;
      if (composite != null)
      {
        return composite.ToInfo();
      }

      AlgorithmBase algBase = alg as AlgorithmBase;
      if (algBase == null)
      {
        throw new Exception("Failed to derive Base implementation for IAlgorithm");
      }
      return algBase.ToInfo();
    }
  }
}
