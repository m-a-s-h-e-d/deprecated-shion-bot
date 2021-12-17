using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Addons.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Victoria;

namespace Shion.Services
{
    /// <summary>
    /// Class containing static helper methods to add services and singleton instances to an <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ConfigureServices
    {
        /// <summary>
        /// Adds all required <see cref="DiscordShardedClientService"/> to the main service collection.
        /// </summary>
        /// <param name="services">A <see cref="IServiceCollection"/> to be passed.</param>
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddHostedService<CommandHandler>()
                .AddStatusCollection()
                .AddMediaCollection();
        }

        private static IServiceCollection AddStatusCollection(this IServiceCollection services)
        {
            return services
                .AddHostedService<BotStatusService>();
        }

        private static IServiceCollection AddMediaCollection(this IServiceCollection services)
        {
            return services
                .AddLavaNode(lavaConfig =>
                {
                    lavaConfig.SelfDeaf = true;
                    lavaConfig.Authorization = "lavalink";
                });
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
