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
    Type ValueType { get; }
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

    public delegate bool ValueValidator(object v);

    public ValueValidator Validator
    {
      get;
      set;
    }

    public Type ValueType
    {
      get;
      set;
    }

    [DataMember(IsRequired = true, Name = "name", Order = 0)]
    public string ParamName
    {
      get;
      set;
    }

    public string Description
    {
      get;
      set;
    }

    [DataMember(IsRequired = true, Name = "val", Order = 2)]
    public object Value
    {
      get => _value;
      set
      {
        if (Validator != null && !Validator.Invoke(value))
        {
          throw new ArgumentException(String.Format("Invalid value for parameter {0}: {1}", ParamName, value));
        }
        _value = value;
      }
    }

    [DataMember(IsRequired = true, Name = "dflt", Order = 1)]
    public object Default
    {
      get;
      set;
    }

    public object Clone()
    {
      return new EditableParameterBase()
      {
        ValueType = this.ValueType,
        ParamName = this.ParamName,
        Description = this.Description,
        Value = this.Value,
        Default = this.Default
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

    public override bool Equals(object obj)
    {
      EditableParameterBase other = obj as EditableParameterBase;
      if (null == other) return false;
      return this.ValueType == other.ValueType &&
             this.ParamName == other.ParamName &&
             this.Value == other.Value;
    }
  }

  [DataContract(Name = "algParams")]
  [KnownType(typeof(EditableParameterBase))]
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


    public object Clone()
    {
      AlgorithmParams clone = new AlgorithmParams();

      List<IEditableParameter> clonedParams = new List<IEditableParameter>();
      clonedParams.AddRange(this.List.Select(p => (IEditableParameter)p.Clone()));
      clone.List = clonedParams;

      return clone;
    }

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
                 //.Where(api => api.GetType() != typeof(GroupAlgorithmParameterInfo))
                 .OrderBy(api => api.Order)
                 .ToList();
    }

    //public static GroupAlgorithmParameterInfo GetGroupAlgParamInfo(this PropertyInfo prop)
    //{
    //  var groupApis = prop.GetCustomAttributes<GroupAlgorithmParameterInfo>().ToList();

    //  if (groupApis.Count == 0) return null; // Accepted use case
    //  if (groupApis.Count > 1)
    //  {
    //    throw new Exception(String.Format("Too many GroupAlgorithmParameterInfo attributes on property {0}", prop.Name));
    //  }
    //  return groupApis.First();
    //}

    public static PropertyInfo GetMatchingPropertyFor(this IAlgorithm instance, IEditableParameter param)
    {
      IEnumerable<PropertyInfo> algProperties = instance.GetType().GetProperties();
      var matchingProps = algProperties.Where(pi => pi.Name == param.ParamName);
      if (matchingProps.Count() == 0) return null;
      if (matchingProps.Count() > 1)
      {
        throw new Exception(String.Format("Multiple Properties with name matching param: {0}", param.ParamName));
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
      foreach (IEditableParameter param in algParams.List)
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
