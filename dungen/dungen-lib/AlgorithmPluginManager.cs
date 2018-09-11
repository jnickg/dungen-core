using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DunGen
{
  public class AlgorithmPluginManager
  {
    #region Static Members
    public static bool IsAlgorithmLoaded(string typeName)
    {
      return (null != GetAlgorithmType(typeName));
    }

    public static IAlgorithm GetAlgorithm(string typeName)
    {
      IAlgorithm alg = null;

      Type algType = GetAlgorithmType(typeName);

      if (null == algType)
      {
        throw new AlgorithmTypeNotFoundException("Algorithm TypeName not loaded in current AppDomain")
        {
          TypeName = typeName
        };
      }

      alg = (IAlgorithm)Activator.CreateInstance(algType);

      if (null == alg)
      {
        throw new Exception(String.Format("Unable to instantiate IAlgorithm of type {0}", typeName));
      }

      return alg;
    }

    public static IEnumerable<IAlgorithm> GetAllLoadedAlgorithms()
    {
      return GetAllLoadedAlgorithmTypes().Select(t =>
      {
        return (IAlgorithm)Activator.CreateInstance(t);
      });
              
    }

    public static Type GetAlgorithmType(string typeName)
    {
      ISet<Type> algTypes = GetAllLoadedAlgorithmTypes();

      algTypes = algTypes.Where(t => t.FullName == typeName).ToHashSet();

      if (algTypes.Count != 1)
      {
        throw new Exception(String.Format("Too many Algorithm types of name {0}", typeName));
      }

      return algTypes.FirstOrDefault();
    }

    public static ISet<Type> GetAllLoadedAlgorithmTypes()
    {
      List<Type> loadedAlgs = new List<Type>();
      foreach (var assy in AppDomain.CurrentDomain.GetAssemblies())
      {
        loadedAlgs.AddRange(
          assy.GetTypes()
              .Where(t => typeof(IAlgorithm).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface));
      }
      return loadedAlgs.ToHashSet();
    }

    #endregion

    #region Instance Members
    #region Private Fields
    private string _directory = AppDomain.CurrentDomain.BaseDirectory;
    #endregion

    public List<IAlgorithm> AlgorithmProtos { get; private set; }
    public List<ITerrainGenAlgorithm> TerrainGenAlgorithmProtos { get; private set; }

    /// <summary>
    /// The current directory. Changing this clears the enumerated
    /// algorithms, but does not re-enumerate.
    /// </summary>
    public string Directory
    {
      get
      {
        return _directory;
      }
      set
      {
        _directory = value;
        this.Clear();
      }
    }

    public AlgorithmPluginManager(string directory = "")
    {
      if (null != directory && "" != directory)
      {
        this.Directory = directory;
      }
    }

    public void Clear()
    {
      this.AlgorithmProtos = new List<IAlgorithm>();
      this.TerrainGenAlgorithmProtos = new List<ITerrainGenAlgorithm>();
    }

    public void Enumerate()
    {
      this.AlgorithmProtos = new List<IAlgorithm>(
        LoadPluginsFromPath<IAlgorithm>(this.Directory));
      this.TerrainGenAlgorithmProtos = new List<ITerrainGenAlgorithm>(
        LoadPluginsFromPath<ITerrainGenAlgorithm>(this.Directory));
    }

    private static List<T> LoadPluginsFromPath<T>(string Path)
    {
      List<T> results = new List<T>();

      DirectoryInfo Directory = new DirectoryInfo(Path);
      if (Directory == null || !Directory.Exists)
      {
        return results; // Nothing to do here
      }

      FileInfo[] files = Directory.GetFiles("*.dll");
      if (files != null && files.Length > 0)
      {
        foreach (FileInfo fi in files)
        {
          List<T> step = LoadPluginFromAssembly<T>(fi.FullName);
          if (step != null && step.Count > 0)
          {
            results.AddRange(step);
          }
        }
      }

      return results;
    }

    private static List<T> LoadPluginFromAssembly<T>(string Filename)
    {
      List<T> results = new List<T>();

      Type pluginType = typeof(T);
#if DEBUG
      Debug.WriteLine("\n=====\nPlugin Type: {0}\nPlugin Name: {1}\n=====\n", pluginType.Module.FullyQualifiedName, pluginType.Name);
#endif

      Assembly assembly = Assembly.LoadFrom(Filename);
      if (assembly == null)
      {
        return results;
      }

      Type[] types = assembly.GetExportedTypes();
      foreach (Type t in types)
      {

        if (!t.IsClass || t.IsNotPublic)
        {
          continue;
        }

#if DEBUG
        Debug.WriteLine("Loaded Assembly\n\tType: {0}\n\tName: {1}", t.Module.FullyQualifiedName, t.Name);
#endif
        //  t.GetInterface(pluginType.Name) != null
        if (pluginType.IsAssignableFrom(t))
        {
          if (t.IsAbstract) continue;
          T plugin = (T)Activator.CreateInstance(t);
          if (plugin != null)
          {
            results.Add(plugin);
          }
        }

      }

      return results;
    }
    #endregion
  }
}
