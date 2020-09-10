using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml.Serialization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;
using System.Linq;
using System.Text;
using DunGen;
using DunGen.Rendering;
using DunGen.TerrainGen;
using DunGen.Plugins;
using DunGen.Algorithm;
using DunGen.Serialization;
using DunGen.Generator;
using DunGen.Tiles;

namespace DunGen.CLI
{
  class Program
  {
    private static MenuItem Menu = new MenuItem("DunGen", true)
    {
      Description = ".NET Core CLI app to generate dungeons",
      StatusMessage = GetStatusMessage,
      Children = new List<MenuItem>()
      {
        new MenuItem("algorithm")
        {
          Description = "Manages algorithm plugins and pre-set palettes",
          Children = new List<MenuItem>()
          {
            new MenuItem("list")
            {
              Description = "List all algorithms currently available",
              Configure = AlgorithmsListCommand_Configure
            },
            new MenuItem("palette")
            {
              Description = "Manages the palette of pre-set algorithms",
              Children = new List<MenuItem>()
              {
                new MenuItem("add")
                {
                  Description = "Add a new algorithm to the palette",
                  Configure = AlgorithmsPaletteAddCommand_Configure
                },
                new MenuItem("list")
                {
                  Description = "List all algorithms in the palette",
                  Configure = AlgorithmsPaletteListCommand_Configure
                },
                new MenuItem("load")
                {
                  Description = "Load an algorithm palette from a file",
                  Configure = AlgorithmsPaletteLoadCommand_Configure
                },
                new MenuItem("remove")
                {
                  Description = "Remove an algorithm from the palette",
                  Configure = AlgorithmsPaletteRemoveCommand_Configure
                },
                new MenuItem("render")
                {
                  Description = "Render algorithm presets to a color palette, for painting",
                  Configure = AlgorithmsPaletteRenderCommand_Configure
                },
                new MenuItem("save")
                {
                  Description = "Save algorithm palette to a file",
                  Configure = AlgorithmsPaletteSaveCommand_Configure
                }
              }
            },
            new MenuItem("plugin")
            {
              Description = "Manages algorithm plugins",
              Children = new List<MenuItem>()
              {
                new MenuItem("install")
                {
                  Description = "Install a new algorithm plugin",
                  Configure = AlgorithmsPluginsInstallCommand_Configure
                },
                new MenuItem("list")
                {
                  Description = "List all installed algorithm plugins",
                  Configure = AlgorithmsPluginsListCommand_Configure
                },
                new MenuItem("uninstall")
                {
                  Description = "Uninstall an algorithm plugin",
                  Configure = AlgorithmsPluginsUninstallCommand_Configure
                },
              }
            },
          }
        },
        new MenuItem("dungeon")
        {
          Description = "Manages working dungeon, including save/load",
          StatusMessage = GetStatusMessage,
          Children = new List<MenuItem>()
          {
            new MenuItem("create")
            {
              Description = "Creates an empty dungeon",
              Configure = DungeonCreateCommand_Configure
            },
            new MenuItem("load")
            {
              Description = "Load a dungeon from a file",
              Configure = DungeonLoadCommand_Configure
            },
            new MenuItem("save")
            {
              Description = "Save the loaded dungeon to a file",
              Configure = DungeonSaveCommand_Configure
            },
            new MenuItem("render")
            {
              Description = "Render the working dungeon to a file",
              Configure = DungeonRenderCommand_Configure
            },
            new MenuItem("runsLoad")
            {
              Description = "Clears the current Generator run list and replaces it with this dungeon's run list",
              Configure = DungeonRunsLoadCommand_Configure
            },
          }
        },
        new MenuItem("generator")
        {
          Description = "Use DunGen algorithms on the working dungeon",
          StatusMessage = GetStatusMessage,
          Children = new List<MenuItem>()
          {
            new MenuItem("go")
            {
              Description = "Run the current algorithms on the working dungeon",
              Configure = GeneratorGoCommand_Configure
            },
            new MenuItem("runs")
            {
              Description = "Manage the algorithm runs",
              Children = new List<MenuItem>()
              {
                new MenuItem("add")
                {
                  Description = "Add a new algorithm run from the current palette",
                  Configure = GeneratorRunsAddCommand_Configure,
                },
                new MenuItem("clear")
                {
                  Description = "Clear all algorithm runs",
                  Configure = GeneratorRunsClearCommand_Configure
                },
                new MenuItem("list")
                {
                  Description = "List the algorithm runs",
                  Configure = GeneratorRunsListCommand_Configure
                },
                new MenuItem("read")
                {
                  Description = "Attempt to read algorithm runs from an image file",
                  Configure = GeneratorRunsReadCommand_Configure
                }
              }
            }
          }
        },
      }
    };

