using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Runtime.Serialization;

namespace DunGen.Algorithm
{
  public interface IEditableParameter : ICloneable
  {
    SerializableType ValueType { get; }
    string ParamName { get; }
    string Description { get; }
    object Value { get; set; }
    object Default { get; }
  }

  [DataContract(Name = "editableParam")]
  [KnownType("GetKnownTypes")]
  public class EditableParameterBase : IEditableParameter
  {
    private object _value = null;

    /// <summary>
    /// If non-null, the corresponding Property to which this Editable
    /// parameter applies.
    /// </summary>
    [DataMember(IsRequired = true, Name = "algProp", Order = 0)]
    public AlgorithmPropertyReference Property { get; set; } = null;

    /// <summary>
    /// The type of value contained by this Editable Parameter. This can be
    /// used as a key to determine how to edit the Value, and what type of
    /// AssociatedParam is stored in this object (if any).
    /// </summary>
    [DataMember(IsRequired = true, Name = "type", Order = 1)]
    public SerializableType ValueType { get; set; } = null;

    /// <summary>
    /// The name of this Editable Parameter. This corresponds to the name
    /// of the property in the associated IAlgorithm Type from which this
    /// Editable Parameter was created, and to which it can be applied.
    /// </summary>
    [DataMember(IsRequired = true, Name = "name", Order = 0)]
    public string ParamName { get; set; } = string.Empty;

    /// <summary>
    /// A human-readable description of this Parameter, and its purpose.
    /// </summary>
    [DataMember(IsRequired = true, Name = "desc", Order = 4)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The current value of this Parameter.
    /// </summary>
    [DataMember(IsRequired = true, Name = "val", Order = 3)]
    public object Value
    {
      get => _value;
      set
      {
        if (Property != null && !Property.Param.TryParseValue(value, out _value))
        {
          throw new ArgumentException(String.Format("Invalid value for parameter {0}: {1}", ParamName, value));
        }
        _value = value;
      }
    }

    /// <summary>
    /// The default value of this Parameter.
    /// </summary>
    [DataMember(IsRequired = true, Name = "dflt", Order = 2)]
    public object Default { get; set; } = null;

    /// <summary>
    /// <see cref="ICloneable.Clone"/>
    /// </summary>
    public object Clone()
    {
      return new EditableParameterBase()
      {
        ValueType = this.ValueType,
        ParamName = this.ParamName,
        Description = this.Description,
        Value = this.Value,
        Default = this.Default,
        Property = this.Property,
      };
    }

    public EditableParameterBase()
    { }

    public static IEnumerable<Type> GetKnownTypes()
    {
      return Parameter.GetKnownTypes();
    }

    public override string ToString()
    {
      return String.Format("Name: {0} Value: {1} ({2})", this.ParamName, this.Value, this.Description);
    }

    /// <summary>
    /// <see cref="object.Equals(object)"/>
    /// </summary>
    public override bool Equals(object obj)
    {
      EditableParameterBase other = obj as EditableParameterBase;
      if (null == other) return false;
      return this.ValueType == other.ValueType &&
             this.ParamName == other.ParamName &&
             // .Equals because Value is an object so == returns false
             this.Value.Equals(other.Value);
    }
  }

  [DataContract(Name = "algParams")]
  [KnownType("GetKnownTypes")]
  public class AlgorithmParams : ICloneable
  {
    [DataMember(IsRequired = true, Name = "list")]
    public IList<IEditableParameter> List { get; set; } = new List<IEditableParameter>();

    public IEditableParameter this[string Name]
    {
      get
      {
        return List.Where(editable => editable.ParamName == Name).FirstOrDefault();
      }
    }

    /// <summary>
    /// <see cref="ICloneable.Clone"/>
    /// </summary>
    public object Clone()
    {
      AlgorithmParams clone = new AlgorithmParams();

      List<IEditableParameter> clonedParams = new List<IEditableParameter>();
      clonedParams.AddRange(this.List.Select(p => (IEditableParameter)p.Clone()));
      clone.List = clonedParams;

      return clone;
    }

