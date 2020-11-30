
namespace Plugin.Sync.Commerce.EntitiesMigration
{
    using System.Reflection;
    using global::Plugin.Sync.Commerce.EntitiesMigration.Pipelines;
    using Microsoft.Extensions.DependencyInjection;
    using Plugin.Sync.Commerce.EntitiesMigration.Services;
    using Sitecore.Commerce.Core;
    using Sitecore.Framework.Configuration;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;
    using Plugin.Sync.Commerce.EntitiesMigration.Pipelines.Blocks;

    /// <summary>
    /// The configure sitecore class.
    /// </summary>
    public class ConfigureSitecore : IConfigureSitecore
    {
        /// <summary>
        /// The configure services.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            services.Sitecore().Pipelines(config => config
               .ConfigurePipeline<IConfigureServiceApiPipeline>(configure => configure.Add<ConfigureServiceApiBlock>())
               .AddPipeline<IExportEntitiesPipeline, ExportEntitiesPipeline>(
                configure =>
                {
                    configure
                    .Add<ExportCommerceEntitiesBlock>();
                })
             .AddPipeline<IImportEntitiesPipeline, ImportEntitiesPipeline>(
                configure =>
                {
                    configure
                    .Add<ImportCommerceEntitiesBlock>();
                }));

            services.RegisterAllCommands(assembly);
            services.AddTransient<ICommerceEntityService, CommerceEntityService>();
        }
    }
}