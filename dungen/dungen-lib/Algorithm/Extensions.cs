using DunGen.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DunGen.Algorithm
{
  public static partial class Extensions
  {
    public static Parameter GetParameter(this PropertyInfo prop)
    {
      return prop.GetCustomAttributes<Parameter>()
           .OrderBy(api => api.Order)
           .FirstOrDefault();
    }
    public static AlgorithmUsage GetUsage(this IAlgorithm alg)
    {
      return alg.GetType().GetCustomAttributes<AlgorithmUsage>().FirstOrDefault();
    }

    public static bool TakesParameters(this Type algType)
    {
      return algType.GetProperties()
                    .Select(prop => prop.GetParameter())
                    .Where(param => param != null)
                    .Count() > 0;
    }

    /// <summary>
    /// Creates an IEditableParameter from this PropertyInfo, if it is possible to do so. Pass an
    /// instance of the Algorithm whose type contains the given property to set the IEditableParameter's
    /// current value to the corresponding Algorithm's Property's value .
    /// </summary>
    /// <returns>An IEditableParameter instance, or null if impossible to create one</returns>
    public static IEditableParameter AsEditable(this PropertyInfo prop, IAlgorithm instance = null)
    {
      var primaryParamInfo = prop.GetParameter();
      if (primaryParamInfo == null) return null;

      if (!primaryParamInfo.Supported) return null;

      if (instance != null && prop.DeclaringType != instance.GetType())
      {
        throw new ArgumentException("Specified instance must declare the specified property");
      }

      object valToAssign = primaryParamInfo.GetDefault();
      if (instance != null && prop != null)
      {
        valToAssign = prop.GetValue(instance);
      }

      var propRef = new AlgorithmPropertyReference()
      {
        AlgorithmType = prop.DeclaringType,
        PropertyName = prop.Name
      };

      return new EditableParameterBase()
      {
        ParamName = propRef.PropertyName,
        Description = primaryParamInfo.Description,
        Default = primaryParamInfo.GetDefault(),
        ValueType = primaryParamInfo.BaseType,
        Value = valToAssign,
        Property = propRef
      };
    }

    public static AlgorithmParams ParamsPrototype(this IAlgorithm instance)
    {
      AlgorithmParams prototype = new AlgorithmParams()
      {
        List = new List<IEditableParameter>()
      };

      foreach (PropertyInfo currentProperty in instance.GetType().GetProperties())
      {
        var primaryParamInfo = currentProperty.GetParameter();
        if (primaryParamInfo == null) continue;

        if (!primaryParamInfo.Show) continue;
        if (!primaryParamInfo.Supported) continue;

        IEditableParameter newParam = currentProperty.AsEditable();

        // TODO make it so it's system configurable whether to show unsupported params
        if (newParam == null && primaryParamInfo.Supported) throw new Exception("Unable to determine Algorithm Parameter Type. Do you need to apply an AlgorithmParameterInfo tag?");
        // ... and add it to the list of parameters!
        if (newParam != null) prototype.List.Add(newParam);
      }

      return prototype;
    }

    public static AlgorithmParams CurrentParameters(this IAlgorithm instance)
    {
      return instance.ParamsPrototype().ApplyFrom(instance);
    }

    public static IEditableParameter AsEditable(this IAlgorithm alg)
    {
      return new EditableParameterBase()
      {
        ParamName = alg.Name,
        Description = "TODO Add description attributes to algorithms",
        // TODO Should be same as AlgorithmParameter::GetDefault()
        Default = AlgorithmPluginEnumerator.GetAlgorithm(alg.GetType()),
        ValueType = typeof(IAlgorithm),
        Value = alg,
        Property = null
      };
    }
  }
}
