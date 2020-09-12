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
  public class AlgorithmController : ControllerBase
  {
    private readonly ILogger<AlgorithmController> _logger;

    public AlgorithmController(ILogger<AlgorithmController> logger)
    {
      _logger = logger;
    }

    [HttpGet("{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<TerrainGenAlgorithmBase> GetBase(string name)
    {
      var algs = AlgorithmPluginEnumerator.GetAllLoadedAlgorithms();

      var thisAlg = algs.Select(alg => alg as TerrainGenAlgorithmBase).FirstOrDefault();
      var desiredAlg = algs
        .Where(alg => typeof(TerrainGenAlgorithmBase).IsAssignableFrom(alg.GetType()) && !alg.GetType().IsAbstract)
        .Where(alg => alg.Name.ToLower() == name.ToLower())
        .Select(alg => alg as TerrainGenAlgorithmBase)
        .FirstOrDefault();

      if (desiredAlg == null) return NotFound();
      
      return desiredAlg;
    }

    [HttpGet("{name}/info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<AlgorithmInfo> GetBaseInfo(string name)
    {
      var algs = AlgorithmPluginEnumerator.GetAllLoadedAlgorithms();

      var thisAlg = algs.Select(alg => alg as TerrainGenAlgorithmBase).FirstOrDefault();
      var desiredAlg = algs
        .Where(alg => typeof(TerrainGenAlgorithmBase).IsAssignableFrom(alg.GetType()) && !alg.GetType().IsAbstract)
        .Where(alg => alg.Name.ToLower() == name.ToLower())
        .Select(alg => alg as TerrainGenAlgorithmBase)
        .FirstOrDefault();

      if (desiredAlg == null) return NotFound();

      return desiredAlg.ToInfo();
    }

    [HttpPost("custom")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult PostNewCustom([FromBody]CompositeAlgorithm newAlgorithm)
    {
      return BadRequest();
    }

    [HttpGet("custom/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<CompositeAlgorithm> GetCustom(string name)
    {
      return NotFound();
    }

    [HttpPut("custom/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]

    public IActionResult PutCustom(string name, [FromBody]CompositeAlgorithm updatedAlgorithm)
    {
      return BadRequest();
    }

    [HttpDelete("custom/{name}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteCustom(string name)
    {
      return BadRequest();
    }
  }
}
