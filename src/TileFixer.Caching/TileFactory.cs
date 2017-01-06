using System;
using System.Drawing;
using System.Drawing.Imaging;
using RestSharp;
using ServiceStack;
using ServiceStack.Text;
using Tile.ServiceModel;
using IRestClient = RestSharp.IRestClient;

namespace Tile.Caching
{
  public static class TileFactory
  {
    public static class ServiceUri
    {
      public static string Current { get; set; }
      public static string Default = @"http://localhost:8080/rest/Spatial/MapTilingService/MyTiles/Tiles";
    }

    private static Func<GetTile, ServiceInput, string> _serviceTransForm;
    //public static string ServiceUri { get; set; }
    private const string ParamUrl = @"{0}/{1}/{2}/{3}:{4}/{5}";

    public static Func<GetTile, ServiceInput, string> ServiceTransForm
    {
      get
      {
        _serviceTransForm = _serviceTransForm ?? DefaultSpectrum;
        return _serviceTransForm;
      }
      set
      {
        _serviceTransForm = value;
      }
    }

    public static string DefaultSpectrum(GetTile request, ServiceInput serviceInput)
    {
      serviceInput = serviceInput ?? new ServiceInput();
      serviceInput.TargetParamFmt = serviceInput.TargetParamFmt ?? ParamUrl;
      serviceInput.ServiceUri = serviceInput.ServiceUri ?? DefaultServiceUri();

      return serviceInput.TargetParamFmt.Fmt(
        serviceInput.ServiceUri.PathAndQuery,
        request.LayerName,
        request.ZIndex + 1,
        request.XIndex + 1,
        request.YIndex + 1,
        request.StaticResource);
    }

    private static byte[] _empty;

    public static EmptyOptions EmptyTileOptions { get; set; }

    private static EmptyOptions FillInDefaults(EmptyOptions current)
    {
      current = current ?? EmptyOptions.Defaults();
      current.Border = current.Border ?? EmptyOptions.Defaults().Border;
      current.Border.Color = current.Border.Color ?? EmptyOptions.Defaults().Border.Color;
      return current;
    }

    public static byte[] EmptyTile()
    {
      EmptyTileOptions = FillInDefaults(EmptyTileOptions);
      using (var image = new Bitmap(TileRequest.TileSize, TileRequest.TileSize, PixelFormat.Format32bppArgb))
      {
        var g = Graphics.FromImage(image);
        g.FillRectangle(Brushes.White, 0f, 0f, image.Width, image.Height);
        if (!EmptyTileOptions.Border.Hide)
        {
          var color = Color.Black;
          if (EmptyTileOptions.Border.Color.HasValue)
          {
            color = EmptyTileOptions.Border.Color.Value;
          }
          g.DrawRectangle(new Pen(color), 0f, 0f, image.Width, image.Height);
        }
        image.MakeTransparent(Color.White);
        var converter = new ImageConverter();
        var data = (byte[]) converter.ConvertTo(image, typeof(byte[]));
        return data;
      }
    }

    public static byte[] Image(EmptyTile request)
    {
      _empty = _empty ?? EmptyTile();
      return _empty;
    }

    public static byte[] Image(GetTile request)
    {
      var uri = !String.IsNullOrEmpty(ServiceUri.Current) ? new Uri(ServiceUri.Current) : DefaultServiceUri();
      return GetTile(request, uri).RawBytes;
    }

    public static byte[] Image(GetTile request, string serviceUri)
    {
      var uri = !String.IsNullOrEmpty(serviceUri) ? new Uri(ServiceUri.Current) : DefaultServiceUri();
      return GetTile(request, uri).RawBytes;
    }

    public static byte[] Image(GetTile tileRequest, IRestClient client, IRestRequest webRequest)
    {
      return GetTile(tileRequest, client, webRequest).RawBytes;
    }

    private static Uri DefaultServiceUri()
    {
      return new Uri("http://localhost:8080/rest/Spatial/MapTilingService/NamedTiles");
    }

    private static IRestResponse GetTile(GetTile tileRequest, IRestClient client, IRestRequest webRequest)
    {
      var log = typeof(TileFactory).Log();
      var msg = new
      {
        Common = tileRequest.ToGetUrl(),
        NonStandard = client.BuildUri(webRequest)
      };
      log.DebugFormat("Tile request {0}", msg.Dump());
      IRestResponse tileResponse;
      try
      {
        tileResponse = client.ExecuteAsGet(webRequest, HttpMethods.Get);
      }
      catch (Exception exc)
      {
        log.ErrorFormat("Issue: {0} at {1}", exc.Message, exc.StackTrace);
        throw;
      }
      return tileResponse;
    }

    private static IRestResponse GetTile(GetTile request, Uri serviceUri)
    {
      var client = new RestClient(serviceUri.AbsoluteUri.Replace(serviceUri.PathAndQuery, ""));
      var tileRequest = new RestRequest(request.ConvertRequest(new ServiceInput { ServiceUri = serviceUri }));
      return GetTile(request, client, tileRequest);
    }

    public static Func<CachedTile> RawTile(GetTile request)
    {
      return RawTile(request, ServiceUri.Current);
    }

    public static Func<CachedTile> RawTile(GetTile request, string serviceUri)
    {
      return () =>
      {
        var log = typeof (TileFactory).Log();
        var bounds = TileCompute.GetBounds(request.XIndex, request.YIndex, request.ZIndex);
        log.DebugFormat("Tile bounds: {0}", bounds);

        var tileResponse = GetTile(request, new Uri(serviceUri));
        log.DebugFormat("Content Type: {0}, size in bytes {1}", tileResponse.ContentType,
          (tileResponse.RawBytes ?? new byte[] {}).LongLength);

        return new CachedTile {Image = tileResponse.RawBytes ?? EmptyTile(), Bounds = bounds};
      };
    }

    private static byte[] GetTextTile(string text)
    {
      using (var image = new Bitmap(TileRequest.TileSize, TileRequest.TileSize, PixelFormat.Format32bppArgb))
      {
        var g = Graphics.FromImage(image);
        g.FillRectangle(Brushes.White, 0f, 0f, image.Width, image.Height);
        g.DrawRectangle(new Pen(Color.Blue), 0f, 0f, image.Width, image.Height);
        g.DrawString(text, new Font("Consolas", 14), Brushes.Blue, 0f, 0f);
        image.MakeTransparent(Color.White);
        var converter = new ImageConverter();
        var data = (byte[]) converter.ConvertTo(image, typeof (byte[]));
        return data;
      }
    }

    public static byte[] SpectrumMetaTile(SpectrumMetaTile request)
    {
      var tileText = "NonStandard{0}Z: {1}{0}X: {2}{0}Y: {3}{0}".Fmt(
        Environment.NewLine,
        request.ZIndex + 1,
        request.XIndex + 1,
        request.YIndex + 1);
      return GetTextTile(tileText);
    }

    public static byte[] MetaTile(MetaTile request)
    {
      var tileText = "Common{0}Z: {1}{0}X: {2}{0}Y: {3}{0}".Fmt(
        Environment.NewLine,
        request.ZIndex,
        request.XIndex,
        request.YIndex);
      return GetTextTile(tileText);
    }
  }

  public static class GetTileExtensions
  {
    public static string ConvertRequest(this GetTile input, ServiceInput target)
    {
      return TileFactory.ServiceTransForm(input, target);
    }
  }

  public class ServiceInput
  {
    public Uri ServiceUri { get; set; }
    public string TargetParamFmt { get; set; }
  }

}