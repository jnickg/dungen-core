using System;
using System.Collections.Generic;
using System.Text;

namespace DunGen
{
  public class AlgorithmTypeNotFoundException : Exception
  {
    public string TypeName { get; set; }

    public AlgorithmTypeNotFoundException(string message, Exception innerException)
      : base(message, innerException)
    { }

    public AlgorithmTypeNotFoundException(string message)
      : base(message)
    { }

    public AlgorithmTypeNotFoundException()
      : base()
    { }
  }
}
