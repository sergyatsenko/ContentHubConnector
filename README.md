## Intro

I wrote Content Hub Connector based on a few integrations for our clients where Content Hub needed to be synced into Sitecore Commerce Engine and made it available to everybody on [this GitHub repo](https://github.com/sergyatsenko/ContentHubConnector).



## What is Content Hub Connector for Sitecore Experience Commerce and how it works

**Content Hub Connector for Sitecore Commerce** is a plugin I built to allow Content Hub entities to be synchronized with Sitecore Experience Commerce catalog. It works much the same way and follows the same architectural approach as Sitecore's own "[Sitecore Connect for Sitecore CMP](https://docs.stylelabs.com/content/3.3.x/user-documentation/cmp/integrate/sitecore-connector-cmp.html)", but runs inside of Sitecore Commerce Engine and updates its catalog (whenever mapped entities are getting created or updated in Content Hub). For this reason, most of the diagram from [Sitecore CMP Connect documentation page](https://docs.stylelabs.com/content/3.3.x/user-documentation/cmp/integrate/sitecore-connector-cmp.html) still applies for my Connector as well:

[Connector High Level Architecture]()



When an entity is created or updated in Content Hub, the trigger is fired and the message is getting sent to Azure Service Bus (from the Content Hub instance). Sitecore Commerce Engine is listening to the same queue and whenever it reads the incoming message, it retrieves entity data from Content Hub and saves changes

Here's another representation of the sync mechanism used in Commerce Catalog using the mapping configuration to match fields of incoming Content Hub entity to its counterpart in Commerce. This way Content Hub can be used as the single "source of truth" system to effectively manage products and categories in Sitecore Commerce Catalog. in my Content Hub Connector

[Connector notifications]()

### Key concepts
* When data is being read from CH I use CH's RESTful API to read and navigate through CH data entities, so no entity or field names are hard-coded there.
* When data is being saved to SCX, I lookup the target field by its name, which comes from the configuration, so (almost) no entity or field names are hard-coded on the SXC side either.
* CH has a mechanism for notifying subscribers about data changes via Azure Service Bus - I listen to these messages to trigger sync on each change, so the sync process is close to real-time
* "Mapping configurations" is a set of SXC configuration policies, which maps SXC catalog items and their fields to CH entities and fields inside them. Any field on SXC item (native, custom, Composer fields) can be mapped to CH and populated from there. The left side of each mapping is the name of the SXC item field and the right side is the JSON path to where its value can be found in CH Entity, here's an example
"Brand": "properties.SAPBrand",
"Manufacturer": "Manufacturer",
"TypeOfGoods": "TypeOfGoods",
"DisplayName": "properties.ProductName",
"Description": "properties.ShortDescription.en-US"

### Feature list
* Sync entities from multiple Content Hub instances into separate catalogs on the same SXC instance
* Listening to Azure Service Bus queue, populated by CH (same mechanism as in Sitecore CMP connector). Reads an ID or updated/created/deleted entity, retrieves JSON of that and all related entities and updates/creates/deletes target Catalog item in SXC
* Optional support for item versioning and workflows (e.g. overwrite drafts, but create new versions for published items)
* Support updating any native, custom, and composer fields on target Categories, Sellable Items, and Variants (minus Composer fields on Variants as SXC don't support that)
* Optional support for syncing list prices from CH
* Import one or multiple CH asset public link URLs into single or multiple fields on SXC Catalog items
* Using Azure service bus reader for close to real-time sync
* Utilizing CH RESTful APIs in a generic way, which allows populating SXC item fields with data values
* An option to clear out target field on already existing SXC catalog items when a mapped value is no longer present in source (CH)
* Optional audit log tracking all sync events and failures with detailed info on fields updated
* Support for mapping configuration policies, which allows defining a dictionary-like structure, which allows mapping SXC item field names (keys) to JSON path, pointing to value to be populated from CH entity (value)
* Support for relations navigation/traversing of CH entities, allowing to extract data fields from related entities (parent, child, one to one, one to many and many to many relationships are all supported). Navigable entities are specified in on the same mapping policies as field mapping
* Ability to navigate to related Entities
* Separate mapping configuration policies to map SXC Category, Sellable Item, and Variant fields to appropriate CH entities
* Ability to associate more than one CH entity types with one SXC catalog entity, using separate mapping configurations, linking different CH schema types with target Catalog item in SXC. e.g. both EntityA and EntityB can be mapped to one SellableItem (or Category or Variant)


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
