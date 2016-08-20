using System.IO;
using ServiceStack;

namespace Tile.ServiceModel
{
  [Route("/spectrumMetaTile/{zIndex}/{xIndex}/{yIndex}", "GET")]
  public class SpectrumMetaTile : IReturn<Stream>
  {
    public int ZIndex { get; set; }
    public int XIndex { get; set; }
    public int YIndex { get; set; }
  }
}