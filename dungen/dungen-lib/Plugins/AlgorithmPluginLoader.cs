using DunGen.Algorithm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DunGen.Plugins
{
  internal static class AlgorithmPluginLoader
  {
    public class AlgorithmPluginStats
    {
      public int CountIAlgorithm { get; set; }
      public int CountITerrainGenAlgorithm { get; set; }
      public FileInfo PluginFile { get; set; }

    }

    public static AlgorithmPluginStats GetStatsFor(string pluginPath)
    {
      FileInfo plugin = new FileInfo(pluginPath);
      int countIAlgorithm = 0,
          countITerrainGenAlgorithm = 0;

      if (false == plugin.Exists) goto rtn;

      AppDomain testLoadDomain = AppDomain.CreateDomain("pluginTestLoad");
      Assembly testAssembly = testLoadDomain.Load(plugin.FullName);

      foreach (Type t in testAssembly.GetExportedTypes())
      {
        if (!t.IsClass || t.IsNotPublic || t.IsAbstract)
        {
          continue;
        }

        if (typeof(IAlgorithm).IsAssignableFrom(t)) ++countIAlgorithm;
        if (typeof(TerrainGen.ITerrainGenAlgorithm).IsAssignableFrom(t)) ++countITerrainGenAlgorithm;
      }

      AppDomain.Unload(testLoadDomain);

      rtn:
      return new AlgorithmPluginStats()
      {
        CountIAlgorithm = countIAlgorithm,
        CountITerrainGenAlgorithm = countITerrainGenAlgorithm,
        PluginFile = plugin
      };
    }

    public static List<T> LoadPluginsFromPath<T>(string Path) where T : IAlgorithm
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

    public static List<T> LoadPluginFromAssembly<T>(string assyFile) where T : IAlgorithm
    {
      List<T> results = new List<T>();
      results.AddRange(
        LoadPluginFromAssembly(assyFile, typeof(T))
        .Select(alg => (T)alg));
      return results;
    }

    public static List<IAlgorithm> LoadPluginFromAssembly(string assyFile, Type pluginType)
    {
      if (false == typeof(IAlgorithm).IsAssignableFrom(pluginType))
      {
        throw new ArgumentException("pluginType must derive from IAlgorithm");
      }

      List<IAlgorithm> results = new List<IAlgorithm>();

#if DEBUG
      Debug.WriteLine("\n=====\nPlugin Type: {0}\nPlugin Name: {1}\n=====\n", pluginType.Module.FullyQualifiedName, pluginType.Name);
#endif

      if (!File.Exists(assyFile))
      {
        return results;
      }

      List<Assembly> alreadyLoaded = AppDomain.CurrentDomain.GetAssemblies().ToList();
      Assembly loadedAssy = Assembly.LoadFrom(assyFile);
      if (loadedAssy == null)
      {
        return results;
      }

      if (alreadyLoaded.Contains(loadedAssy))
      {
#if DEBUG
        Console.WriteLine("Didn't load {0}. Assembly \"{1}\" already loaded.", assyFile, loadedAssy.GetName().Name);
#endif
        return results;
      }

      Type[] exportedTypes = loadedAssy.GetExportedTypes();
      foreach (Type t in exportedTypes)
      {

        if (!t.IsClass || t.IsNotPublic || t.IsAbstract)
        {
          continue;
        }

        if (!pluginType.IsAssignableFrom(t)) continue;

#if DEBUG
        Debug.WriteLine("Loaded Assembly\n\tType: {0}\n\tName: {1}", t.Module.FullyQualifiedName, t.Name);
#endif

        IAlgorithm pluginPrototype = (IAlgorithm)Activator.CreateInstance(t);
        if (pluginPrototype != null)
        {
          results.Add(pluginPrototype);
        }
      }

      return results;
    }
  }
}
