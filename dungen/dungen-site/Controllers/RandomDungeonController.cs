using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DunGen;
using DunGen.Generator;
using DunGen.Plugins;
using DunGen.Algorithm;
using DunGen.Rendering;
using System.Drawing;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net.Mime;
using System.Drawing.Imaging;
using DunGen.TerrainGen;
using ImageMagick;
using System.Text;

namespace DunGen.Site.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  [Produces("application/json")]
  public class RandomDungeonController : ControllerBase
  {
    private readonly ILogger<RandomDungeonController> _logger;

    private static ISet<IAlgorithm> _algorithms = new HashSet<IAlgorithm>
    {
      new BlobRecursiveDivision()
      {

      },
      new BlobRecursiveDivision()
      {
        RoomSize = 6
      },
      new BlobRecursiveDivision()
      {
        RoomSize = 4,
        MaxGapProportion = .1,
        GapCount = 3
      },
      new LinearRecursiveDivision()
      {

      },
      new RecursiveBacktracker()
      {
        WallStrategy = TerrainGenAlgorithmBase.WallFormation.Tiles
      },
      new RecursiveBacktracker()
      {
        WallStrategy = TerrainGenAlgorithmBase.WallFormation.Boundaries
      },
      new DiffusionLimitedAggregation()
      {
        DensityFactor = 0.5
      },
      new DiffusionLimitedAggregation()
      {
        DensityFactor = 0.1
      },
      new DiffusionLimitedAggregation()
      {
        DensityFactor = 0.25
      },
      new MazeWithRooms()
      {
        RoomBuilder = new MonteCarloRoomCarver()
        {
          RoomHeightMin = 2,
          RoomWidthMin = 2,
          RoomWidthMax = 6,
          RoomHeightMax = 6,
          AvoidOpen = false
        }
      },
      new MazeWithRooms()
      {
        MazeCarver = new RecursiveBacktracker()
        {
          WallStrategy = TerrainGenAlgorithmBase.WallFormation.Boundaries
        },
        RoomBuilder = new MonteCarloRoomCarver()
        {
          RoomHeightMin = 3,
          RoomWidthMin = 3,
          RoomWidthMax = 11,
          RoomHeightMax = 11,
          AvoidOpen = false,
          TargetRoomCount = 4
        }
      },
      new CompositeAlgorithm()
      {
        CompositeName = "CaveWithRooms",
        Algorithms = new AlgorithmList()
        {
          new WallUpper(),
          new DiffusionLimitedAggregation()
          {
            DensityFactor = 0.25
          },
          new MonteCarloRoomCarver()
          {
            RoomHeightMin = 3,
            RoomWidthMin = 3,
            RoomWidthMax = 7,
            RoomHeightMax = 7,
            AvoidOpen = false,
            WallStrategy = TerrainGenAlgorithmBase.WallFormation.Tiles,
            BorderPadding = 0,
            TargetRoomCount = 5
          }
        }
      }
    };

    public RandomDungeonController(ILogger<RandomDungeonController> logger)
    {
      _logger = logger;
    }

    private void AddImageToCollection(Image img, MagickImageCollection collection, int delay_hundreths)
    {
      using (var ms = new MemoryStream())
      {
        img.Save(ms, ImageFormat.Bmp);
        ms.Seek(0, SeekOrigin.Begin);
        var magick = new MagickImage(ms);
        magick.AnimationDelay = delay_hundreths;
        collection.Add(magick);
      }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Get(int id, [FromQuery] int width = 25, [FromQuery] int height = 25)
    {
      Random r = new Random(id);

      var allAlgs = _algorithms;
      var alg = allAlgs.ElementAt(r.Next(allAlgs.Count()));
      var runs = new List<AlgorithmRun>
      {
        new AlgorithmRun()
        {
          Alg = alg.Clone() as IAlgorithm,
          Context = new AlgorithmContextBase()
          {
            R = new AlgorithmRandom(id)
          }
        }
      };

      var renderer = new DungeonTileRenderer();
      renderer.ShadeGroups = false;
      var generator = new DungeonGenerator();
      var stepImages = new List<Image>();
      var collection = new MagickImageCollection();
      var counter = 0;

      Action<IAlgorithmContext> RenderAction = new Action<IAlgorithmContext>((context) =>
      {
        if (counter++ % 5 != 0) return;

        var d = context.D;
        var img = renderer.Render(d);

        AddImageToCollection(img, collection, 5);
      });

      generator.Options = new DungeonGenerator.DungeonGeneratorOptions()
      {
        DoReset = true,
        EgressConnections = null,
        Width = width,
        Height = height,
        AlgRuns = runs,
        Callbacks = {
          RenderAction
        }
      };

      try
      {
        var dungeon = generator.Generate();
        var lastImage = renderer.Render(dungeon);
        AddImageToCollection(lastImage, collection, 500);

        var gif_ms = new MemoryStream();
        collection.First().AnimationIterations = 0;
        var magickSettings = new QuantizeSettings();
        magickSettings.Colors = 256;
        collection.Quantize(magickSettings);
        collection.Optimize();
        collection.Write(gif_ms, MagickFormat.Gif);
        gif_ms.Seek(0, SeekOrigin.Begin);

        var ms = new MemoryStream();
        lastImage.Save(ms, ImageFormat.Png);
        ms.Seek(0, SeekOrigin.Begin);

        var jsonObj = new
        {
          alt = "A DunGenImage",
          algorithm = alg.Name,
          imageBytes = ms.GetBuffer(),
          gifBytes = gif_ms.GetBuffer()
        };

        return Ok(jsonObj);
      }
      catch (NotImplementedException)
      {
        return NotFound(String.Format("{0} is not implemented.", alg.Name));
      }
      catch (Exception e)
      {
        var message = new StringBuilder();
        message.AppendLine("Something went wrong. Exception details:");
        while (e != null)
        {
          message.AppendFormat("\"{0}\"\n", e.Message);
          message.AppendLine(e.StackTrace);
          message.AppendLine(e.InnerException != null ? "Inner Exception:" : "");
          e = e.InnerException;
        }
        return NotFound(message.ToString());
      }
    }
  }
}
