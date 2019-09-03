using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace DunGen.Algorithm
{
  [CollectionDataContract(Name = "algorithmList", ItemName = "algorithm")]
  [KnownType("GetKnownTypes")]
  public class AlgorithmList : List<IAlgorithm>
  {
    public static IEnumerable<Type> GetKnownTypes()
    {
      IEnumerable<Type> knownTypes = AlgorithmBase.GetKnownTypes();
      knownTypes.Concat(CompositeAlgorithm.GetKnownTypes());
      return knownTypes;
    }
  }

  /// <summary>
  /// An Algorithm which is comprised entirely of other, already-set algorithms.
  /// Contains no parameters of its own, and its name is not bound to its type.
  /// </summary>
  [DataContract(Name = "compositeAlgorithm")]
  public class CompositeAlgorithm : AlgorithmBase
  {
    [DataMember(Name = "algorithms", IsRequired = true, Order = 1)]
    public AlgorithmList Algorithms { get; set; } = new AlgorithmList();

    [DataMember(Name = "name", IsRequired = true, Order = 0)]
    public string CompositeName { get; set; } = string.Empty;

    public override string Name => CompositeName;

    public override void Run(IAlgorithmContext context)
    {
      foreach (IAlgorithm alg in Algorithms)
      {
        alg.Run(context);
      }
    }

    public override object Clone()
    {
      var rtn = new CompositeAlgorithm();

      rtn.Algorithms.AddRange(this.Algorithms.Select(alg => (IAlgorithm)alg.Clone()));
      rtn.CompositeName = this.CompositeName;

      return rtn;
    }

    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();

      sb.AppendFormat("Composite Algorithm \"{0}\"\n", CompositeName);
      foreach (var alg in Algorithms)
      {
        sb.AppendLine(alg.ToString());
      }

      return sb.ToString();
    }

    public override AlgorithmInfo ToInfo()
    {
      CompositeAlgorithmInfo info = new CompositeAlgorithmInfo()
      {
        Type = new SerializableType(this.GetType()),
        Parameters = this.TakesParameters ? this.Parameters : new AlgorithmParams(),
        CompositeName = this.CompositeName,
        Algorithms = new AlgorithmInfoList()
      };

      info.Algorithms.AddRange(this.Algorithms.Select(alg => alg.ToInfo()));

      return info;
    }
  }
}
