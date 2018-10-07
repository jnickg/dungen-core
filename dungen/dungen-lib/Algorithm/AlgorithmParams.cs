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
    /// Gets the AlgorithmParameterInfo attributes applied to this property, removes instances
    /// of GroupAlgorithmParameterInfo, and orders them by the AlgorithmParameterInfo.Order
    /// value, before returning that list.
    /// </summary>
    public static List<AlgorithmParameterInfo> GetOrderedAlgParamInfos(this PropertyInfo prop)
    {
      return prop.GetCustomAttributes<AlgorithmParameterInfo>()
                 .Where(api => api.GetType() != typeof(GroupAlgorithmParameterInfo))
                 .OrderBy(api => api.Order)
                 .ToList();
    }

    public static GroupAlgorithmParameterInfo GetGroupAlgParamInfo(this PropertyInfo prop)
    {
      var groupApis = prop.GetCustomAttributes<GroupAlgorithmParameterInfo>().ToList();

      if (groupApis.Count == 0) return null; // Accepted use case
      if (groupApis.Count > 1)
      {
        throw new Exception(String.Format("Too many GroupAlgorithmParameterInfo attributes on property {0}", prop.Name));
      }
      return groupApis.First();
    }

    public static PropertyInfo GetMatchingPropertyFor(this IAlgorithm instance, IEditingAlgorithmParameter param)
    {
      IEnumerable<PropertyInfo> algProperties = instance.GetType().GetProperties();
      var matchingProps = algProperties.Where(pi => pi.Name == param.Name);
      if (matchingProps.Count() != 1) throw new Exception(String.Format("Multiple Properties with name mathing param: {0}", param.Name));
      return matchingProps.First();
    }

    /// <summary>
    /// Uses reflection to identify the properties of the specified
    /// algorithm, match them to this set of algParams by name, and
    /// apply their values
    /// </summary>
    public static void ApplyTo(this AlgorithmParams algParams, IAlgorithm algInstance)
    {
      foreach (IEditingAlgorithmParameter param in algParams.List)
      {
        PropertyInfo matchingProperty = algInstance.GetMatchingPropertyFor(param);

        var apis = matchingProperty.GetOrderedAlgParamInfos();
        if (apis.Count == 0) continue;

        AlgorithmParameterInfo primaryParamInfo = apis.First();

        // More than one API should mean this is a Parameter Group. We need to retrieve
        // that explicitly before attempting to parse the value out from `param`
        if (apis.Count > 1)
        {
          // Check first if this is a composite parameter. If so, let user know if they
          // screwed up in configuration it
          GroupAlgorithmParameterInfo groupApi = matchingProperty.GetGroupAlgParamInfo();
          primaryParamInfo = groupApi ?? throw new Exception(
            String.Format("Parameter {0} has {1} AlgorithmParameterInfo attributes," +
            "and so must have exactly one GroupAlgorithmParameterInfo. Found 0.",
            param.Name, apis.Count));
        }

        if (!primaryParamInfo.TryApplyValue(param, algInstance))
        {
          throw new Exception("Failed to apply IEditingAlgorithmParameter to matching instance property");
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
      foreach (IEditingAlgorithmParameter param in algParams.List)
      {
        PropertyInfo matchingProperty = algorithm.GetMatchingPropertyFor(param);

        // We don't need as much reflection protection here, because the
        // IAlgorithm instance's property should be explicitly typed
        object val = matchingProperty.GetValue(algorithm);
        param.Value = val;
      }
      return algParams;
    }
  }
}
