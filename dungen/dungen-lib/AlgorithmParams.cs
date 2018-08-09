using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace DunGen
{
  public class AlgorithmParams
  {
    public IList<IAlgorithmParameter> Parameters { get; set; }
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
      foreach (IAlgorithmParameter param in algParams.Parameters)
      {
        foreach (PropertyInfo pi in algorithm.GetType().GetProperties())
        {
          if (pi.Name == param.Name)
          {
            // TODO ck if requested value is VALID before applying
            pi.SetValue(algorithm, param.Value);
            continue;
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
      foreach (IAlgorithmParameter param in setParams.Parameters)
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
