using ServiceStack;
using Tile.Caching;
using Tile.ServiceModel;

namespace Tile.ServiceInterface
{
  public class TileLayer : Service
  {
    public object Get(MetaTile request)
    {
      return new HttpResult(TileFactory.MetaTile(request), MimeTypes.ImagePng);
    }

    public object Get(SpectrumMetaTile request)
    {
      return new HttpResult(TileFactory.SpectrumMetaTile(request), MimeTypes.ImagePng);
    }

    public object Get(GetTile request)
    {
      var log = this.Log();
      var cacheKey = request.CacheKey();
      log.DebugFormat("Cache Id: {0}", cacheKey);
      var result = Cache.ToResultUsingCache(cacheKey, TileFactory.RawTile(request));
      return new HttpResult(result.Image, MimeTypes.ImagePng);
    }

    public object Get(GetTileBounds request)
    {
      return TileCompute.GetBounds(request.xIndex, request.yIndex, request.zIndex);
    }

    public object Get(GetTileRequests request)
    {
      var point = new GeoPoint
      {
        Latitude = request.Latitude,
        Longitude = request.Longitude
      };
      return TileRequest.LatLongToTiles(point);
    }

    public object Get(GetBoundedTileRequests request)
    {
      var southEast = new GeoPoint { Latitude = request.SouthEastLat, Longitude = request.SouthEastLng };
      var northWest = new GeoPoint { Latitude = request.NorthWestLat, Longitude = request.NorthWestLng };
      return TileRequest.BoundsToTiles(southEast, northWest);
    }
  }
}