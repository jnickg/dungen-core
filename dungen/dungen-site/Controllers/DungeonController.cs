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

namespace DunGen.Site.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  [Produces("application/json")]
  public class DungeonController : ControllerBase
  {
    private readonly ILogger<DungeonController> _logger;

    public DungeonController(ILogger<DungeonController> logger)
    {
      _logger = logger;
    }

    private Dungeon GetRandomDungeon(int id, int width, int height)
    {
      Random r = new Random(id);

      var allAlgs = RandomDungeonController.RandomDungeonAlgorithms;
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

      var generator = new DungeonGenerator();
      generator.Options = new DungeonGenerator.DungeonGeneratorOptions()
      {
        DoReset = true,
        EgressConnections = null,
        Width = width,
        Height = height,
        AlgRuns = runs
      };

      return generator.Generate();
    }

    [HttpGet("random/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<Dungeon> GetRandom(int id, [FromQuery] int width = 25, [FromQuery] int height = 25)
    {
      try
      {
        return Ok(GetRandomDungeon(id, width, height));
      }
      catch (NotImplementedException e)
      {
        return NotFound(String.Format("Unimplemented algorithm:\n{0}", e.Message));
      }
      catch (Exception e)
      {
        return NotFound(String.Format("Something went wrong:\n{0}", e.Message));
      }
    }

    [HttpGet("random/{id}/image")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public ActionResult<Dungeon> GetRandomImage(int id, [FromQuery] int width = 25, [FromQuery] int height = 25)
    {
      try
      {
        var dungeon = GetRandomDungeon(id, width, height);
        var renderer = new DungeonTileRenderer();
        var image = renderer.Render(dungeon);

        var ms = new MemoryStream();
        image.Save(ms, ImageFormat.Png);
        ms.Seek(0, SeekOrigin.Begin);

        var jsonObj = new
        {
          alt = "A DunGenImage",
          algorithm = dungeon.Runs.First().Info.ToInstance().Name,
          imageBytes = ms.GetBuffer()
        };

        return Ok(jsonObj);

      }
      catch (NotImplementedException e)
      {
        return NotFound(String.Format("Unimplemented algorithm:\n{0}", e.Message));
      }
      catch (Exception e)
      {
        return NotFound(String.Format("Something went wrong:\n{0}", e.Message));
      }
    }
  }
}
