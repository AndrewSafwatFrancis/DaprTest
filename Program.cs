using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;

namespace Scrips.Integration.Nabidh.Api
{
    public static class Program
    {
        public static void Main(string[] args)
        {

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                /* local startup */
                CreateHostBuilder(args).Build().Run();
            }
            else
            {
                /* cloud startup*/
                var configuration = GetConfiguration();
                CreateHostBuilder(configuration, args).Build().Run();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {

            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(options =>
                    {
                        // Setup a HTTP/2 endpoint without TLS.
                        options.ListenLocalhost(8013, o => o.Protocols =
                            HttpProtocols.Http2);

                        options.ListenLocalhost(7013,
                            listenOptions => { listenOptions.Protocols = HttpProtocols.Http1AndHttp2; });
                    });
                    webBuilder.UseStartup<Startup>();
                });
        }

        private static IHostBuilder CreateHostBuilder(IConfiguration configuration, string[] args)
        {
            return Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.CaptureStartupErrors(false)
                        .UseKestrel(options =>
                        {
                            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
                            var (httpPort, grpcPort) = GetDefinedPorts(configuration);
                            options.Listen(IPAddress.Any, httpPort,
                                listenOptions => { listenOptions.Protocols = HttpProtocols.Http1AndHttp2; });
                            options.Listen(IPAddress.Any, grpcPort,
                                listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
                        })
                        .UseStartup<Startup>();
                });
        }

        private static IConfiguration GetConfiguration()
        {
            var environmentVar = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json", false, true)
                .AddJsonFile($"appsettings.{environmentVar}.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            return builder.Build();
        }

        private static (int httpPort, int grpcPort) GetDefinedPorts(IConfiguration config)
        {
            var grpcPort = config.GetValue("GRPC_PORT", 8013);
            var port = config.GetValue("PORT", 7013);
            return (port, grpcPort);
        }
    }
}
