using Microsoft.Extensions.DependencyInjection;
using Plugin.Sync.Commerce.CatalogImport.Pipelines;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks;
using Plugin.Sync.Commerce.CatalogImport.ServiceBus;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.SQL;
using Sitecore.Framework.Configuration;
using Sitecore.Framework.Pipelines.Definitions.Extensions;
using System.Reflection;
using System.Web.Services.Description;
using System.Linq;

namespace Plugin.Sync.Commerce.CatalogImport
{


    public class ConfigureSitecore : IConfigureSitecore
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IServiceBusConsumer, ServiceBusConsumer>();


            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);
            
            services.AddScoped<CommandsController, CommandsController>();
            services.Sitecore().Pipelines(config => config
                .ConfigurePipeline<IConfigureServiceApiPipeline>(configure => configure.Add<ConfigureServiceApiBlock>())
                .AddPipeline<IImportCategoryPipeline, ImportCategoryPipeline>(
                    configure =>
                    {
                        configure.Add<ExtractCatalogEntityFieldsFromJsonDataBlock>()
                        .Add<CreateOrUpdateCategoryBlock>()
                        .Add<UpdateComposerFieldsBlock>();
                        //.Add<UpdateCustomComponentsBlock>();
                    })
                .AddPipeline<IImportSellableItemPipeline, ImportSellableItemPipeline>(
                    configure =>
                    {
                        configure.Add<ExtractCatalogEntityFieldsFromJsonDataBlock>()
                        .Add<CreateOrUpdateSellableItemBlock>()
                        .Add<UpdateComposerFieldsBlock>();
                        //.Add<UpdateCustomComponentsBlock>();
                    })
                .AddPipeline<IImportSellableItemFromContentHubPipeline, ImportSellableItemFromContentHubPipeline>(
                    configure =>
                    {
                        configure //.Add<GetAzureQueueMessageBlock>()
                        .Add<GetContentHubEntityBlock>()
                        .Add<ExtractCatalogEntityFieldsFromJsonDataBlock>()
                        .Add<CreateOrUpdateSellableItemBlock>()
                        .Add<UpdateComposerFieldsBlock>()
                        .Add<UpdateCustomComponentsBlock>();
                    })
                .AddPipeline<IImportCategoryFromContentHubPipeline, ImportCategoryFromContentHubPipeline>(
                    configure =>
                    {
                        configure //.Add<GetAzureQueueMessageBlock>()
                        .Add<GetContentHubEntityBlock>()
                        .Add<ExtractCatalogEntityFieldsFromJsonDataBlock>()
                        .Add<CreateOrUpdateCategoryBlock>()
                        .Add<UpdateComposerFieldsBlock>()
                        .Add<UpdateCustomComponentsBlock>();
                    })
                //.ConfigurePipeline<IPersistEntityPipeline>(
                //   configure =>
                //   {
                //       configure.Add<AddSellableItemToUpdatedSellableItemsListBlock>().Before<PersistEntityBlock>();
                //   })
                );

            services.RegisterAllCommands(assembly);
        }
    }
}