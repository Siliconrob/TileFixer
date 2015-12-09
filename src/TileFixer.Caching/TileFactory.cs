﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using RestSharp;
using ServiceStack;
using ServiceStack.Configuration;
using ServiceStack.Text;
using Tile.ServiceModel;
using IRestClient = RestSharp.IRestClient;

namespace Tile.Caching
{
  public static class TileFactory
  {
    private const string ServiceUrl = @"TileService";
    private const string ParamUrl = @"{0}/{1}/{2}/{3}:{4}/{5}";

    private static string RestParams(GetTile request, string serviceKey)
    {
      return ParamUrl.Fmt(
        ServiceUri(serviceKey).PathAndQuery,
        request.LayerName,
        request.zIndex + 1,
        request.xIndex + 1,
        request.yIndex + 1,
        request.StaticResource);
    }

    public static byte[] Image(GetTile request, string serviceKey = ServiceUrl)
    {
      return GetTile(request, serviceKey).RawBytes;
    }

    public static byte[] Image(GetTile tileRequest, IRestClient client, IRestRequest webRequest)
    {
      return GetTile(tileRequest, client, webRequest).RawBytes;
    }

    private static Uri ServiceUri(string tileServiceKey)
    {
      return new Uri(new AppSettings().Get(tileServiceKey, "http://localhost:8080/rest/Spatial/MapTilingService/NamedTiles"));
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

    private static IRestResponse GetTile(GetTile request, string tileServiceKey)
    {
      var uri = ServiceUri(tileServiceKey);
      var client = new RestClient(uri.AbsoluteUri.Replace(uri.PathAndQuery, ""));
      var tileRequest = new RestRequest(RestParams(request, tileServiceKey));
      return GetTile(request, client, tileRequest);
    }

    public static Func<CachedTile> RawTile(GetTile request, string serviceKey = ServiceUrl)
    {
      return () =>
      {
        var log = typeof (TileFactory).Log();
        var bounds = TileCompute.GetBounds(request.xIndex, request.yIndex, request.zIndex);
        log.DebugFormat("Tile bounds: {0}", bounds);

        var tileResponse = GetTile(request, serviceKey);
        log.DebugFormat("Content Type: {0}, size in bytes {1}", tileResponse.ContentType,
          (tileResponse.RawBytes ?? new byte[] {}).LongLength);

        return new CachedTile {Image = tileResponse.RawBytes, Bounds = bounds};
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
        request.zIndex + 1,
        request.xIndex + 1,
        request.yIndex + 1);
      return GetTextTile(tileText);
    }

    public static byte[] MetaTile(MetaTile request)
    {
      var tileText = "Common{0}Z: {1}{0}X: {2}{0}Y: {3}{0}".Fmt(
        Environment.NewLine,
        request.zIndex,
        request.xIndex,
        request.yIndex);
      return GetTextTile(tileText);
    }
  }
}