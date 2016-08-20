using System.IO;
using ServiceStack;

namespace Tile.ServiceModel
{
  [Route("/getTile/{LayerName}/{zIndex}/{xIndex}/{yIndex}/{StaticResource}", "GET")]
  public class GetTile : IReturn<Stream>
  {
    public string LayerName { get; set; }
    public string StaticResource { get; set; }
    public int ZIndex { get; set; }
    public int XIndex { get; set; }
    public int YIndex { get; set; }
  }

  public static class GetTileExtensions
  {
    private const string CacheKeyFormat = @"{0}-{1}-{2}-{3}-{4}";
    public static string CacheKey(this GetTile request)
    {
      return CacheKeyFormat.Fmt(
        request.LayerName,
        request.ZIndex,
        request.XIndex,
        request.YIndex,
        request.StaticResource);
    }    
  }
}