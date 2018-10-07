﻿using DunGen.Plugins;
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
    Group,                // A list of other parameters, grouped together
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

  [CollectionDataContract(Name = "paramGroup", ItemName = "param")]
  [KnownType(typeof(EditingAlgorithmParameter))]
  public class EditingAlgorithmParameterGroup : List<IEditingAlgorithmParameter>, IEditingAlgorithmParameter
  {
    public GroupAlgorithmParameterInfo ParamInfo
    {
      get;
      set;
    }
    public ParameterCategory Category => ParameterCategory.Group;

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

    public EditingAlgorithmParameterGroup Values
    {
      get => this;
      set
      {
        this.Clear();
        if (value != null)
        {
          this.AddRange(value);
        }
      }
    }

    public object Value
    {
      get
      {
        return this;
      }
      set
      {
        var editingVal = value as EditingAlgorithmParameterGroup;
        if (null != editingVal)
        {
          this.Values = editingVal;
          return;
        }
        // Copying from an IAlgorithm instance's current values
        var paramGroup = value as AlgorithmParameterGroup;
        if (null != paramGroup)
        {
          if (paramGroup.Count != this.Count)
          {
            // This is most likely to occur if the algorithm instance's param data is corrupted
            // at run-time by the algorithm itself (somehow).
            throw new ArgumentException(String.Format("Supplied AlgorithmParameterGroup should " +
              "have {0} items. (Actual: {1})", this.Count, paramGroup.Count));
          }
          for (int i = 0; i < this.Count; ++i)
          {
            this[i].Value = paramGroup[i];
          }
        }
      }
    }

    public AlgorithmParameterGroup Defaults
    {
      get;
      set;
    }

    public object Default
    {
      get => Defaults;
      set => Defaults = value as AlgorithmParameterGroup;
    }

    public object Clone()
    {
      return new EditingAlgorithmParameterGroup()
      {
        Name = this.Name,
        Description = this.Description,
        Value = this.Value,
        Default = this.Default
      };
    }

    public EditingAlgorithmParameterGroup()
    { }

    public EditingAlgorithmParameterGroup(GroupAlgorithmParameterInfo api)
    {
      this.ParamInfo = api;
    }

    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();

      sb.AppendLine(String.Format("Name: {0} ({1})\nValues:\n", this.Name, this.Description));
      foreach (var editable in this)
      {
        sb.AppendLine("\t" + editable.ToString());
      }

      return sb.ToString();
    }
  }

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

    public override string ToString()
    {
      return String.Format("Name: {0} Value: {1} ({2})", this.Name, this.Value, this.Description);
    }
  }

  #endregion

  #region AlgorithmParameterInfo Types
  /// <summary>
  /// An attribute tag used to mark which properties of an IAlgorithmParameter instance
  /// are to be considered modifiable parameters of the algorithm.
  /// </summary>
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
  public abstract class AlgorithmParameterInfo : System.Attribute
  {
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
    /// If this attribute describes a member of a ParameterGroup, this is
    /// the name that will be shown to the user. If this value is specified
    /// on an AlgorithmParameterInfo not describing a member of a Parameter
    /// Group, this value is ignored.
    /// </summary>
    public string GroupMemberName { get; set; } = string.Empty;
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
    /// <param name="PropertyName">The name of the property to show.</param>
    /// <returns>An object implementing IAlgorithmParameter, or NULL
    /// of none could be generated.</returns>
    public virtual IEditingAlgorithmParameter ToEditableParam(PropertyInfo property)
    {
      // TODO make it so it's system configurable whether to show unsupported params
      if (!Supported) return null;
      return ConvertToEditableParam(property.Name);
    }

    internal abstract IEditingAlgorithmParameter ConvertToEditableParam(string propertyName);

    public virtual bool TryApplyValue(IEditingAlgorithmParameter source, IAlgorithm destination)
    {
      PropertyInfo matchingProperty = destination.GetMatchingPropertyFor(source);
      object parsedValue;
      if (false == TryParseValue(source.Value, out parsedValue))
      {
        return false;
      }

      matchingProperty.SetValue(destination, parsedValue);
      return true;
    }

    public abstract bool TryParseValue(object value, out object parsedValue);

    public static IEnumerable<Type> GetKnownTypes()
    {
      List<Type> knownTypes = new List<Type>()
      {
        typeof(int),                      // IntegerAlgorithmParamInfo
        typeof(double),                   // DecimalAlgorithmParamInfo
        typeof(bool),                     // BooleanAlgorithmParamInfo
        typeof(AlgorithmType),            // AlgorithmAlgorithmParamInfo
        typeof(AlgorithmParameterGroup)   // GroupAlgorithmParameterInfo
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

    internal override IEditingAlgorithmParameter ConvertToEditableParam(string propertyName)
    {
      return new EditingAlgorithmParameter(this)
      {
        Name = propertyName,
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

    internal override IEditingAlgorithmParameter ConvertToEditableParam(string propertyName)
    {
      return new EditingAlgorithmParameter(this)
      {
        Name = propertyName,
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

    internal override IEditingAlgorithmParameter ConvertToEditableParam(string propertyName)
    {
      return new EditingAlgorithmParameter(this)
      {
        Name = propertyName,
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

    internal override IEditingAlgorithmParameter ConvertToEditableParam(string propertyName)
    {
      return new EditingAlgorithmParameter(this)
      {
        Name = propertyName,
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

    internal override IEditingAlgorithmParameter ConvertToEditableParam(string propertyName)
    {
      return new EditingAlgorithmParameter(this)
      {
        Name = propertyName,
        Description = this.Description,
        Default = new AlgorithmType(this.Default),
        Category = ParameterCategory.Algorithm,
        Value = AlgorithmPluginEnumerator.GetAlgorithm(Default)
      };
    }
  }

  /// <summary>
  /// Shim type to be used when creating groups of Algorithm Parameters
  /// </summary>
  /// <typeparam name="T">Any Type for which there exists an AlgorithmParameterInfo</typeparam>
  [CollectionDataContract(Name = "paramGroup", ItemName = "param")]
  [KnownType("GetKnownTypes")]
  public class AlgorithmParameterGroup : List<object>
  {
    public AlgorithmParameterGroup() { }

    public AlgorithmParameterGroup(IEnumerable<object> members)
      : base(members)
    { }

    public void ParamAt<ElemType>(int index, out ElemType element)
    {
      element = ParamAt<ElemType>(index);
    }

    public ElemType ParamAt<ElemType> (int index)
    {
      return (ElemType)this[index];
    }

    public void ParamAt<ElemType, EnumType>(EnumType enumIndex, out ElemType element)
    {
      element = ParamAt<ElemType, EnumType>(enumIndex);
    }

    public ElemType ParamAt<ElemType, EnumType>(EnumType enumIndex)
    {
      if (false == typeof(int).IsAssignableFrom(Enum.GetUnderlyingType(typeof(EnumType))))
      {
        throw new ArgumentException("Enum used to retrieve element must be assignable to int");
      }
      int index = (int)Convert.ChangeType(enumIndex, Enum.GetUnderlyingType(typeof(EnumType)));
      return ParamAt<ElemType>(index);
    }

    public static IEnumerable<Type> GetKnownTypes()
    {
      return AlgorithmParameterInfo.GetKnownTypes();
    }
  }

  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
  public class GroupAlgorithmParameterInfo : AlgorithmParameterInfo
  {
    private bool ValidateGroupType(Type value)
    {
      return GetKnownTypes().Contains(value);
    }

    private static ArgumentException GenerateExceptionFor(AlgorithmParameterGroup group, int index)
    {
      // Object item = group[index];
      // TODO
      return new ArgumentException("TBD");
    }

    private static bool GroupTypesValid(AlgorithmParameterGroup group, bool throwOnFailure = false)
    {
      IEnumerable<Type> knownTypes = GetKnownTypes();

      foreach (var param in group)
      {
        if (false == knownTypes.Contains(param.GetType()))
        {
          if (throwOnFailure) throw GenerateExceptionFor(group, group.IndexOf(param));
          return false;
        }
      }
      return true;
    }

    public GroupAlgorithmParameterInfo()
      : base(ParameterCategory.Group)
    { }

    public override bool TryApplyValue(IEditingAlgorithmParameter source, IAlgorithm destination)
    {
      PropertyInfo matchingProperty = destination.GetMatchingPropertyFor(source);
      var sourceParamGroup = source.Value as EditingAlgorithmParameterGroup;
      if (null == sourceParamGroup) return false;

      var paramGroupInfos = matchingProperty.GetOrderedAlgParamInfos();
      if (null == paramGroupInfos || sourceParamGroup.Count != paramGroupInfos.Count)
      {
        return false;
      }

      AlgorithmParameterGroup newValues = new AlgorithmParameterGroup();
      for (int i = 0; i < sourceParamGroup.Count; ++i)
      {
        object parsedVal;
        if (false == paramGroupInfos[i].TryParseValue(sourceParamGroup[i].Value, out parsedVal))
        {
          return false;
        }
        newValues.Add(parsedVal);
      }

      matchingProperty.SetValue(destination, newValues);
      return true;
    }

    public override bool TryParseValue(object value, out object parsedValue)
    {
      throw new NotSupportedException("Can't parse a value from a group of values");
    }

    public override IEditingAlgorithmParameter ToEditableParam(PropertyInfo property)
    {
      if (!Supported) return null;

      var apis = property.GetOrderedAlgParamInfos();
      List<IEditingAlgorithmParameter> editables = apis.Select(api => api.ConvertToEditableParam(api.GroupMemberName)).ToList();
      var editableGroup = new EditingAlgorithmParameterGroup(this)
      {
        Name = property.Name,
        Description = this.Description,
      };

      editableGroup.AddRange(editables);

      return editableGroup;
    }

    internal override IEditingAlgorithmParameter ConvertToEditableParam(string property)
    {
      throw new NotSupportedException("Can't create EditableParam Group without PropertyInfo");
    }
  }
  #endregion
}
