using ServiceStack;
using ServiceStack.Configuration;
using Tile.Caching;
using Tile.ServiceModel;

namespace Tile.ServiceInterface
{
  public class TileLayer : Service
  {
    [CacheResponse(CacheControl = CacheControl.MustRevalidate | CacheControl.Public, Duration = 604800)]
    public object Get(MetaTile request)
    {
      return new HttpResult(TileFactory.MetaTile(request), MimeTypes.ImagePng);
    }

    [CacheResponse(CacheControl = CacheControl.MustRevalidate | CacheControl.Public, Duration = 604800)]
    public object Get(SpectrumMetaTile request)
    {
      return new HttpResult(TileFactory.SpectrumMetaTile(request), MimeTypes.ImagePng);
    }

    public object Get(GetTile request)
    {
      var log = this.Log();
      var cacheKey = request.CacheKey();
      log.DebugFormat("Cache Id: {0}", cacheKey);
      if (TileFactory.ServiceUri.Current.IsNullOrEmpty())
      {
        TileFactory.ServiceUri.Current = (new AppSettings()).Get("TileService", TileFactory.ServiceUri.Default);
      }
      var result = Cache.ToResultUsingCache(cacheKey, TileFactory.RawTile(request));
      return new HttpResult(result.Image, MimeTypes.ImagePng);
    }

    public object Get(GetTileBounds request)
    {
      return TileCompute.GetBounds(request.XIndex, request.YIndex, request.ZIndex);
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