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
        private static AscPool Pool;
        public static BrigadeService Bs;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddOptions();
            services.Configure<Config>(Configuration.GetSection(ConfigSection));
            services.AddCors();

          
            // Создаем пул соединений
            var config = Configuration.GetSection(ConfigSection).Get<Config>().AscConfig;
            var loger = Configuration.Get<LoggerFactory>().CreateLogger("AscPool");
            Pool = new AscPool(config, loger);

            Bs = new BrigadeService(Pool, config.ServicesUpdateTimeMs.BrigedeGetDeviceList);
            Bs.Start();
            //services.AddSingleton(provider => Bs);

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

            loggerFactory.AddFile("Logs/mylog-{Date}.txt");
        }
    }
}
