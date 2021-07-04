using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BaseBallDataScraper.Logics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BaseBallDataScraper
{
    class Startup
    {
        public static IServiceCollection ConfigureServices()
        {
            var services = new ServiceCollection();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
            IConfiguration configuration = builder.Build();
            services.AddSingleton(configuration);

            services.AddSingleton<DatabaseAccessor>();

            services.AddTransient<ScrapBaseballData>();

            return services;
        }
    }
}
