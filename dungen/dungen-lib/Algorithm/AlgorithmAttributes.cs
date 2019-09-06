using DunGen.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace DunGen.Algorithm
{
  #region Algorithm Attribute Types
  /// <summary>
  /// An attribute tag used to identify information about an IAlgorithm
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public class AlgorithmUsage : System.Attribute
  {
    /// <summary>
    /// A description of this Algorithm.
    /// </summary>
    public string Description { get; set; } = string.Empty;
  }

  /// <summary>
  /// A serializable reference to an Algorithm type's property
  /// </summary>
  [DataContract(Name = "algPropRef")]
  public class AlgorithmPropertyReference
  {
    private SerializableType _algorithmType = null;
    private string _propertyName = string.Empty;  

    [DataMember(Name = "algType", IsRequired = true, Order = 0)]
    public SerializableType AlgorithmType
    {
      get => _algorithmType;
      set => _algorithmType = value;
    }

    [DataMember(Name = "propName", IsRequired = true, Order = 1)]
    public string PropertyName
    {
      get => _propertyName;
      set => _propertyName = value;
    }

    public PropertyInfo Info
    {
      get => AlgorithmType.ConvertToType(true).GetMatchingPropertyFor(PropertyName);
    }

    public bool IsParam
    {
      get => Info.GetParameter() != null;
    }

    public Parameter Param
    {
      get => Info.GetParameter();
    }
  }

  /// <summary>
  /// An attribute tag used to mark which properties of an IAlgorithmParameter instance
  /// are to be considered modifiable parameters of the algorithm.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public abstract class Parameter : System.Attribute
  {
    /// <summary>
    /// The type of Parameter connected to this Parameter
    /// </summary>
    public Type BaseType { get; protected set; } = null;

    /// <summary>
    /// The relative ordering of this Attribute, relative to others, when
    /// multiple AlgorithmParameterInfo attributes are applied to a single
    /// Algorithm Property. If multiple attributes are applied to a non-
    /// composite Algorithm Property (i.e. a basic type), the lowest-ordered
    /// valid AlgorithmParameterInfo wil lbe used. If used on a composite
    /// Algorithm Property, this will determine the order in which the
    /// values appear.
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// If not NULL or empty, a human-readable description of this Parameter.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether this Parameter is supported by the Algorithm implementation.
    /// Defaults to true, but may be set to false by Algorithms that don't
    /// support inherited Parameters (i.e. by overriding the Parameter and
    /// defining a new AlgorithmParameterInfo for the Parameter).
    /// 
    /// If false, this parameter will still appear in the Algorithm's
    /// "Parameters" property.
    /// </summary>
    public bool Supported { get; set; } = true;

    /// <summary>
    /// Whether to show this Parameter. Can be set to false for common
    /// Parameters not worth showing repeatedly, or for super secret
    /// hidden Easter-egg Parameters.
    /// </summary>
    public bool Show { get; set; } = true;

    /// <summary>
    /// Creates this Parameter with the specified Type. The type is used by clients
    /// to determine which interface to provide, when editing the parameter.
    /// </summary>
    /// <param name="paramType">The elementary type of this Parameter.</param>
    public Parameter(Type paramType)
    {
      this.BaseType = paramType;
    }

    /// <summary>
    /// Gets the appropriate default value for this Parameter. This can be a primitive
    /// value or an instance of an object, depending on the Parameter's type.
    /// </summary>
    public abstract object GetDefault();

    public virtual bool TryApplyValue(IEditableParameter source, IAlgorithm destination)
    {
      if (null == destination || null == source) throw new ArgumentNullException();

      PropertyInfo matchingProperty = destination.GetMatchingPropertyFor(source);
      object parsedValue;
      if (false == TryParseValue(source.Value, out parsedValue))
      {
        return false;
      }

      matchingProperty.SetValue(destination, parsedValue);
      return true;
    }

    public virtual bool TryParseValue<ParsedType>(IAlgorithm source, string propertyName, out ParsedType parsedValue)
    {
      PropertyInfo prop = source.GetMatchingPropertyFor(propertyName);
      return TryParseValue(source, prop, out parsedValue);
    }

    public bool TryParseValue<ParsedType>(IAlgorithm source, PropertyInfo prop, out ParsedType parsedValue)
    {
      object sourceVal = prop.GetValue(source);
      return TryParseValue(sourceVal, out parsedValue);
    }

    public bool TryParseValue<ParsedType>(IEditableParameter source, out ParsedType parsedValue)
    {
      return TryParseValue(source.Value, out parsedValue);
    }

    public virtual bool TryParseValue<ParsedType>(object value, out ParsedType parsedValue)
    {
      Type valueType = value.GetType();
      Type baseType = BaseType;
      Type typeToParse = typeof(ParsedType);

      parsedValue = default(ParsedType);

      // Check if we won't be able to produce the requested type.
      if (typeToParse.IsAssignableFrom(baseType))
      {
        parsedValue = (ParsedType)value;
        return true;
      }

      return false;
    }

    /// <summary>
    /// Gets a collection of all supported Parameter Types, which the client should
    /// be able to handle editing.
    /// </summary>
    public static IEnumerable<Type> GetParamTypes()
    {
      List<Type> paramTypes = new List<Type>()
      {
        typeof(int),
        typeof(double),
        typeof(bool),
        typeof(IAlgorithm),
        typeof(Enum)
      };

      return paramTypes;
    }

    /// <summary>
    /// Gets a collection of all known types that have Parameter tags associated
    /// with them, for the purposes of serialization.
    /// </summary>
    public static IEnumerable<Type> GetKnownTypes()
    {
      List<Type> knownTypes = new List<Type>()
      {
        typeof(int),                      // IntegerAlgorithmParamInfo
        typeof(double),                   // DecimalAlgorithmParamInfo
        typeof(bool),                     // BooleanAlgorithmParamInfo
        typeof(SerializableType),         // AlgorithmAlgorithmParamInfo
      };

      // Reflect through every Algorithm type loaded, to identify all
      // enumerations we need to know about for SelectionAlgorithmParameterInfo
      foreach (var assy in AppDomain.CurrentDomain.GetAssemblies())
      {
        // Linq is so handy but seriously it's garbage to debug those nested .Where().Select() calls
        Type[] types = assy.GetTypes();
        // Of all the loaded Algorithms...
        types = types.Where(t => typeof(IAlgorithm).IsAssignableFrom(t)).ToArray();
        IEnumerable<PropertyInfo> props = types.SelectMany(t => t.GetProperties().AsEnumerable());
        // ... We want Enum-based properties that are marked as an Algorithm Parameter
        props = props.Where(p => p.PropertyType.IsEnum &&
                                 null != p.PropertyType.GetCustomAttributes<Parameter>(true));
        knownTypes.AddRange(props.Select(p => p.PropertyType));
      }

      // Add all known algorithms so we can handle composite algorithms,
      // or algorithms taking other algs as parameters.
      knownTypes.AddRange(AlgorithmBase.GetKnownTypes());

      // Transform it to a set and back, to remove duplicates
      return knownTypes.Distinct().ToList();
    }
  }

  public class IntegerParameter : Parameter
  {
    /// <summary>
    /// The minimum value for this Parameter.
    /// </summary>
    public int Minimum { get; set; } = int.MinValue;

    /// <summary>
    /// The maxmimum value for this Parameter.
    /// </summary>
    public int Maximum { get; set; } = int.MaxValue;

    /// <summary>
    /// The default value for this Parameter.
    /// </summary>
    public int Default { get; set; } = 0;

    public IntegerParameter()
      : base(typeof(int))
    { }

    public override object GetDefault()
    {
      return Default;
    }

    public override bool TryParseValue<ParsedType>(object value, out ParsedType parsedValue)
    {
      Type valueType = value.GetType();
      int intValue = 0;

      bool valueOk = base.TryParseValue(value, out parsedValue);
      if (!valueOk) return false;

      // See if we can simply cast it to an int
      if (valueType.IsPrimitive && BaseType.IsAssignableFrom(valueType))
      {
        intValue = (int)value;
        valueOk = (intValue >= Minimum && intValue <= Maximum);
      }
      // Next try parsing it from string
      if (int.TryParse(value.ToString(), out intValue))
      {
        valueOk = (intValue >= Minimum && intValue <= Maximum);
      }

      parsedValue = (ParsedType)Convert.ChangeType(intValue, typeof(ParsedType));
      return valueOk;
    }
  }

  public class DecimalParameter : Parameter
  {
    /// <summary>
    /// The minimum value for this Parameter.
    /// </summary>
    public double Minimum { get; set; } = double.MinValue;

    /// <summary>
    /// The maxmimum value for this Parameter.
    /// </summary>
    public double Maximum { get; set; } = double.MaxValue;

    /// <summary>
    /// The default value for this Parameter.
    /// </summary>
    public double Default { get; set; } = 0.0;

    /// <summary>
    /// The minimum number of precision points the algorithm agrees to honor
    /// when the algorithm runs. If "0", the value is unspecified.
    /// </summary>
    public int Precision { get; set; } = 0;

    public DecimalParameter()
      : base(typeof(double))
    { }

    public override object GetDefault()
    {
      return Default;
    }

    public override bool TryParseValue<ParsedType>(object value, out ParsedType parsedValue)
    {
      double dblValue = 0.0;

      bool valueOk = base.TryParseValue(value, out parsedValue);
      if (!valueOk) return false;

      // See if we can simply cast it
      if (value.GetType().IsPrimitive)
      {
        dblValue = (double)value;
        valueOk = (dblValue >= Minimum && dblValue <= Maximum);
      }
      // Next try parsing it
      if (double.TryParse(value.ToString(), out dblValue))
      {
        valueOk = (dblValue >= Minimum && dblValue <= Maximum);
      }

      parsedValue = (ParsedType)Convert.ChangeType(dblValue, typeof(ParsedType));
      return valueOk;
    }
  }

  public class SelectionParameter : Parameter
  {
    private object _default = null;
    private Type _selectionType = null;

    /// <summary>
    /// The Enumerated type of this Selection Parameter. NULL if unspecified.
    /// </summary>
    public Type SelectionType
    {
      get => _selectionType;
      set
      {
        if (null != value && false == value.IsEnum)
        {
          throw new ArgumentException("Provided \"selection\" must be an Enumeration");
        }

        if (_default != null && _default.GetType() != value)
        {
          _default = null;
        }

        _selectionType = value;
      }
    }

    /// <summary>
    /// The default value for this Selection Parameter. NULL if unspecified.
    /// </summary>
    public object Default
    {
      get => _default;
      set
      {
        if (value != null && false == value.GetType().IsEnum)
        {
          throw new ArgumentException("Provided \"Default\" value must be an Enumeration");
        }

        if (SelectionType != null && SelectionType != value.GetType())
        {
          throw new ArgumentException("\"Default\" must be a value of the provided Selection type");
        }

        _default = value;
      }
    }

    /// <summary>
    /// A list of available choices from the Selection type. If there are
    /// exceptions or values this Parameter does not support, this property
    /// will reflect those exceptions.
    /// </summary>
    public List<string> Choices
    {
      get
      {
        List<string> choices = new List<string>();
        foreach (var v in Enum.GetValues(SelectionType))
        {
          choices.Add(v.ToString());
        }
        return choices;
      }
    }

    public SelectionParameter()
      : base(typeof(Enum))
    { }

    public override object GetDefault()
    {
      return Default;
    }

    public override bool TryParseValue<ParsedType>(object value, out ParsedType parsedValue)
    {
      object valueObj = null;

      bool valueOk = base.TryParseValue(value, out parsedValue);
      if (!valueOk) return false;

      if (value is string || value.GetType().IsEnum)
      {
        valueOk = Choices.Contains(value.ToString());
        if (!valueOk) return false;
        valueOk = Enum.TryParse(SelectionType, value.ToString(), out valueObj);
      }
      if (value.GetType().IsPrimitive)
      {
        valueOk = Enum.IsDefined(SelectionType, value);
        valueObj = Enum.ToObject(SelectionType, value);
      }

      parsedValue = (ParsedType)Convert.ChangeType(valueObj, typeof(ParsedType));

      return valueOk;
    }
  }

  public class BooleanParameter : Parameter
  {
    /// <summary>
    /// The default boolean value for this parameter.
    /// </summary>
    public bool Default { get; set; }

    public BooleanParameter()
      : base(typeof(bool))
    { }

    public override object GetDefault()
    {
      return Default;
    }

    public override bool TryParseValue<ParsedType>(object value, out ParsedType parsedValue)
    {
      bool valBool = false;

      bool valueOk = base.TryParseValue(value, out parsedValue);
      if (valueOk) return true;

      if (value.GetType().IsPrimitive)
      {
        valBool = (bool)value;
        valueOk = true;
      }
      if (value is string)
      {
        valueOk = Boolean.TryParse(value.ToString(), out valBool);
      }

      parsedValue = (ParsedType)Convert.ChangeType(valBool, typeof(ParsedType));

      return valueOk;
    }
  }

  public class AlgorithmParameter : Parameter
  {
    /// <summary>
    /// A more specific base type for the Algorithms allowed by this parameter. Any
    /// type inheriting from IAlgorithm is valid.
    /// </summary>
    public Type AlgorithmBaseType { get; set; } = typeof(IAlgorithm);

    /// <summary>
    /// The type of algorithm to be used by default. This currently can't hold
    /// parameters, so users are limited to specifying a basic algorithm type
    /// whose default parameters will be used.
    /// </summary>
    public Type DefaultType { get; set; }

    public AlgorithmParameter()
      : base(typeof(IAlgorithm))
    { }

    public override object GetDefault()
    {
      return AlgorithmPluginEnumerator.GetAlgorithm(DefaultType);
    }

    public override bool TryParseValue<ParsedType>(object value, out ParsedType parsedValue)
    {
      parsedValue = default(ParsedType); // Will return will if unsuccessful
      IAlgorithm algValue = null;
      Type typeOfValue = value.GetType();

      bool valueOk = base.TryParseValue(value, out parsedValue);
      if (!valueOk) return false;

      // If the value is an info for some reason, instantiate it
      if (typeOfValue == typeof(AlgorithmInfo))
      {
        AlgorithmInfo infoVal = value as AlgorithmInfo;
        if (null != infoVal)
        {
          algValue = infoVal.ToInstance();
          valueOk = true;
        }
      }

      // The new value passed is an actual Algorithm
      if (typeof(IAlgorithm).IsAssignableFrom(typeOfValue))
      {
        valueOk = BaseType.IsAssignableFrom(typeOfValue) &&
          AlgorithmBaseType.IsAssignableFrom(typeOfValue) &&
          !typeOfValue.IsAbstract;
        if (!valueOk) return false;
        algValue = value as IAlgorithm;
      }

      parsedValue = (ParsedType)algValue;

      return valueOk;
    }
  }
  #endregion

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
                    .Where(param => null != param)
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
      if (null == primaryParamInfo) return null;

      if (!primaryParamInfo.Supported) return null;


      if (null != instance && prop.DeclaringType != instance.GetType())
      {
        throw new ArgumentException("Specified instance must declare the specified property");
      }

      object valToAssign = primaryParamInfo.GetDefault();
      if (null != instance && null != prop)
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
        if (null == primaryParamInfo) continue;

        if (!primaryParamInfo.Show) continue;
        if (!primaryParamInfo.Supported) continue;

        IEditableParameter newParam = currentProperty.AsEditable();

        // TODO make it so it's system configurable whether to show unsupported params
        if (null == newParam && primaryParamInfo.Supported) throw new Exception("Unable to determine Algorithm Parameter Type. Do you need to apply an AlgorithmParameterInfo tag?");
        // ... and add it to the list of parameters!
        if (null != newParam) prototype.List.Add(newParam);
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
