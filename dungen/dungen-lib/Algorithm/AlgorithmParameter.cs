using DunGen.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace DunGen.Algorithm
{
  #region Types and Interfaces
  public enum ParameterCategory
  {
    UNKNOWN = 0,          // Unknown -- do not use
    Integer,              // A whole-number numeric value
    Decimal,              // A decimal-point numeric value
    Selection,            // Select from a pre-set list of options
    Boolean,              // On-off switch
    Algorithm,            // An algorithm with its own set of parameters
    // TODO: These guys!
    // ParameterGroup,       // A list of other parameters, grouped together
  }

  public interface IEditingAlgorithmParameter : ICloneable
  {
    ParameterCategory Category { get; }
    string Name { get; }
    string Description { get; }
    object Value { get; set; }
    object Default { get; }
  }
  #endregion

  #region EditingAlgorithmParameter Types
  [DataContract(Name = "param")]
  [KnownType("GetKnownTypes")]
  public class EditingAlgorithmParameter : IEditingAlgorithmParameter
  {
    private object _value;

    public AlgorithmParameterInfo ParamInfo
    {
      get;
      set;
    }

    public ParameterCategory Category
    {
      get;
      set;
    }

    [DataMember(IsRequired = true, Name = "name", Order = 0)]
    public string Name
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
        if (ParamInfo == null) _value = value; // Deserialization
        if (ParamInfo != null && !ParamInfo.TryParseValue(value, out _value))
        {
          throw new ArgumentException(String.Format("Invalid value for parameter {0}: {1}", Name, value));
        }
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
      return new EditingAlgorithmParameter()
      {
        Category = this.Category,
        Name = this.Name,
        Description = this.Description,
        Value = this.Value,
        Default = this.Default
      };
    }

    public EditingAlgorithmParameter()
    { }

    public EditingAlgorithmParameter(AlgorithmParameterInfo api)
    {
      this.ParamInfo = api;
      // TODO this should be able to pull info from API
    }

    public static IEnumerable<Type> GetKnownTypes()
    {
      return AlgorithmParameterInfo.GetKnownTypes();
    }
  }

  #endregion

  #region AlgorithmParameterInfo Types
  /// <summary>
  /// An attribute tag used to mark which properties of an IAlgorithmParameter instance
  /// are to be considered modifiable parameters of the algorithm.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public abstract class AlgorithmParameterInfo : System.Attribute
  {
    /// <summary>
    /// The category of Parameter described by this AlgorithmParameterInfo
    /// object.
    /// </summary>
    public ParameterCategory Category { get; private set; }

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

    public AlgorithmParameterInfo(ParameterCategory category)
    {
      if (category == ParameterCategory.UNKNOWN) throw new ArgumentException("Must supply known Parameter Category");
      this.Category = category;
    }

    public AlgorithmParameterInfo(ParameterCategory category, string description)
    {
      if (category == ParameterCategory.UNKNOWN) throw new ArgumentException("Must supply known Parameter Category");
      this.Category = category;
      this.Description = description;
    }

    /// <summary>
    /// Creates an Editable Parameter object from this Parameter's descriptor
    /// </summary>
    /// <param name="Name">The name of the property to show.</param>
    /// <returns>An object implementing IAlgorithmParameter, or NULL
    /// of none could be generated.</returns>
    public IEditingAlgorithmParameter ToEditableParam(string Name)
    {
      // TODO make it so it's system configurable whether to show unsupported params
      if (!Supported) return null;
      return ConvertToEditableParam(Name);
    }

    protected abstract IEditingAlgorithmParameter ConvertToEditableParam(string Name);

    public virtual bool TryParseValue(IEditingAlgorithmParameter param, out object parsedValue)
    {
      return TryParseValue(param.Value, out parsedValue);
    }

    public abstract bool TryParseValue(object value, out object parsedValue);

    public static IEnumerable<Type> GetKnownTypes()
    {
      List<Type> knownTypes = new List<Type>()
      {
        typeof(int),    // IntegerAlgorithmParamInfo
        typeof(double), // DecimalAlgorithmParamInfo
        typeof(bool)    // BooleanAlgorithmParamInfo
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
                                 null != p.PropertyType.GetCustomAttributes<AlgorithmParameterInfo>(true));
        knownTypes.AddRange(props.Select(p => p.PropertyType));
      }

      // Add all known algorithms so we can handle composite algorithms,
      // or algorithms taking other algs as parameters.
      knownTypes.AddRange(AlgorithmBase.GetKnownTypes());

      // Transform it to a set and back, to remove duplicates
      return knownTypes.Distinct().ToList();
    }
  }

  public class IntegerAlgorithmParamInfo : AlgorithmParameterInfo
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

    public IntegerAlgorithmParamInfo()
      : base(ParameterCategory.Integer)
    { }

    public IntegerAlgorithmParamInfo(string description, int defaultValue, int min, int max)
      : base(ParameterCategory.Integer, description)
    {
      this.Minimum = min;
      this.Maximum = max;
      this.Default = defaultValue;
    }

    protected override IEditingAlgorithmParameter ConvertToEditableParam(string Name)
    {
      return new EditingAlgorithmParameter(this)
      {
        Name = Name,
        Description = this.Description,
        Default = this.Default,
        Category = ParameterCategory.Integer,
        Value = this.Default
      };
    }

    public override bool TryParseValue(object value, out object parsedValue)
    {
      bool valueOk = false;
      int intValue = 0;
      // See if we can simply cast it
      if (value.GetType().IsPrimitive)
      {
        intValue = (int)value;
        valueOk = (intValue >= Minimum && intValue <= Maximum);
      }
      // Next try parsing it
      if (int.TryParse(value.ToString(), out intValue))
      {
        valueOk = (intValue >= Minimum && intValue <= Maximum);
      }

      parsedValue = intValue;
      return valueOk;
    }
  }

  public class DecimalAlgorithmParamInfo : AlgorithmParameterInfo
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
    /// The minimum number of precision points the algorithm agrres to honor
    /// when the algorithm runs. If "0", the value is unspecified.
    /// </summary>
    public int PrecisionPoints { get; set; } = 0;

    public DecimalAlgorithmParamInfo()
      :base(ParameterCategory.Decimal)
    { }

    public DecimalAlgorithmParamInfo(string description, double defaultValue, double min, double max, int precisionPts)
      : base(ParameterCategory.Decimal, description)
    {
      this.Minimum = min;
      this.Maximum = max;
      this.Default = defaultValue;
      this.PrecisionPoints = precisionPts;
    }

    protected override IEditingAlgorithmParameter ConvertToEditableParam(string Name)
    {
      return new EditingAlgorithmParameter(this)
      {
        Name = Name,
        Description = this.Description,
        Default = this.Default,
        Category = ParameterCategory.Decimal,
        Value = this.Default
      };
    }

    public override bool TryParseValue(object value, out object parsedValue)
    {
      bool valueOk = false;
      double dblValue;
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

      parsedValue = dblValue;
      return valueOk;
    }
  }

  public class SelectionAlgorithmParameterInfo : AlgorithmParameterInfo
  {
    private Type _selection = null;
    private object _default = null;

    /// <summary>
    /// The Enumerated type of this Selection Parameter. NULL if unspecified.
    /// </summary>
    public Type Selection
    {
      get => _selection;
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

        _selection = value;
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

        if (_selection != null && _selection != value.GetType())
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
        foreach (var v in Enum.GetValues(_selection.GetType()))
        {
          Choices.Add(v.ToString());
        }
        return choices;
      }
    }

    public SelectionAlgorithmParameterInfo()
      : base(ParameterCategory.Selection)
    { }

    public SelectionAlgorithmParameterInfo(string description, Type selection, object defaultValue)
      : base(ParameterCategory.Selection, description)
    {
      this.Selection = selection;
      this.Default = defaultValue;
    }

    protected override IEditingAlgorithmParameter ConvertToEditableParam(string Name)
    {
      return new EditingAlgorithmParameter(this)
      {
        Name = Name,
        Description = this.Description,
        Default = this.Default,
        Category = ParameterCategory.Selection,
        Value = this.Default
      };
    }

    public override bool TryParseValue(object value, out object parsedValue)
    {
      bool valueOk = false;
      object valueObj = null;

      if (value is string || value.GetType().IsEnum)
      {
        valueOk = Enum.TryParse(Selection, value.ToString(), out valueObj);
      }
      if (value.GetType().IsPrimitive)
      {
        valueOk = Enum.IsDefined(Selection, value);
        valueObj = Enum.ToObject(Selection, value);
      }

      parsedValue = valueObj;
      return valueOk;
    }
  }

  public class BooleanAlgorithmParameterInfo : AlgorithmParameterInfo
  {
    /// <summary>
    /// The default boolean value for this parameter.
    /// </summary>
    public bool Default { get; set; }

    public BooleanAlgorithmParameterInfo()
      : base(ParameterCategory.Boolean)
    { }

    public BooleanAlgorithmParameterInfo(string description, bool defaultValue)
      : base(ParameterCategory.Boolean, description)
    {
      this.Default = defaultValue;
    }

    protected override IEditingAlgorithmParameter ConvertToEditableParam(string Name)
    {
      return new EditingAlgorithmParameter(this)
      {
        Name = Name,
        Description = this.Description,
        Default = this.Default,
        Category = ParameterCategory.Boolean,
        Value = this.Default
      };
    }

    public override bool TryParseValue(object value, out object parsedValue)
    {
      bool valueOk = false;
      bool valBool = false;
      if (value.GetType().IsPrimitive)
      {
        valBool = (bool)value;
        valueOk = true;
      }
      if (value is string)
      {
        valueOk = Boolean.TryParse(value.ToString(), out valBool);
      }

      parsedValue = valBool;
      return valueOk;
    }
  }

  public class AlgorithmAlgorithmParameterInfo : AlgorithmParameterInfo
  {
    public Type AlgorithmBaseType { get; set; } = typeof(IAlgorithm);

    /// <summary>
    /// The type of algorithm to be used by default. This currently can't hold
    /// parameters, so users are limited to specifying a basic algorithm type
    /// whose default parameters will be used.
    /// </summary>
    public Type Default { get; set; }

    public AlgorithmAlgorithmParameterInfo()
      : base(ParameterCategory.Algorithm)
    { }

    public override bool TryParseValue(object value, out object parsedValue)
    {
      bool valueOk = false;
      parsedValue = null; // Will return will if unsuccessful
      Type typeOfIAlgorithm = typeof(IAlgorithm);
      Type typeOfValue = value.GetType();

      if (typeOfValue.IsPrimitive || typeOfValue.IsEnum)
      {
        throw new ArgumentException("Can't parse an Algorithm type from the given value");
      }
      if (value is string)
      {
        // Do we need to deserialize a string?
      }

      // If the value is an info for some reason, instantiate it
      if (typeOfValue == typeof(AlgorithmInfo))
      {
        AlgorithmInfo infoVal = value as AlgorithmInfo;
        if (null != infoVal)
        {
          parsedValue = infoVal.ToInstance();
          valueOk = true;
        }
      }

      // The new value passed is an actual Algorithm
      if (typeOfIAlgorithm.IsAssignableFrom(typeOfValue) &&
          AlgorithmBaseType.IsAssignableFrom(typeOfValue) &&
          !typeOfValue.IsAbstract)
      {
        parsedValue = value;
        valueOk = true;
      }

      return valueOk;
    }

    protected override IEditingAlgorithmParameter ConvertToEditableParam(string Name)
    {
      return new EditingAlgorithmParameter(this)
      {
        Name = Name,
        Description = this.Description,
        Default = this.Default,
        Category = ParameterCategory.Algorithm,
        Value = AlgorithmPluginEnumerator.GetAlgorithm(Default)
      };
    }
  }
  #endregion
}
