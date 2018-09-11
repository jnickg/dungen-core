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
  public interface IAlgorithm : ICloneable
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
    /// <summary>
    /// Runs the Algorithm with the specified context
    /// </summary>
    void Run(IAlgorithmContext context);
  }

  /// <summary>
  /// A context in which an Algorithm can be run.
  /// </summary>
  public interface IAlgorithmContext
  {
    /// <summary>
    /// The dungeon on which the Algorithm should run
    /// </summary>
    Dungeon D { get; set; }
    /// <summary>
    /// The mask which the Algorithm should use when operating on
    /// the Context's dungeon
    /// </summary>
    bool[,] Mask { get; set; }
    /// <summary>
    /// If not NULL, A user-specified Random object to be used by the
    /// Algorithm.
    /// </summary>
    AlgorithmRandom R { get; set; }
  }

  /// <summary>
  /// A general context for an Algorithm, with zero frills.
  /// </summary>
  public class AlgorithmContextBase : IAlgorithmContext
  {
    /// <summary>
    /// The Dungeon on which to operate in this context.
    /// </summary>
    public Dungeon D { get; set; }
    /// <summary>
    /// The mask with which to operate on the dungeon.
    /// </summary>
    public bool[,] Mask { get; set; }
    /// <summary>
    /// If non-null the Random instance to use when generating.
    /// </summary>
    public AlgorithmRandom R { get; set; }
  }

  /// <summary>
  /// A pairing of an algorithm with its appropriate context. Also
  /// handles some basic logic of actually running the algorithm.
  /// </summary>
  public class AlgorithmRun
  {
    public IAlgorithm Alg { get; set; }
    public IAlgorithmContext Context { get; set; }

    public void RunAlgorithm()
    {
      if (null != Alg)
      {
        Alg.Run(Context);
      }
    }

    public void PrepareFor(Dungeon d)
    {
      if (null == d) throw new ArgumentNullException();
      if (null == Context) Context = new AlgorithmContextBase();

      Context.D = d;

      if (Context.Mask == null && Context.D != null)
      {
        Context.Mask = Context.D.Tiles.DefaultMask;
      }
      if (Context.Mask.GetLength(0) != Context.D.Tiles.Height ||
          Context.Mask.GetLength(1) != Context.D.Tiles.Width)
      {
        throw new Exception("Invalid mask for algorithm run; can't be " +
          "used with given Dungeon");
      }
    }
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
    /// Generates an AlgorithmParams object based on the Algorithm's type
    /// information, and populates it with the Algorithm object's current
    /// parameter values (i.e. from the object's associated Parameter
    /// properties).
    ///
    /// Should not be manipulated directly. Instead, retrieve and latch
    /// Parameter values using the pattern shown below:
    /// <code>
    ///   var editableParams = algObject.Parameters; // Retrieve
    ///   // Manipulate params object to change values
    ///   algObject.Parameters = editableParams; // Latch
    /// </code>
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
        List = new List<IEditingAlgorithmParameter>()
      };

      foreach (PropertyInfo propInfo in this.GetType().GetProperties())
      {
        foreach (AlgorithmParameterInfo paramInfo in propInfo.GetCustomAttributes<AlgorithmParameterInfo>())
        {
          if (!paramInfo.Show) continue;
          if (!paramInfo.Supported) continue;

          IEditingAlgorithmParameter newParam = paramInfo.ToEditableParam(propInfo.Name);
          // TODO make it so it's system configurable whether to show unsupported params
          if (null == newParam && paramInfo.Supported) throw new Exception("Unable to determine Algorithm Parameter Type. Do you need to apply an AlgorithmParameterInfo tag?");
          // ... and add it to the list of parameters!
          if (null != newParam) prototype.List.Add(newParam);
        }
      }

      return prototype;
    }

    /// <see cref="IAlgorithm.Run(IAlgorithmContext)"/>
    public abstract void Run(IAlgorithmContext context);

    /// <summary>
    /// Full clone of this Algorithm object, including current parameter
    /// values.
    /// </summary>
    /// <returns>
    /// An instance of IAlgorithm identical to the type of the object on
    /// which the call was made.
    /// </returns>
    public object Clone()
    {
      Type algT = this.GetType();
      if (algT.IsAbstract) return null;
      IAlgorithm algClone = (IAlgorithm)Activator.CreateInstance(algT);
      if (algClone != null && algClone.TakesParameters)
      {
        algClone.Parameters = this.Parameters;
      }
      return algClone;
    }
  }
}
