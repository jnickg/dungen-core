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
      set
      {
        _value = (double)value;
      }
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

    public override object Value { get => _value; set => _value = (int)value; }
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
      set
      {
        if (value.GetType() != _selection) throw new ArgumentException(String.Format("Value must be of type {0}", _selection));
        _value = value;
      }
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
      set
      {
        if (false == (value is bool)) throw new ArgumentException("Value must be boolean");
        _value = (bool)value;
      }
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
  public class AlgorithmParameterInfo : System.Attribute
  {
    // TODO make this identical to IAlgorithmParameter in some way, so that
    // all an algorithm implementer has to do is add this attribute to their
    // AlgorithmParameter's parameter properties, so that the system can
    // hoover up all the attributes
    public ParameterCategory Category { get; private set; }
    public string Description { get; private set; }

    public AlgorithmParameterInfo(ParameterCategory category, string description)
    {
      if (category == ParameterCategory.UNKNOWN) throw new ArgumentException("Must supply known Parameter Category");
      this.Category = category;
      this.Description = description;
    }
  }

  public class IntegerAlgorithmParamInfo : AlgorithmParameterInfo
  {
    public int Minimum { get; private set; }
    public int Maximum { get; private set; }
    public int Default { get; private set; }


    public IntegerAlgorithmParamInfo(string description, int defaultValue, int min, int max)
      : base(ParameterCategory.Integer, description)
    {
      this.Minimum = min;
      this.Maximum = max;
      this.Default = defaultValue;
    }
  }

  public class DecimalAlgorithmParamInfo : AlgorithmParameterInfo
  {
    public double Minimum { get; private set; }
    public double Maximum { get; private set; }
    public double Default { get; private set; }
    public int PrecisionPoints { get; private set; }


    public DecimalAlgorithmParamInfo(string description, double defaultValue, double min, double max, int precisionPts)
      : base(ParameterCategory.Decimal, description)
    {
      this.Minimum = min;
      this.Maximum = max;
      this.Default = defaultValue;
      this.PrecisionPoints = precisionPts;
    }
  }

  public class SelectionAlgorithmParameterInfo : AlgorithmParameterInfo
  {
    private Type _selection;
    private object _default;

    public Type Selection
    {
      get => _selection;
    }

    public object Default
    {
      get => _default;
    }

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

    public SelectionAlgorithmParameterInfo(string description, Type selection, object defaultValue)
      : base(ParameterCategory.Selection, description)
    {
      if (false == selection.IsEnum)
      {
        throw new ArgumentException("Provided \"selection\" must be an Enumeration");
      }

      if (defaultValue.GetType() != selection)
      {
        throw new ArgumentException("\"defaultValue\" must be a value of the provided selection");
      }

      this._selection = selection;
      this._default = defaultValue;
    }
  }

  public class BooleanAlgorithmParameterInfo : AlgorithmParameterInfo
  {
    public bool Default { get; private set; }
    public BooleanAlgorithmParameterInfo(string description, bool defaultValue)
      : base(ParameterCategory.Boolean, description)
    {
      this.Default = defaultValue;
    }
  }
  #endregion
}
