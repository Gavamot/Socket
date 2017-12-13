using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Service.Core;
using Service.Services;
using Service.Configuration;

namespace Service
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public const string ConfigSection = "AppConfiguration";
        public static DeviceService Ds;


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddOptions();
            services.Configure<Config>(Configuration.GetSection(ConfigSection));
            services.AddCors();

            var config = Configuration.GetSection(ConfigSection).Get<Config>().AscConfig;
            var logFactory = Configuration.Get<LoggerFactory>();
            logFactory.AddFile("Logs/mylog-{Date}.txt");

            AscPool.Init(config.Connect, config.ConnectionPoolSize, logFactory.CreateLogger("PollSize"));
            Ds = new DeviceService(config.DeviceService, logFactory.CreateLogger("DeviceService"));
            Ds.Start();

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(
               options => options.WithOrigins("*").AllowAnyMethod()
            );

            app.UseMvc();

            
        }
    }
}