    private static readonly string default_plugin_dir = "Plugins";
    private static readonly DirectoryInfo _pluginDir = Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, default_plugin_dir));
    private static readonly int default_dungeon_height = 51;
    private static readonly int default_dungeon_width = 51;
    private static AlgorithmPalette _currentPalette;
    private static Dungeon loadedDungeon = null;
    private static string loadedDungeon_fileName = string.Empty;
    private static List<AlgorithmRun> _runs = new List<AlgorithmRun>();

    static void Main(string[] args)
    {
      Console.WriteLine("Loading DunGen CLI...");
      // Initialize Program Data
      DirectoryInfo pluginDir = Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, default_plugin_dir));

      Action<FileInfo> loadNotifier = (file) =>
      {
#if DEBUG
        Console.WriteLine("Loaded assembly: {0}", file.FullName);
#endif
      };
      AlgorithmPluginEnumerator.Enumerate(pluginDir.GetFiles().ToList(), loadNotifier);

      _currentPalette = AlgorithmPalette.DefaultPalette(
        AlgorithmPluginEnumerator.GetAllLoadedAlgorithms());

      _currentPalette.Add("Test1", new CompositeAlgorithm()
      {
        Algorithms = new AlgorithmList()
        {
          new RecursiveBacktracker()
          {
            WallStrategy = TerrainGenAlgorithmBase.WallFormation.Tiles,
          },
          new MonteCarloRoomCarver()
          {
            GroupRooms = true,
            AvoidOpen = false,
            RoomHeightMin = 3,
            RoomWidthMin = 3,
            RoomHeightMax = 10,
            RoomWidthMax = 10,
            TargetRoomCount = 15,
          }
        },
        CompositeName = "ComposoteTest1"
      }.ToPaletteItem());

      // Configure and run interactive program
      // Reference: https://github.com/anthonyreilly/ConsoleArgs/blob/master/Program.cs
      var rootApp = new CommandLineApplication();
      Program.Menu.SelfConfigure(rootApp);
      rootApp.VersionOption("-v|--version", () => {
        return string.Format("Version {0}",
          Assembly.GetEntryAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion);
      });
      rootApp.Out = Console.Out;
      try
      {
        rootApp.Execute(args);
      }
      catch (CommandParsingException ex)
      {
        Console.WriteLine(ex.Message);
      }
      catch (Exception ex)
      {
        Console.WriteLine("Unable to execute due to error:" + Environment.NewLine + "{0}", ex.Message);
#if DEBUG
        Console.WriteLine("Module: {0}", ex.TargetSite.Module.ToString());
        Console.WriteLine("At: {0}", ex.TargetSite.ToString());
        Console.WriteLine("Stack Trace:");
        Console.WriteLine(ex.StackTrace);
#endif
      }
    }

    private static string GetStatusMessage()
    {
      StringBuilder sb = new StringBuilder();

      if (loadedDungeon_fileName != null && loadedDungeon_fileName != string.Empty && loadedDungeon != null)
      {
        sb.AppendLine(String.Format("Dungeon loaded from: {0}", loadedDungeon_fileName));
        sb.AppendLine(String.Format("Dungeon is {0}x{1} Tiles", loadedDungeon.Tiles.Width, loadedDungeon.Tiles.Height));
      }

      return sb.ToString();
    }

    private static void DungeonLoadCommand_Configure(CommandLineApplication cmd)
    {
      var filenameArg = cmd.Argument("filename",
        "The filename of the dungeon to load");

      cmd.OnExecute(() =>
      {
        DungeonSerializer loader = new DungeonSerializer();
        try
        {
          loadedDungeon = loader.Load(filenameArg.Value);
          loadedDungeon_fileName = filenameArg.Value;
          Console.WriteLine("Loaded dungeon from file: {0}", loadedDungeon_fileName);
        }
        catch (FileNotFoundException)
        {
          Console.WriteLine("File not found: {0}", filenameArg.Value);
          return 1;
        }
        return 0;
      });
    }

    private static void DungeonSaveCommand_Configure(CommandLineApplication cmd)
    {
      var newNameOption = cmd.Option("-n|--name",
        "The filename to which to save the dungeon",
        CommandOptionType.SingleValue);

      cmd.OnExecute(() =>
      {
        if (loadedDungeon == null || loadedDungeon_fileName == string.Empty || loadedDungeon_fileName == null)
        {
          Console.WriteLine("ERROR: No dungeon loaded");
          return 1;
        }

        string saveName = loadedDungeon_fileName;
        if (newNameOption.HasValue())
        {
          saveName = newNameOption.Value();
        }
        DungeonSerializer saver = new DungeonSerializer();
        try
        {
          saver.Save(loadedDungeon, saveName, FileMode.Create);
          Console.WriteLine("Dungeon saved to file: {0}", saveName);
        }
        catch (Exception ex)
        {
          Console.WriteLine("Unable to save: {0}", ex.Message);
#if DEBUG
          Console.WriteLine("Module: {0}", ex.TargetSite.Module.ToString());
          Console.WriteLine("At: {0}", ex.TargetSite.ToString());
          Console.WriteLine("Stack Trace:");
          Console.WriteLine(ex.StackTrace);
#endif
          return 1;
        }
        
        return 0;
      });
    }

    private static void DungeonRenderCommand_Configure(CommandLineApplication cmd)
    {
      var saveOption = cmd.Option("-s|--save <path>",
        "Save the rendered image to a file",
        CommandOptionType.SingleValue);
      var overwriteOption = cmd.Option("-o|--overwrite",
        "If a save path was specified, overwrite if the file already exists",
        CommandOptionType.NoValue);
      var viewOption = cmd.Option("-v|--view",
        "Immediately preview the image in the default image viewer",
        CommandOptionType.NoValue);

      cmd.OnExecute(() =>
      {
        DungeonTileRenderer renderer = new DungeonTileRenderer();
        string tempFile = System.IO.Path.GetTempFileName();
        using (Image renderedDungeon = renderer.Render(loadedDungeon))
        {
          renderedDungeon.Save(tempFile, ImageFormat.Bmp);
          if (saveOption.HasValue())
          {
            string filePath = saveOption.Value();
            if (File.Exists(filePath) && !overwriteOption.HasValue())
            {
              Console.WriteLine("File exists: {0}", filePath);
              Console.WriteLine("Overwrite with -o|--overwrite");
              cmd.ShowHint();
              Console.WriteLine("Exiting...");
              return 1;
            }
            renderedDungeon.Save(filePath, ImageFormat.Bmp);
          }
        }
        if (viewOption.HasValue())
        {
          System.Diagnostics.Process.Start(tempFile);
        }
        return 0;
      });
    }

    private static void DungeonRunsLoadCommand_Configure(CommandLineApplication cmd)
    {
      cmd.OnExecute(() =>
      {
        if (loadedDungeon == null)
        {
          Console.WriteLine("Error: No dungeon loaded");
          return 1;
        }

        int clearedCount = _runs.Count;
        _runs.Clear();
        _runs.AddRange(loadedDungeon.Runs.ReconstructRuns());

        Console.WriteLine("Cleared {0} runs and added {1} runs from loaded dungeon", clearedCount, _runs.Count);
        return 0;
      });
    }

    private static void DungeonCreateCommand_Configure(CommandLineApplication cmd)
    {
      var filePathOption = cmd.Option("-s|--save <path>",
        "Specifies a relative file path/name where the new, empty dungeon should be saved.",
        CommandOptionType.SingleValue);
      var overwriteOption = cmd.Option("-o|--overwrite",
        "If a file path has been specified, use this option to overwrite an existing file.",
        CommandOptionType.NoValue);
      var autoLoadOption = cmd.Option("-w|--work",
        "Automatically switch the workingdungeon to the newly created dungeon (no lose-data warning",
        CommandOptionType.NoValue);

      var widthArg = cmd.Argument("width",
        "The width of the dungeon to generate.");
      var heightArg = cmd.Argument("height",
        "The width of the dungeon to generate.");

      cmd.OnExecute(() =>
      {
        int width = default_dungeon_width;
        int height = default_dungeon_height;
        if (null != widthArg.Value)
        {
          if (!int.TryParse(widthArg.Value, out width))
          {
            width = default_dungeon_width;
          }
        }
        if (heightArg.Value != null)
        {
          if (!int.TryParse(heightArg.Value, out height))
          {
            height = default_dungeon_height;
          }
        }

        // We now know the file path is correct, and it's OK to overwrite if
        // needed. So we can create the empty dungeon and save it.
        Dungeon emptyDungeon = new Dungeon()
        {
          Tiles = new DungeonTiles(width, height)
        };

        string filePath = string.Empty;
        if (filePathOption.HasValue())
        {
          DungeonSerializer serializer = new DungeonSerializer();
          filePath = filePathOption.Value();
          // Check if the ultimate file path will overwrite. Bail out if needed
          if (File.Exists(filePath) && !overwriteOption.HasValue())
          {
            Console.WriteLine("File exists: {0}", filePath);
            Console.WriteLine("Overwrite with -o|--overwrite");
            cmd.ShowHint();
            Console.WriteLine("Exiting...");
            return 1;
          }
          serializer.Save(emptyDungeon, filePath, FileMode.Create);
          Console.WriteLine("Saved new empty Dungeon to: {0}", filePath);
        }

        if (autoLoadOption.HasValue())
        {
          loadedDungeon = emptyDungeon;
          loadedDungeon_fileName = filePathOption.HasValue() ? filePath : "UNSAVED";
        }
        
        return 0;
      });
    }

    private static void DungeonViewCommand_Configure(CommandLineApplication cmd)
    {
      cmd.OnExecute(() =>
      {
        DungeonTileRenderer renderer = new DungeonTileRenderer();
        string tempFile = System.IO.Path.GetTempFileName();
        using (Image renderedDungeon = renderer.Render(loadedDungeon))
        {
          renderedDungeon.Save(tempFile, ImageFormat.Bmp);
        }
        System.Diagnostics.Process.Start(tempFile);
        return 0;
      });
    }

    private static void GeneratorGoCommand_Configure(CommandLineApplication cmd)
    {
      var resetOption = cmd.Option("-r|--reset",
        "Resets the working dungeon before running",
        CommandOptionType.NoValue);
      var clearOption = cmd.Option("-c|--clear",
        "Clears the list of Generator runs after executing",
        CommandOptionType.NoValue);

      cmd.OnExecute(() =>
      {
        if (loadedDungeon == null)
        {
          Console.WriteLine("Error: no dungeon loaded.");
          return 0;
        }
        DungeonGenerator generator = new DungeonGenerator();
        generator.WorkingDungeon = loadedDungeon;
        generator.Options = new DungeonGenerator.DungeonGeneratorOptions()
        {
          DoReset = resetOption.HasValue(),
          EgressConnections = null,
          AlgRuns = _runs,
          Width = loadedDungeon.Tiles.Width,
          Height = loadedDungeon.Tiles.Height,
        };
        Console.WriteLine("Running {0} algorithms on dungeon...", generator.Options.AlgRuns.Count);
        loadedDungeon = generator.Generate();
        loadedDungeon_fileName = "UNSAVED";

        if (clearOption.HasValue())
        {
          Console.WriteLine("Cleared all executed generator runs from Run List.");
          _runs.Clear();
        }

        return 0;
      });
    }

    private static void GeneratorRunsAddCommand_Configure(CommandLineApplication command)
    {
      List<IAlgorithm> terrainGenProtos = new List<IAlgorithm>();
      terrainGenProtos.AddRange(_currentPalette.Values.Select(s => s.CreateInstance()));

      int algCounter = 0;
      foreach (string palletteItemName in _currentPalette.Keys)
      {
        var algProto = _currentPalette[palletteItemName].CreateInstance();
        // Add a discrete command for each algorithm available
        command.Command(algCounter++.ToString(), (algCmd) =>
        {
          algCmd.Description = String.Format("{0,-40} - ({1})", palletteItemName, algProto.Name);
          StringBuilder extendedHelp = new StringBuilder();
          extendedHelp.AppendLine(String.Format("Parameters are: {0}", String.Join(", ", algProto.Parameters.List.Select((p) => p.ParamName))));
          foreach (var p in algProto.Parameters.List)
          {
            extendedHelp.AppendLine(String.Format("\t* {0} - '{1}'", p.ParamName, p.Description));
            // TODO explain valid values for this parameter
          }

          algCmd.ExtendedHelpText = extendedHelp.ToString();
          algCmd.HelpOption(MenuItem.HelpOptionString);

          // Users pass this repeatedly for every Algorithm Parameter they
          // want to specify as non-default.
          var paramOptions = algCmd.Option("-p|--param",
            "Attempts to set the parameter of the specified name to the given value, " +
            "before adding this algorithm to the run list.",
            CommandOptionType.MultipleValue);

          algCmd.OnExecute(() =>
          {
            IAlgorithm alg = algProto.Clone() as IAlgorithm;
            if (alg == null) throw new Exception("Can't clone Algorithm prototype for addition to run list.");

            // Apply non-default parameter values
            if (paramOptions.HasValue())
            {
              if (alg.TakesParameters == false) throw new ArgumentException("This algorithm doesn't take parameters");
              AlgorithmParams nonDefaultParams = alg.Parameters;
              foreach (var paramOptionInput in paramOptions.Values)
              {
                // Should be sent in like this: "-p SomeOption=value"
                string paramName = paramOptionInput.Split('=').First();
                var matches = nonDefaultParams.List.Where((p) => p.ParamName == paramName).Count();
                if (matches == 1)
                {
                  string paramVal = paramOptionInput.Split('=').Last(); // Second

                  nonDefaultParams.List
                           .Where((p) => p.ParamName == paramName)
                           .ToList()
                           .ForEach((p) => p.Value = paramVal);
                }
              }
              // TODO You should not have to explicitly get, edit, and set Params.
              // ...But it doesn't hurt much.
              alg.Parameters = nonDefaultParams;
            }
            _runs.Add(new AlgorithmRun()
            {
              Alg = alg,
              Context = null
            });
            return 0;
          });
        });
      }

      // Not implemented as a MenuItem because that implements its own execution for "*"
      command.Command("*", (cmd) =>
      {
        cmd.OnExecute(() =>
        {
          foreach (IAlgorithm alg in AlgorithmPluginEnumerator.GetAllLoadedAlgorithms())
          {
            _runs.Add(new AlgorithmRun()
            {
              Alg = alg,
              Context = null
            });
          }
          return 0;
        });
      });

     command.OnExecute(() =>
      {
        command.ShowHelp();
        return 0;
      });
    }

    private static void GeneratorRunsClearCommand_Configure(CommandLineApplication cmd)
    {
      cmd.OnExecute(() =>
      {
        _runs.Clear();
        return 0;
      });
    }

    private static void GeneratorRunsListCommand_Configure(CommandLineApplication cmd)
    {
      cmd.OnExecute(() =>
      {
        int counter = 0;
        foreach (var run in _runs)
        {
          Console.WriteLine("{0,2} - {1,-20}", counter++, run.Alg.Name);
          foreach (var p in run.Alg.Parameters.List)
          {
            Console.WriteLine(p);
          }
        }
        return 0;
      });
    }

    private static void GeneratorRunsReadCommand_Configure(CommandLineApplication cmd)
    {
      var fileArgument = cmd.Argument("Filename",
        "The path to the file containing a pre-drawn map layout");

      var clearOption = cmd.Option("-c|--clear",
        "Set this option to clear the algorithm runs first",
        CommandOptionType.NoValue);

      cmd.OnExecute(() =>
      {
        if (_currentPalette == null)
        {
          throw new Exception("Must have an algorithm palette loaded to read image");
        }

        if (_runs.Count != 0 && !clearOption.HasValue())
        {
          throw new Exception(String.Format("{0} Runs already set. Specify '-c' to clear runs first", _runs.Count));
        }

        Image maskSource = Bitmap.FromFile(Path.GetFullPath(fileArgument.Value));
        var maskDictionary = MaskInterpreter.ParseMasks(maskSource, _currentPalette);

        foreach (var maskPairing in maskDictionary)
        {
          if (!maskPairing.Value.ContainsTrue()) continue;
          _runs.Add(new AlgorithmRun()
          {
            Alg = maskPairing.Key.CreateInstance(),
            Context = new AlgorithmContextBase()
            {
              D = null,
              Mask = maskPairing.Value,
              R = null
            }
          });
        }

        return 0;
      });
    }

    private static void AlgorithmsListCommand_Configure(CommandLineApplication cmd)
    {
      cmd.OnExecute(() =>
      {
        List<IAlgorithm> algsToList = new List<IAlgorithm>();
        algsToList.AddRange(AlgorithmPluginEnumerator.GetAllLoadedAlgorithms());

        foreach(var alg in algsToList)
        {
          Console.WriteLine();
          Console.WriteLine();
          Console.WriteLine("Algorithm \"{0}\"", alg.Name);
          foreach (var param in alg.Parameters.List)
          {
            Console.WriteLine(param);
          }
        }
        return 0;
      });
    }

    private static void AlgorithmsPaletteSaveCommand_Configure(CommandLineApplication cmd)
    {
      var fileNameArg = cmd.Argument("filename",
        "The filename to which to save the palette");
      var overwriteOption = cmd.Option("-o|--overwrite",
        "Whether to overwrite the file of the given name, if it already exists.",
        CommandOptionType.NoValue);
      var exportPdnOption = cmd.Option("--pdn",
        "Specify this option to export the file to a Paint.NET color palette file",
        CommandOptionType.NoValue);

      cmd.OnExecute(() =>
      {
        if (_currentPalette == null)
        {
          Console.WriteLine("ERROR: No algorithm palette loaded");
          return 1;
        }

        string saveName = fileNameArg.Value;
        bool doOverwrite = overwriteOption.HasValue();
        AlgorithmPaletteSerializer saver = new AlgorithmPaletteSerializer();
        try
        {
          saver.Save(_currentPalette, saveName, doOverwrite ? FileMode.Create : FileMode.CreateNew);
          Console.WriteLine("Palette saved to file: {0}", saveName);
          if (exportPdnOption.HasValue())
          {
            string pdnFile = Path.GetFileNameWithoutExtension(saveName) + "_pdn.txt";
            saver.ExportForPdn(_currentPalette, pdnFile, doOverwrite ? FileMode.Create : FileMode.CreateNew);
            Console.WriteLine("Palette exported to PDN palette file: {0}", pdnFile);
          }
        }
        catch (IOException)
        {
          Console.WriteLine("Unable to save - {0} already exists.", saveName);
          return 1;
        }
        catch (Exception ex)
        {
          Console.WriteLine("Unable to save: {0}", ex.Message);
#if DEBUG
          Console.WriteLine("Module: {0}", ex.TargetSite.Module.ToString());
          Console.WriteLine("At: {0}", ex.TargetSite.ToString());
          Console.WriteLine("Stack Trace:");
          Console.WriteLine(ex.StackTrace);
#endif
          return 1;
        }

        return 0;
      });
    }

    private static void AlgorithmsPaletteLoadCommand_Configure(CommandLineApplication cmd)
    {
      var filenameArg = cmd.Argument("filename",
        "The filename of the palette to load");

      cmd.OnExecute(() =>
      {
        AlgorithmPaletteSerializer loader = new AlgorithmPaletteSerializer();
        try
        {
          _currentPalette = loader.Load(filenameArg.Value);
          Console.WriteLine("Loaded palette from file: {0}", filenameArg.Value);
        }
        catch (FileNotFoundException)
        {
          Console.WriteLine("File not found: {0}", filenameArg.Value);
          return 1;
        }
        catch (Exception ex)
        {
          Console.WriteLine("Unable to load: {0}", ex.Message);
#if DEBUG
          Console.WriteLine("Module: {0}", ex.TargetSite.Module.ToString());
          Console.WriteLine("At: {0}", ex.TargetSite.ToString());
          Console.WriteLine("Stack Trace:");
          Console.WriteLine(ex.StackTrace);
#endif
        }
        return 0;
      });
    }

    private static void AlgorithmsPaletteListCommand_Configure(CommandLineApplication cmd)
    {
      cmd.OnExecute(() =>
      {
        Console.WriteLine("{0,-12} - {1,-30} - {2}", "COLOR", "NAME", "TYPE");
        foreach (var n in _currentPalette.Keys)
        {
          string typeStr = String.Format("UNLOADED ({0})", _currentPalette[n].Info.Type.AssemblyQualifiedName);
          if (_currentPalette[n].Info.Type.IsTypeLoaded())
          {
            typeStr = _currentPalette[n].Info.Type.ConvertToType(true).Name;
          }
          Console.WriteLine("0x{0,-10:X} - {1,-30} - {2}", _currentPalette[n].PaletteColor.ToArgb(), n, typeStr);
        }
        return 0;
      });
    }

    private static void AlgorithmsPaletteRemoveCommand_Configure(CommandLineApplication command)
    {
      var rmvNameOption = command.Option("-n|--name",
        "The name of the palette item to remove",
        CommandOptionType.MultipleValue);

      var allOption = command.Option("-a|--all",
        "Removes all items from the palette",
        CommandOptionType.NoValue);

      command.OnExecute(() =>
      {
        if (allOption.HasValue())
        {
          int cleared = _currentPalette.Count;
          _currentPalette.Clear();
          Console.WriteLine("Cleared {0} entries from the palette", cleared);
          return 0;
        }

        if (rmvNameOption.HasValue())
        {
          foreach (var name in rmvNameOption.Values)
          {
            if (!_currentPalette.ContainsKey(name))
            {
              Console.WriteLine("Palette item of name {0} not found", name);
              continue;
            }
            _currentPalette.Remove(name);
            Console.WriteLine("Palette item {0} removed.", name);
          }
        }

        return 0;
      });
    }

    private static void AlgorithmsPaletteRenderCommand_Configure(CommandLineApplication command)
    {
      var fileNameArg = command.Argument("Filename",
        "The name of the file to which the palette will be rendered");

      var overwriteOption = command.Option("-o|--overwrite",
        "Specify this option to force overwriting of an existing file",
        CommandOptionType.NoValue);

      var labelOption = command.Option("-l|--label",
        "Specify this option to add labels for each palette item",
        CommandOptionType.NoValue);

      var voxelSizeOption = command.Option("-s|--size",
        "The size of each palette square, in pixels",
        CommandOptionType.SingleValue);

      command.OnExecute(() =>
      {
        if (_currentPalette == null)
        {
          Console.WriteLine("ERROR: No algorithm palette loaded");
          return 1;
        }

        string saveName = Path.GetFullPath(fileNameArg.Value);
        bool doOverwrite = overwriteOption.HasValue();
        bool doLabel = labelOption.HasValue();

        int voxSz = voxelSizeOption.HasValue() ? int.Parse(voxelSizeOption.Value()) : 10;
        int outputWidthFactor = doLabel ? 30 : 1;
        int outputHeightFactor = _currentPalette.Count;
        Bitmap output = new Bitmap(voxSz * outputWidthFactor, voxSz * outputHeightFactor);
        Graphics g = Graphics.FromImage(output);

        for (int i = 0; i < _currentPalette.Count; ++i)
        {
          var paletteEntry = _currentPalette.ElementAt(i);
          Rectangle paletteVoxel = new Rectangle(0, i * voxSz, voxSz, voxSz);
          Brush paletteItemColor = new SolidBrush(paletteEntry.Value.PaletteColor);
          g.FillRectangle(paletteItemColor, paletteVoxel);
          if (doLabel)
          {
            g.DrawString(
              paletteEntry.Key,
              new Font(FontFamily.GenericMonospace, (float)(voxSz * 0.8)),
              new SolidBrush(Color.White),
              (float) voxSz + 1,
              (float) i * voxSz - 1);
          }
        }

        if (File.Exists(saveName) && doOverwrite)
        {
          File.Delete(saveName);
        }

        output.Save(saveName);

        return 0;
      });
    }

    private static void AlgorithmsPaletteAddCommand_Configure(CommandLineApplication command)
    {
      int algCounter = 0;

      foreach (var algProto in AlgorithmPluginEnumerator.GetAllLoadedAlgorithms())
      {
        // Add a discrete command for each algorithm available
        command.Command(algCounter++.ToString(), (algCmd) =>
        {
          algCmd.Description = String.Format("{0,-40} - ({1})", algProto.Name, algProto.GetType().FullName);
          StringBuilder extendedHelp = new StringBuilder();
          extendedHelp.AppendLine(String.Format("Parameters are: {0}", String.Join(", ", algProto.Parameters.List.Select((p) => p.ParamName))));
          foreach (var p in algProto.Parameters.List)
          {
            extendedHelp.AppendLine(String.Format("\t* {0} - '{1}'", p.ParamName, p.Description));
            // TODO explain valid values for this parameter
          }

          algCmd.ExtendedHelpText = extendedHelp.ToString();
          algCmd.HelpOption(MenuItem.HelpOptionString);

          var paletteItemName = algCmd.Argument("Name",
            "Name of the new palette item");

          var overwriteOption = algCmd.Option("-o|--overwrite",
            "Specify this option if you would like to overwrite an existing palette item",
            CommandOptionType.NoValue);

          // Users pass this repeatedly for every Algorithm Parameter they
          // want to specify as non-default.
          var paramOptions = algCmd.Option("-p|--param",
            "Attempts to set the parameter of the specified name to the given value, " +
            "before adding this algorithm to the run list.",
            CommandOptionType.MultipleValue);

          algCmd.OnExecute(() =>
          {
            IAlgorithm alg = algProto.Clone() as IAlgorithm;
            if (alg == null) throw new Exception("Can't clone Algorithm prototype for addition to palette.");

            // Apply non-default parameter values
            if (paramOptions.HasValue())
            {
              if (alg.TakesParameters == false) throw new ArgumentException("This algorithm doesn't take parameters");
              AlgorithmParams nonDefaultParams = alg.Parameters;
              foreach (var paramOptionInput in paramOptions.Values)
              {
                // Should be sent in like this: "-p SomeOption=value"
                string paramName = paramOptionInput.Split('=').First();
                var matches = nonDefaultParams.List.Where((p) => p.ParamName == paramName).Count();
                if (matches == 1)
                {
                  string paramVal = paramOptionInput.Split('=').Last(); // Second

                  nonDefaultParams.List
                           .Where((p) => p.ParamName == paramName)
                           .ToList()
                           .ForEach((p) => p.Value = paramVal);
                }
              }
              // TODO You should not have to explicitly get, edit, and set Params.
              // ...But it doesn't hurt much.
              alg.Parameters = nonDefaultParams;
            }
            
            if (_currentPalette.ContainsKey(paletteItemName.Value) && !overwriteOption.HasValue())
            {
              throw new ArgumentException("Palette item of that name exists. Specify " +
                "overwrite option if you want to overwrite, or select a different name.");
            }
            _currentPalette[paletteItemName.Value] = alg.ToPaletteItem();
            Console.WriteLine("Palette item {0} updated to specified algorithm/parameters", paletteItemName.Value);
            return 0;
          });
        });
      }

      // Not implemented as a MenuItem because that implements its own execution for "*"
      command.Command("*", (cmd) =>
      {
        cmd.OnExecute(() =>
        {
          foreach (IAlgorithm alg in AlgorithmPluginEnumerator.GetAllLoadedAlgorithms())
          {
            _runs.Add(new AlgorithmRun()
            {
              Alg = alg,
              Context = null
            });
          }
          return 0;
        });
      });

      command.OnExecute(() =>
      {
        command.ShowHelp();
        return 0;
      });
    }

    private static void AlgorithmsPluginsListCommand_Configure(CommandLineApplication cmd)
    {
      cmd.OnExecute(() =>
      {
        foreach (var plugin in _pluginDir.GetFiles())
        {
          Console.WriteLine("Loaded assembly {0}", plugin.Name);
        }
        return 0;
      });
    }

    private static void AlgorithmsPluginsUninstallCommand_Configure(CommandLineApplication cmd)
    {
      cmd.OnExecute(() =>
      {
        throw new NotSupportedException("Can't yet uninstall plugins at runtime. " +
          "Close the program, manually remove from Plugins folder, and re-launch.");
        return 0;
      });
    }

    private static void AlgorithmsPluginsInstallCommand_Configure(CommandLineApplication cmd)
    {
      var locationArgument = cmd.Argument("Location",
        "Path to the DLL or Directory of DLLs that should be installed");

      var recurseOption = cmd.Option("-r|--recursive",
        "If the specified location is a directory, recurse into subdirectories.",
        CommandOptionType.NoValue);

      var temporaryOption = cmd.Option("-t|--temp",
        "Only install plugin for this runtime; do not add it as a permanent plugin",
        CommandOptionType.NoValue);

      cmd.OnExecute(() =>
      {
        List<FileInfo> pluginsToInstall = new List<FileInfo>();
        string pluginPath = Path.GetFullPath(locationArgument.Value);
        if (File.Exists(pluginPath))
        {
          pluginsToInstall.Add(new FileInfo(locationArgument.Value));
        }
        else if (Directory.Exists(pluginPath))
        {
          // Define a recursive action to enumerate subdirectories. Keep it
          // anonymous so as not to pollute the Program class
          Action<DirectoryInfo, bool> enumerate = null;
          enumerate = new Action<DirectoryInfo, bool>((dir, r) =>
          {
            pluginsToInstall.AddRange(dir.GetFiles());
            if (r)
            {
              foreach (var subdir in dir.GetDirectories())
              {
                enumerate?.Invoke(subdir, r);
              }
            }
          });

          DirectoryInfo pluginDir = new DirectoryInfo(locationArgument.Value);
          enumerate?.Invoke(pluginDir, recurseOption.HasValue());
        }

        Action<FileInfo> successfulInstall = (newPlugin) =>
        {
          Console.WriteLine("Loaded plugin {0}", newPlugin.FullName);
          if (temporaryOption.HasValue()) return;
          string fileName = Path.GetFileName(newPlugin.FullName);
          string destFile = Path.Combine(_pluginDir.FullName, fileName);
          Console.WriteLine("Copying to {0}...", destFile);
          newPlugin.CopyTo(destFile);
        };

        AlgorithmPluginEnumerator.Enumerate(pluginsToInstall, successfulInstall);

        return 0;
      });
    }
  }
}
