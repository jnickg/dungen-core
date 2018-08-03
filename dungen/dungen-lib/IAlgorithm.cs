using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DunGen
{
  /// <summary>
  /// An interface satisfied by all algorithms that run when generating
  /// a dungeon
  /// </summary>
  public interface IAlgorithm
  {
    /// <summary>
    /// The name of this algorithm.
    /// </summary>
    string Name { get; }
    /// <summary>
    /// Whether this algorithm uses parameters. If false, getting
    /// Parameters returns null, and setting it does nothing
    /// </summary>
    bool TakesParameters { get; }
    /// <summary>
    /// The current run parameters for this algorithm. If the value is
    /// set, the new parameters should be used in the next run of the
    /// algorithm.
    /// </summary>
    AlgorithmParams Parameters { get; set; }
    /// <summary>
    /// Gets a prototype instance of this algorithm's parameters.
    /// </summary>
    AlgorithmParams GetParamsPrototype();
  }

  /// <summary>
  /// Shared base implementation for all algorithms -- mostly
  /// the automatic reflective getting and setting of its 
  /// constituent parameters
  /// </summary>
  public abstract class AlgorithmBase : IAlgorithm
  {
    public virtual string Name
    {
      get => this.GetType().Name;
    }

    /// <summary>
    /// The magic sauce that allows for easy retrieval of parameter
    /// information, and setting of actual algorithm's properties
    /// via those parameter info objects.
    /// </summary>
    public AlgorithmParams Parameters
    {
      get
      {
        return GetParamsPrototype().ApplyFrom(this);
      }
      set
      {
        value.ApplyTo(this);
      }
    }

    public bool TakesParameters
    {
      get
      {
        bool containsParams = false;
        foreach (PropertyInfo propInfo in this.GetType().GetProperties())
        {
          List<AlgorithmParameterInfo> infos = new List<AlgorithmParameterInfo>(propInfo.GetCustomAttributes<AlgorithmParameterInfo>());
          if (infos.Count > 0)
          {
            containsParams = true;
            break;
          }
        }
        return containsParams;
      }
    }

    public AlgorithmBase()
    {
      this.GetParamsPrototype().ApplyTo(this);
    }

    public AlgorithmParams GetParamsPrototype()
    {
      AlgorithmParams prototype = new AlgorithmParams()
      {
        Parameters = new List<IAlgorithmParameter>()
      };

      foreach (PropertyInfo propInfo in this.GetType().GetProperties())
      {
        foreach (AlgorithmParameterInfo paramInfo in propInfo.GetCustomAttributes<AlgorithmParameterInfo>())
        {
          IAlgorithmParameter newParam = null;

          BooleanAlgorithmParameterInfo boolInfo = paramInfo as BooleanAlgorithmParameterInfo;
          if (null != boolInfo)
          {
            newParam = new BooleanAlgorithmParameter(
              propInfo.Name,
              paramInfo.Description,
              boolInfo.Default);
          }

          IntegerAlgorithmParamInfo numericInfo = paramInfo as IntegerAlgorithmParamInfo;
          if (null != numericInfo)
          {
            newParam = new IntegerAlgorithmParameter(
              propInfo.Name,
              paramInfo.Description,
              numericInfo.Minimum,
              numericInfo.Maximum,
              numericInfo.Default);
          }

          DecimalAlgorithmParamInfo decimalInfo = paramInfo as DecimalAlgorithmParamInfo;
          if (null != decimalInfo)
          {
            newParam = new DecimalAlgorithmParameter(
              propInfo.Name,
              paramInfo.Description,
              decimalInfo.Minimum,
              decimalInfo.Maximum,
              decimalInfo.Default,
              decimalInfo.PrecisionPoints);
          }

          SelectionAlgorithmParameterInfo selectionInfo = paramInfo as SelectionAlgorithmParameterInfo;
          if (null != selectionInfo)
          {
            newParam = new SelectionAlgorithmParameter(
              propInfo.Name,
              paramInfo.Description,
              selectionInfo.Selection,
              selectionInfo.Default);
          }

          if (null == newParam) throw new Exception("Unable to determine Algorithm Parameter Type. Do you need to apply an AlgorithmParameterInfo tag?");
          // ... and add it to the list of parameters!
          prototype.Parameters.Add(newParam);
        }
      }

      return prototype;
    }
  }
}
