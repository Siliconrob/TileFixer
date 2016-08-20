using System;
using System.Diagnostics;
using Funq;
using ServiceStack;
using ServiceStack.Admin;
using ServiceStack.Api.Swagger;
using ServiceStack.Configuration;
using ServiceStack.MsgPack;
using ServiceStack.ProtoBuf;
using ServiceStack.Redis;
using ServiceStack.Text;
using ServiceStack.Validation;
using ServiceStack.VirtualPath;
using Tile.ServiceInterface;
using Tile.ServiceModel;

namespace Tile.Fixer
{
  public class RouteAppHost : AppHostBase
  {
    private const string RedisHost = @"RedisServer";
    //Tell Service Stack the name of your application and where to find your web services
    // if this is not the current assembly the routes will not appear properly.
    public RouteAppHost()
      : base(AppName, typeof (TileLayer).Assembly)
    {
    }

    private static string AppName
    {
      get
      {
        var asm = typeof(RouteAppHost).Assembly;
        var info = FileVersionInfo.GetVersionInfo(asm.Location);
        var version = info.FileVersion;
        var name = asm.FullName.SplitOnFirst(',').FirstNonDefaultOrEmpty();
        return "{0} {1}".Fmt(name, version);
      }
    }

    public override void Configure(Container container)
    {
      RegisterUnHandledExceptions();
      var redisServerUrl = (new AppSettings()).Get(RedisHost, "localhost:6379");
      container.Register<IRedisClientsManager>(c => new RedisManagerPool(redisServerUrl));
      container.Register(c => c.Resolve<IRedisClientsManager>().GetCacheClient()).ReusedWithin(ReuseScope.None);

      Plugins.Add(new ServerEventsFeature());
      Plugins.Add(new SessionFeature());

      // This enables cross-domain calls from Javascript client.
      Plugins.Add(new CorsFeature());
      Plugins.Add(new PostmanFeature());
      Plugins.Add(new MsgPackFormat());
      Plugins.Add(new ProtoBufFormat());
      // logs available at  /requestLogs
      Plugins.Add(new RequestLogsFeature
      {
        RequestLogger = new CsvRequestLogger(
            new FileSystemVirtualPathProvider(this, Config.WebHostPhysicalPath),
            "requestlogs/{year}-{month}/{year}-{month}-{day}.csv",
            "requestlogs/{year}-{month}/{year}-{month}-{day}-errors.csv",
            TimeSpan.FromSeconds(5))
      });

      Plugins.Add(new AdminFeature());

      // Register service validators
      Plugins.Add(new ValidationFeature());
      container.RegisterValidators(typeof(TileLayer).Assembly);

      // Add swagger API
      Plugins.Add(new SwaggerFeature());
    }

    private void RegisterServiceExceptions()
    {
      ServiceExceptionHandlers.Add((httpReq, serviceReq, exception) =>
      {
        var currentLog = this.Log();
        currentLog.ErrorFormat("Http Request DTO {0}", httpReq.Dto.Dump());
        currentLog.ErrorFormat("Service Request {0}", serviceReq.Dump());
        currentLog.ErrorFormat("Exception {0} at {1}", exception.Message, exception.StackTrace);
        return DtoUtils.CreateErrorResponse(serviceReq, exception);
      });
    }

    private void RegisterUncaughtExceptions()
    {
      //Custom global uncaught exception handling strategy
      UncaughtExceptionHandlers.Add((req, res, operationName, ex) =>
      {
        var currentLog = this.Log();
        currentLog.ErrorFormat("Request {0}", req.Dump());
        currentLog.ErrorFormat("Response {0}", res.Dump());
        currentLog.ErrorFormat("OperationName {0} Exception {1} at {2}",
          operationName,
          ex.Message,
          ex.StackTrace);
        res.Write("UncaughtException {0}".Fmt(ex.GetType().Name));
        res.EndRequest(true);
      });
    }

    private void RegisterUnHandledExceptions()
    {
      RegisterUncaughtExceptions();
      RegisterServiceExceptions();
    }
  }
}