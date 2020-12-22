## Intro

I wrote Content Hub Connector based on a few integrations for our clients where Content Hub needed to be synced into Sitecore Commerce Engine and made it available to everybody on [this GitHub repo](https://github.com/sergyatsenko/ContentHubConnector).



## What is Content Hub Connector for Sitecore Experience Commerce and how it works

**Content Hub Connector for Sitecore Commerce** is a plugin I built to allow Content Hub entities to be synchronized with Sitecore Experience Commerce catalog. It works much the same way and follows the same architectural approach as Sitecore's own "[Sitecore Connect for Sitecore CMP](https://docs.stylelabs.com/content/3.3.x/user-documentation/cmp/integrate/sitecore-connector-cmp.html)", but runs inside of Sitecore Commerce Engine and updates its catalog (whenever mapped entities are getting created or updated in Content Hub). For this reason, most of the diagram from [Sitecore CMP Connect documentation page](https://docs.stylelabs.com/content/3.3.x/user-documentation/cmp/integrate/sitecore-connector-cmp.html) still applies for my Connector as well:

[Connector High Level Architecture]()



When an entity is created or updated in Content Hub, the trigger is fired and the message is getting sent to Azure Service Bus (from the Content Hub instance). Sitecore Commerce Engine is listening to the same queue and whenever it reads the incoming message, it retrieves entity data from Content Hub and saves changes

Here's another representation of the sync mechanism used in Commerce Catalog using the mapping configuration to match fields of incoming Content Hub entity to its counterpart in Commerce. This way Content Hub can be used as the single "source of truth" system to effectively manage products and categories in Sitecore Commerce Catalog. in my Content Hub Connector

[Connector notifications]()





## Setting up Azure Service Bus and Scrip, Action, Trigger in Content Hub

Steps to configure Azure Service Bus and then create needed Script, Action, and Trigger in Content Hub are very much the same as described in my blog post on [How to Send Messages from Content Hub to Azure Service Bus](https://xcentium.com/Blog/2020/06/28/How-to-Send-Messages-from-Content-Hub-to-Azure-Service-Bus).



## Including Content Hub Connector plugin in **your** Sitecore Commerce Engine solution

- Clone [ContentHubConnector repo from Github](https://github.com/sergyatsenko/ContentHubConnector)
- Add the [Plugin.Sync.Commerce.CatalogImport](https://github.com/sergyatsenko/ContentHubConnector/tree/main/src/Plugin.Sync.Commerce.CatalogImport) project to your Commerce Engine Solution.
- Add the following settings to **config.json** in **your** Commerce Engine project (use mine as an example)
  - **EnableContentHubSync**: true enables queue processing, false disables queue processing, but keeps custom endpoints to sync entities on demand
  - **ServiceBusConnectionString**: copy and paste the connection string from **your** Azure Service Bus topic
  - **ServiceBusTopicName**: copy and past the name of the topic in use in **your** Azure service bus configuration
  - **ServiceBusSubscriptionName**: copy and paste the name of **your** subscription to use



## Mapping Configuration

Mapping configuration is a set of policies to map specific entities and their fields in Content Hub to their counterparts in Sitecore Commerce Catalog.

I included sample configurations to map Commerce [SellableItem](https://github.com/sergyatsenko/ContentHubConnector/blob/main/src/Sitecore.Commerce.Engine/wwwroot/data/Environments/PlugIn.CatalogImport.SellableItemMappingPolicySet-1.0.0.json) and [Category](https://github.com/sergyatsenko/ContentHubConnector/blob/main/src/Sitecore.Commerce.Engine/wwwroot/data/Environments/PlugIn.CatalogImport.CategoryMappingPolicySet-1.0.0.json) to their counterpart entities in Content Hub.

TODO: explain mapping configuration
