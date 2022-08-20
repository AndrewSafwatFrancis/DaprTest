using System.Text.Json.Serialization;
using TestDapr.Callback;

namespace Scrips.Integration.Nabidh.Api
{
    public class Startup
    {
        /// <summary>
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// </summary>
        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(o => o.JsonSerializerOptions
                    .ReferenceHandler = ReferenceHandler.IgnoreCycles)
                .AddDapr();
            services.AddEndpointsApiExplorer();
            services.AddGrpc();

            services.AddHealthChecks();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseAuthorization();
            app.UseAuthentication();
            app.UseCloudEvents();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapSubscribeHandler();
                endpoints.MapGrpcService<DaprServer>();
                endpoints.MapControllers();
            });
        }
    }
}