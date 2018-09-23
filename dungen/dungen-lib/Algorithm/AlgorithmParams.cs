using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Runtime.Serialization;

namespace DunGen.Algorithm
{
  [DataContract(Name = "algParams")]
  [KnownType(typeof(EditingAlgorithmParameter))]
  public class AlgorithmParams : ICloneable
  {
    [DataMember(IsRequired = true, Name = "list")]
    public IList<IEditingAlgorithmParameter> List { get; set; } = new List<IEditingAlgorithmParameter>();

    public object Clone()
    {
      AlgorithmParams clone = new AlgorithmParams();

      List<IEditingAlgorithmParameter> clonedParams = new List<IEditingAlgorithmParameter>();
      clonedParams.AddRange(this.List.Select(p => (IEditingAlgorithmParameter)p.Clone()));
      clone.List = clonedParams;

      return clone;
    }
  }

  public static partial class Extensions
  {
    /// <summary>
    /// Uses reflection to identify the properties of the specified
    /// algorithm, match them to this set of algParams by name, and
    /// apply their values
    /// </summary>
    public static void ApplyTo(this AlgorithmParams algParams, IAlgorithm algorithm)
    {
      foreach (IEditingAlgorithmParameter param in algParams.List)
      {
        foreach (PropertyInfo pi in algorithm.GetType().GetProperties())
        {
          List<AlgorithmParameterInfo> apis = pi.GetCustomAttributes<AlgorithmParameterInfo>().ToList();
          if (apis.Count == 0) continue;
          if (pi.Name == param.Name)
          {
            if (apis.ToList().Count > 1) throw new Exception("Too many AlgorithmParameterInfo objects for this property");
            AlgorithmParameterInfo paramInfo = apis.ToList().First();
            object parsedValue;
            if (!paramInfo.TryParseValue(param, out parsedValue)) throw new Exception("Invalid value to be set");
            pi.SetValue(algorithm, parsedValue);
          }
        }
      }
    }

    /// <summary>
    /// Uses reflection to identify the properties of the specified
    /// algorithm, match them to this set of algParams by name, and
    /// set the values of algParams based on the algorithm's current
    /// state.
    /// </summary>
    public static AlgorithmParams ApplyFrom(this AlgorithmParams algParams, IAlgorithm algorithm)
    {
      AlgorithmParams setParams = algParams;
      foreach (IEditingAlgorithmParameter param in setParams.List)
      {
        foreach (PropertyInfo pi in algorithm.GetType().GetProperties())
        {
          if (pi.Name == param.Name)
          {
            param.Value = pi.GetValue(algorithm);
            continue;
          }
        }
      }
      return setParams;
    }
  }
}
