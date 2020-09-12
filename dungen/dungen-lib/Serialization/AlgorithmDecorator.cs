using DunGen.Algorithm;
using DunGen.TerrainGen;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.ComponentModel;

namespace DunGen.Serialization
{
  /// <summary>
  /// A class that initializes loaded algorithms for JSON Schema creation through
  /// Newtonsoft.JSON, by decorating them with the appropriate attributes dynamically.
  /// This allows users to define only the attributes included in DunGen, while still
  /// facilitating standard serialization.
  /// </summary>
  public static class AlgorithmDecorator
  {
    public static T JsonSchema<T>() where T : IAlgorithm
    {
      var type = typeof(T);
      var instance = (T)Activator.CreateInstance(type);
      var allParamProperties = type.GetProperties().Where(pi => pi.GetCustomAttribute<Parameter>() != null);
      foreach (var prop in allParamProperties)
      {
        var newAttr = new List<Attribute>();

        newAttr.Add(new RequiredAttribute());

        var paramAttr = prop.GetCustomAttribute<Parameter>();
        switch (paramAttr)
        {
          case IntegerParameter param:
            newAttr.Add(new RangeAttribute(param.Minimum, param.Maximum));
            break;
          case DecimalParameter param:
            newAttr.Add(new RangeAttribute(param.Minimum, param.Maximum));
            break;
          case SelectionParameter param:
            newAttr.Add(new EnumDataTypeAttribute(param.SelectionType));
            break;
          case BooleanParameter param:
            break;
          case AlgorithmParameter param:
            break;
          case null:
          default:
            throw new Exception();
        }

        TypeDescriptor.AddAttributes(instance, newAttr.ToArray());
      }
      return instance;
    }
  }
}
