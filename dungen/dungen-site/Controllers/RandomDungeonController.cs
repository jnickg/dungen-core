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

namespace DunGen.Site.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  [Produces("application/json")]
  public class RandomDungeonController : ControllerBase
  {
    private readonly ILogger<RandomDungeonController> _logger;

    public RandomDungeonController(ILogger<RandomDungeonController> logger)
    {
      _logger = logger;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Get(int id, [FromQuery] int width = 25, [FromQuery] int height = 25)
    {
      Random r = new Random(id);
      
      var allAlgs = AlgorithmPluginEnumerator.GetAllLoadedAlgorithms();
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

      try
      {
        var dungeon = generator.Generate();
        var renderer = new DungeonTileRenderer();
        var image = renderer.Render(dungeon);

        var ms = new MemoryStream();
        image.Save(ms, ImageFormat.Png);
        ms.Seek(0, SeekOrigin.Begin);

        var jsonObj = new
        {
          alt = "A DunGenImage",
          algorithm = alg.GetType().Name,
          imageBytes = ms.GetBuffer()
        };

        return Ok(jsonObj);
      }
      catch (NotImplementedException)
      {
        return NotFound(String.Format("{0} is not implemented.", alg.GetType().Name));
      }
      catch (Exception e)
      {
        return NotFound(String.Format("Something went wrong:\n{0}", e.Message));
      }
    }
  }
}
