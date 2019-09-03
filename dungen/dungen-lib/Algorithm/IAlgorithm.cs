using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace DunGen.Algorithm
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
    private IAlgorithm _alg;

    public IAlgorithm Alg
    {
      get => _alg;
      set
      {
        _alg = (IAlgorithm)value.Clone();
        if (_alg.TakesParameters)
        {
          _alg.Parameters = value.ParamsPrototype();
          _alg.Parameters = value.Parameters;
        }
      }
    }
    public IAlgorithmContext Context { get; set; }

    public void RunAlgorithm()
    {
      if (null != Alg)
      {
        Alg.Run(Context);
      }
    }

    public void PrepareFor(Dungeon d, AlgorithmRandom r = null)
    {
      if (null == d) throw new ArgumentNullException();
      if (null == r) r = AlgorithmRandom.RandomInstance();
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

      if (null == Context.R) Context.R = r;
    }
  }

  /// <summary>
  /// Shared base implementation for all algorithms -- mostly
  /// the automatic reflective getting and setting of its 
  /// constituent parameters
  /// </summary>
  [DataContract(Name = "algorithm")]
  [KnownType("GetKnownTypes")]
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
    [DataMember(Name = "params", Order = 1, IsRequired = false)]
    public AlgorithmParams Parameters
    {
      get
      {
        return this.CurrentParameters();
      }
      set
      {
        value.ApplyTo(this);
      }
    }

    public virtual bool TakesParameters
    {
      get
      {
        bool containsParams = false;
        foreach (PropertyInfo propInfo in this.GetType().GetProperties())
        {
          List<Parameter> infos = new List<Parameter>(propInfo.GetCustomAttributes<Parameter>());
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
      this.ParamsPrototype().ApplyTo(this);
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
    public virtual object Clone()
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

    public override string ToString()
    {
      StringBuilder sb = new StringBuilder();

      sb.AppendFormat("Algorithm \"{0}\"\n", Name);
      foreach (var param in Parameters.List)
      {
        sb.AppendLine(param.ToString());
      }

      return sb.ToString();
    }

    public override bool Equals(object obj)
    {
      IAlgorithm objAsAlg = obj as IAlgorithm;
      if (null == objAsAlg) return false;

      return this.ToInfo().Equals(objAsAlg.ToInfo());
    }

    public static IEnumerable<Type> GetKnownTypes()
    {
      // Reflect through every Algorithm type loaded, and just add them as potential candidates for
      List<Type> knownTypes = new List<Type>();

      foreach (var assy in AppDomain.CurrentDomain.GetAssemblies())
      {
        Type[] types = assy.GetTypes();

        Type[] iAlgTypes = types.Where(t => typeof(IAlgorithm).IsAssignableFrom(t) && !t.IsAbstract).ToArray();
        Type[] algInfoTypes = types.Where(t => typeof(AlgorithmInfo).IsAssignableFrom(t) && !t.IsAbstract).ToArray();

        knownTypes.AddRange(iAlgTypes);
        knownTypes.AddRange(algInfoTypes);
      }

      return knownTypes;
    }

    public virtual AlgorithmInfo ToInfo()
    {
      return new AlgorithmInfo()
      {
        Type = new SerializableType(this.GetType()),
        Parameters = this.TakesParameters ? this.Parameters : new AlgorithmParams()
      };
    }
  }
}
