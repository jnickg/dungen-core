using DunGen.Algorithm;
using DunGen.TerrainGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace DunGen.Plugins
{
  public static class AlgorithmPluginEnumerator
  {
    #region Private Fields
    private static DirectoryInfo _mainDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
    private static HashSet<string> _loadedFiles = new HashSet<string>();
    #endregion

    public static bool IsAlgorithmLoaded(string typeName)
    {
      return (null != GetAlgorithmType(typeName));
    }

    public static IAlgorithm GetAlgorithm(Type algType)
    {
      IAlgorithm alg = null;

      if (null == algType)
      {
        throw new AlgorithmTypeNotFoundException("Algorithm TypeName not loaded in current AppDomain")
        {
          TypeName = algType.FullName
        };
      }

      alg = (IAlgorithm)Activator.CreateInstance(algType);

      if (null == alg)
      {
        throw new Exception(String.Format("Unable to instantiate IAlgorithm of type {0}", algType.FullName));
      }

      return alg;
    }

    public static IAlgorithm GetAlgorithm(string typeName)
    {

      Type algType = GetAlgorithmType(typeName);

      return GetAlgorithm(algType);
    }

    public static IEnumerable<IAlgorithm> GetAllLoadedAlgorithms()
    {
      return AllLoadedAlgorithmTypes().Select(t =>
      {
        return (IAlgorithm)Activator.CreateInstance(t);
      });
              
    }

    public static Type GetAlgorithmType(string typeName)
    {
      ISet<Type> algTypes = AllLoadedAlgorithmTypes();

      algTypes = algTypes.Where(t => t.FullName == typeName).ToHashSet();

      if (algTypes.Count == 0)
      {
        throw new AlgorithmTypeNotFoundException(String.Format("Did not find Algorithm type of name {0}", typeName));
      }

      if (algTypes.Count > 1)
      {
        throw new Exception(String.Format("Too many Algorithm types of name {0}", typeName));
      }

      return algTypes.FirstOrDefault();
    }

    public static ISet<Type> AllLoadedAlgorithmTypes()
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

    public static ISet<Type> AllAlgorithmInterfaces()
    {
      List<Type> loadedAlgs = new List<Type>();
      foreach (var assy in AppDomain.CurrentDomain.GetAssemblies())
      {
        loadedAlgs.AddRange(
          assy.GetTypes()
              .Where(t => typeof(IAlgorithm).IsAssignableFrom(t) && t.IsInterface));
      }
      return loadedAlgs.ToHashSet();
    }

    public static void Enumerate(List<FileInfo> addtiionalFiles, Action<FileInfo> successCallback = null)
    {
#if DEBUG
      Console.WriteLine("Enumerating assemblies...");
#endif
      if (null == addtiionalFiles || 0 == addtiionalFiles.Count) return;

      addtiionalFiles = addtiionalFiles.Where(fi => fi.Exists && fi.Extension == ".dll").ToList();
      foreach (FileInfo plugin in addtiionalFiles)
      {
        TryLoadFromFile(plugin, successCallback);
      }
    }

    public static bool TryLoadFromFile(FileInfo plugin, Action<FileInfo> successCallback = null)
    {
      if (_loadedFiles.Contains(plugin.FullName)) return true;

      //var stats = AlgorithmPluginLoader.GetStatsFor(plugin.FullName);
      //if (null == stats || 0 == stats.CountIAlgorithm) return false;

      List<IAlgorithm> loadedAlgs = AlgorithmPluginLoader.LoadPluginFromAssembly<IAlgorithm>(plugin.FullName);
      if (null == loadedAlgs || 0 == loadedAlgs.Count) return false;

      _loadedFiles.Add(plugin.FullName);
      successCallback?.Invoke(plugin);
      return true;
    }

    public static void LoadFromPath(DirectoryInfo pluginDir, Action<FileInfo> successCallback = null)
    {
      foreach (FileInfo plugin in pluginDir.GetFiles("*.dll"))
      {
        TryLoadFromFile(plugin, successCallback);
      }
    }
  }
}
