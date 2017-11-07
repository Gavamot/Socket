using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Service.Core;
using Service.Services;

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

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddOptions();
            services.Configure<Config>(Configuration.GetSection(ConfigSection));

            services.AddSingleton<AscPool>(
                provider =>
                {
                    var config = Configuration.GetSection(ConfigSection).Get<Config>().AscConfig;
                    var loger = Configuration.Get<LoggerFactory>().CreateLogger("AscPool");
                    return  new AscPool(config, loger);
                }
            );
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMvc();
            loggerFactory.AddFile("Logs/mylog-{Date}.txt");
        }
    }
}
