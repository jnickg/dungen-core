using DunGen.Algorithm;
using System;
using System.Collections.Generic;
using System.Text;

namespace DunGen.TerrainGen
{
  public abstract class ControlledStartAlgorithm : TerrainGenAlgorithmBase
  {
    //public enum StartConfigurationArg
    //{
    //  X = 0,
    //  Y = 1
    //};

    //[GroupAlgorithmParameterInfo(Description = "Configures the start of this algorithm relative to its mask")]
    //[DecimalAlgorithmParamInfo(
    //  Order = (int)StartConfigurationArg.X,
    //  GroupMemberName = "X%",
    //  Description = "0.0-1.0 left-to-right percentage of X start location",
    //  Minimum = 0.0,
    //  Maximum = 1.0,
    //  Default = 0.5,
    //  PrecisionPoints = 0)]
    //[DecimalAlgorithmParamInfo(
    //  Order = (int)StartConfigurationArg.Y,
    //  GroupMemberName = "Y%",
    //  Description = "0.0-1.0 left-to-right percentage of Y start location",
    //  Minimum = 0.0,
    //  Maximum = 1.0,
    //  Default = 0.5,
    //  PrecisionPoints = 0)]
    //public AlgorithmParameterGroup StartConfiguration { get; set; }
  }
}
