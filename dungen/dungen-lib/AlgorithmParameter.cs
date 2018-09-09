using System;
using System.Collections.Generic;
using System.Text;

namespace DunGen
{
  #region Types and Interfaces
  public enum ParameterCategory
  {
    UNKNOWN = 0,          // Unknown -- do not use
    Integer,              // A whole-number numeric value
    Decimal,              // A decimal-point numeric value
    Selection,            // Select from a pre-set list of options
    Boolean               // On-off switch
    // TODO: These guys!
    // ParameterGroup,       // A list of other parameters, grouped together
    // TerrainGenAlgorithm   // An entire TerrainGenAlgorithm with its own set of parameters
  }

  public interface IAlgorithmParameter : ICloneable
  {
    ParameterCategory Category { get; }
    string Name { get; }
    string Description { get; }
    object Value { get; set; }
    object Default { get; }
  }
  #endregion

  #region AlgorithmParameter Types
  public abstract class AlgorithmParameterBase : IAlgorithmParameter
  {
    private ParameterCategory _category;
    private string _name;
    private string _description;

    public ParameterCategory Category { get => _category; protected set => _category = value; }
    public string Name { get => _name; protected set => _name = value; }
    public string Description { get => _description; protected set => _description = value; }

    public abstract object Value { get; set; }
    public abstract object Default { get; }

    public AlgorithmParameterBase(ParameterCategory category, string name, string description)
    {
      if (category == ParameterCategory.UNKNOWN) throw new ArgumentException("Must supply known Parameter Category");
      this._category = category;
      this._name = name;
      this._description = description;
    }

    public abstract void SetToDefault(); // Implemented in child class so non-null default can be used

    public abstract object Clone();
  }

  public class DecimalAlgorithmParameter : AlgorithmParameterBase
  {
    private double _value;
    private double _default;
    private double _min;
    private double _max;
    private int _precisionPoints;

    public override object Value
    {
      get => _value;
      set => _value = Double.Parse(value.ToString());
    }
    public override object Default { get => _default; }

    public DecimalAlgorithmParameter(string name, string description, double min, double max, double dflt, int precisionPts)
      : base(ParameterCategory.Decimal, name, description)
    {
      this._min = min;
      this._max = max;
      this._default = dflt;
      this._precisionPoints = precisionPts;

      SetToDefault();
    }

    public override object Clone()
    {
      return new DecimalAlgorithmParameter(this.Name, this.Description, this._min, this._max, this._default, this._precisionPoints);
    }

    public override void SetToDefault()
    {
      this._value = _default;
    }
  }

  public class IntegerAlgorithmParameter : AlgorithmParameterBase
  {
    private int _value;
    private int _default;
    private int _min;
    private int _max;

    public override object Value
    {
      get => _value;
      set => _value = Int32.Parse(value.ToString());
    }
    public override object Default { get => _default; }

    public IntegerAlgorithmParameter(string name, string description, int min, int max, int dflt)
      : base(ParameterCategory.Integer, name, description)
    {
      this._min = min;
      this._max = max;
      this._default = dflt;
      SetToDefault();
    }

    public override object Clone()
    {
      return new IntegerAlgorithmParameter(this.Name, this.Description, this._min, this._max, this._default);
    }

    public override void SetToDefault()
    {
      this._value = _default;
    }
  }

  public class SelectionAlgorithmParameter : AlgorithmParameterBase
  {
    private Type _selection;
    private object _default;
    private object _value;

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

    public override object Value
    {
      get => _value;
      set => _value = Enum.Parse(_selection, value.ToString());
    }

    public override object Default { get => _default; }

    public SelectionAlgorithmParameter(string name, string description, Type selection, object defaultValue)
      : base(ParameterCategory.Selection, name, description)
    {
      if (false == selection.IsEnum)
      {
        throw new ArgumentException("Provided \"selection\" must be an Enumeration");
      }

      if (defaultValue.GetType() != selection)
      {
        throw new ArgumentException(String.Format("\"defaultValue\" must be of type {0}", _selection));
      }

      this._selection = selection;
      this._default = defaultValue;
      SetToDefault();
    }

    public override object Clone()
    {
      return new SelectionAlgorithmParameter(this.Name, this.Description, this._selection, this._default);
    }

    public override void SetToDefault()
    {
      this._value = _default;
    }
  }

  public class BooleanAlgorithmParameter : AlgorithmParameterBase
  {
    private bool _value;
    private bool _default;

    public override object Value
    {
      get => _value;
      set => _value = Boolean.Parse(value.ToString());
    }

    public override object Default
    {
      get => _default;
    }

    public BooleanAlgorithmParameter(string name, string description, bool dflt)
      : base(ParameterCategory.Boolean, name, description)
    {
      this._default = dflt;
      SetToDefault();
    }

    public override object Clone()
    {
      return new BooleanAlgorithmParameter(this.Name, this.Description, this._default);
    }

    public override void SetToDefault()
    {
      this._value = _default;
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
    public IAlgorithmParameter ToEditableParam(string Name)
    {
      // TODO make it so it's system configurable whether to show unsupported params
      if (!Supported) return null;
      return ConvertToEditableParam(Name);
    }

    protected abstract IAlgorithmParameter ConvertToEditableParam(string Name);
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

    protected override IAlgorithmParameter ConvertToEditableParam(string Name)
    {
      return new IntegerAlgorithmParameter(
          Name,
          this.Description,
          this.Minimum,
          this.Maximum,
          this.Default);
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

    protected override IAlgorithmParameter ConvertToEditableParam(string Name)
    {
      return new DecimalAlgorithmParameter(
          Name,
          this.Description,
          this.Minimum,
          this.Maximum,
          this.Default,
          this.PrecisionPoints);
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
      this._selection = selection;
      this._default = defaultValue;
    }

    protected override IAlgorithmParameter ConvertToEditableParam(string Name)
    {
      return new SelectionAlgorithmParameter(
          Name,
          this.Description,
          this.Selection,
          this.Default);
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

    protected override IAlgorithmParameter ConvertToEditableParam(string Name)
    {
      return new BooleanAlgorithmParameter(
          Name,
          this.Description,
          this.Default);
    }
  }
  #endregion
}
