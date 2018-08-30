using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace DunGen
{
  public class AlgorithmPluginManager
  {
    #region Private Fields
    private string _directory = AppDomain.CurrentDomain.BaseDirectory;
    #endregion

    public List<IAlgorithm> Algorithms { get; private set; }
    public List<ITerrainGenAlgorithm> TerrainGenAlgorithms { get; private set; }

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

    public void Clear()
    {
      this.Algorithms = new List<IAlgorithm>();
      this.TerrainGenAlgorithms = new List<ITerrainGenAlgorithm>();
    }

    public void ReEnumerate()
    {
      this.Algorithms = new List<IAlgorithm>(
        LoadPluginsFromPath<IAlgorithm>(this.Directory));
      this.TerrainGenAlgorithms = new List<ITerrainGenAlgorithm>(
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
          T plugin = (T)Activator.CreateInstance(t);
          if (plugin != null)
          {
            results.Add(plugin);
          }
        }

      }

      return results;
    }
  }
}
