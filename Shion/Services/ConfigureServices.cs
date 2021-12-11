using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Shion.Services
{
    public static class ConfigureServices
    {
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddHostedService<CommandHandler>()
                .AddStatusCollection();
        }

        private static IServiceCollection AddStatusCollection(this IServiceCollection services)
        {
            return services
                .AddHostedService<BotStatusService>();
        }

        private static IServiceCollection AddSocialsCollection(this IServiceCollection services)
        {
            return services;
        }

        private static IServiceCollection AddGamesCollection(this IServiceCollection services)
        {
            return services;
        }

        private static IServiceCollection AddToolsCollection(this IServiceCollection services)
        {
            return services;

            // services.AddHostedService<SchedulingService>(); //TODO This should be a BackgroundService, does not need access to Discord Client (most likely)
        }
    }
}