    /// <summary>
    /// <see cref="object.Equals(object)"/>
    /// </summary>
    public override bool Equals(object obj)
    {
      AlgorithmParams other = obj as AlgorithmParams;
      if (null == other) return false;
      if (this.List.Count != other.List.Count) return false;
      for (int i = 0; i < this.List.Count; ++i)
      {
        if (false == this.List[i].Equals(other.List[i])) return false;
      }
      return true;
    }

    /// <summary>
    /// Gets a list of all types that must be known by AlgorithmParams, for the purpose
    /// of serialization.
    /// </summary>
    public static IEnumerable<Type> GetKnownTypes()
    {
      List<Type> knownTypes = new List<Type>();

      foreach (var assy in AppDomain.CurrentDomain.GetAssemblies())
      {
        Type[] types = assy.GetTypes();
        types = types.Where(t => typeof(EditableParameterBase).IsAssignableFrom(t)).ToArray();
        knownTypes.AddRange(types);
      }

      return knownTypes;
    }
  }

  public static partial class Extensions
  {
    /// <summary>
    /// Gets the AlgorithmParameterInfo attributes applied to this property, removes instances
    /// of GroupAlgorithmParameterInfo, and orders them by the AlgorithmParameterInfo.Order
    /// value, before returning that list.
    /// </summary>
    public static List<Parameter> GetOrderedAlgParamInfos(this PropertyInfo prop)
    {
      return prop.GetCustomAttributes<Parameter>()
                 .OrderBy(api => api.Order)
                 .ToList();
    }

    /// <summary>
    /// Attempts to find the PropertyInfo in this IAlgorithm instance, that matches the specified 
    /// IEditableParameter.
    /// </summary>
    public static PropertyInfo GetMatchingPropertyFor(this IAlgorithm instance, IEditableParameter param)
    {
      return instance.GetMatchingPropertyFor(param.ParamName);
    }

    /// <summary>
    /// Attempts to find the PropertyInfo in this IAlgorithm instance, that matches the specified
    /// Parameter name. Returns null on failure.
    /// </summary>
    public static PropertyInfo GetMatchingPropertyFor(this IAlgorithm instance, string paramName)
    {
      return instance.GetType().GetMatchingPropertyFor(paramName);
    }

    /// <summary>
    /// Attempts to find the PropertyInfo in this Type , that matches the specified
    /// Parameter name. Returns null on failure.
    /// </summary>
    public static PropertyInfo GetMatchingPropertyFor(this Type type, string paramName)
    {
      IEnumerable<PropertyInfo> properties = type.GetProperties();
      var matchingProps = properties.Where(pi => pi.Name == paramName);
      if (matchingProps.Count() == 0) return null;
      if (matchingProps.Count() > 1)
      {
        throw new Exception(String.Format("Multiple Properties with name matching param: {0}", paramName));
      }
      return matchingProps.First();
    }

    /// <summary>
    /// Uses reflection to identify the properties of the specified
    /// algorithm, match them to this set of algParams by name, and
    /// apply their values
    /// </summary>
    public static void ApplyTo(this AlgorithmParams algParams, IAlgorithm algInstance)
    {
      foreach (IEditableParameter param in algParams.List)
      {
        PropertyInfo matchingProperty = algInstance.GetMatchingPropertyFor(param);

        if (matchingProperty == null)
        {
          continue;
        }

        var primaryParamInfo = matchingProperty.GetParameter();

        if (!primaryParamInfo.TryApplyValue(param, algInstance))
        {
          throw new Exception("Failed to apply IEditableParameter to matching instance property");
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
      foreach (IEditableParameter param in algParams.List)
      {
        PropertyInfo matchingProperty = algorithm.GetMatchingPropertyFor(param);

        if (matchingProperty == null)
        {
          continue;
        }

        var primaryParamInfo = matchingProperty.GetParameter();

        object currentVal = null;
        if (!primaryParamInfo.TryParseValue(algorithm, matchingProperty, out currentVal))
        {
          throw new Exception("Failed to apply matching instance property to IEditableParameter.");
        }
        param.Value = currentVal;
      }
      return algParams;
    }
  }
}
