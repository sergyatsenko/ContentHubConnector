//using Microsoft.Azure; // Namespace for CloudConfigurationManager
//using Microsoft.Azure.ServiceBus;
//using Microsoft.Azure.Storage; // Namespace for CloudStorageAccount
//using Microsoft.Azure.Storage.Queue; // Namespace for Queue storage types
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Sync.Commerce.CatalogImport.Extensions;
using Plugin.Sync.Commerce.CatalogImport.Models;
using Plugin.Sync.Commerce.CatalogImport.Pipelines.Arguments;
using Plugin.Sync.Commerce.CatalogImport.Policies;
using Serilog;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Core.Commands;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Composer;
using Sitecore.Framework.Caching;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace Plugin.Sync.Commerce.CatalogImport.Pipelines.Blocks
{
    /// <summary>
    /// Import data into an existing Category or new SellableItem entity
    /// </summary>
    [PipelineDisplayName("GetAzureQueueMessageBlock")]
    public class GetAzureQueueMessageBlock : PipelineBlock<ImportCatalogEntityArgument, ImportCatalogEntityArgument, CommercePipelineExecutionContext>
    {
        #region Private fields
        //static ISubscriptionClient _subscriptionClient;
        static string _connectionString = "Endpoint=sb://xccontenthubdemo.servicebus.windows.net/;SharedAccessKeyName=products_content;SharedAccessKey=yK0BD0C+ijNNWfl3cUJdmOJgC4MT/QSZZFqAAir7MZQ=";
        static string _subscriptionName = "quadfectaproductsync";
        static string _topicName = "products_content";
        #endregion

        #region Public methods
        /// <summary>
        /// Public contructor
        /// </summary>
        /// <param name="commerceCommander"></param>
        /// <param name="composerCommander"></param>
        /// <param name="importHelper"></param>
        public GetAzureQueueMessageBlock()
        {
            try
            {

                //_subscriptionClient = new SubscriptionClient(connectionString, topicName, subscriptionName);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        /// <summary>
        /// Main execution point
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<ImportCatalogEntityArgument> Run(ImportCatalogEntityArgument arg, CommercePipelineExecutionContext context)
        {
            try
            {
                //ProcessMessages();
                var subClient = SubscriptionClient.CreateFromConnectionString(_connectionString, _topicName, _subscriptionName);
                subClient.OnMessage(m =>
                {
                    Log.Error(m.GetBody<string>());
                });
                var message = subClient.Peek();//.ConfigureAwait(false);
                var entityId = message.Properties["target_id"];
                //subClient.OnMessage(m =>
                //{
                //    Console.WriteLine(m.GetBody<string>());
                //});

                await Task.Run(() => true);

                return arg;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error creating or updating Category Entity {arg}. {ex.Message}";
                Log.Error(ex, errorMessage);
                context.Abort(errorMessage, ex);
                return arg;
            }
        }


        #endregion

        #region Private methods
        //private void ProcessMessages()
        //{

        //    var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
        //    {
        //        // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
        //        // Set it according to how many messages the application wants to process in parallel.
        //        MaxConcurrentCalls = 1,

        //        // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
        //        // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
        //        AutoComplete = false
        //    };

        //    // Register the function that processes messages.
        //    _subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        //    //CloudStorageAccount storageAccount = CloudStorageAccount.Parse("Endpoint=sb://xccontenthubdemo.servicebus.windows.net/;SharedAccessKeyName=cmp_content;SharedAccessKey=N9tzt8Zo/zIy1SL+b2H8JydIzfTMvYkc95BIF5tTVto=;EntityPath=cmp_content");
        //    //CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
        //    //CloudQueue queue = queueClient.GetQueueReference("myqueue");
        //    //CloudQueueMessage peekedMessage = queue.PeekMessage();
        //    //Log.Information(peekedMessage.AsString);
        //    //return peekedMessage.AsString;
        //}

        //static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        //{
        //    // Process the message.
        //    Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

        //    // Complete the message so that it is not received again.
        //    // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
        //    //await _subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
        //    await _subscriptionClient.AbandonAsync(message.SystemProperties.LockToken).ConfigureAwait(false);

        //    // Note: Use the cancellationToken passed as necessary to determine if the subscriptionClient has already been closed.
        //    // If subscriptionClient has already been closed, you can choose to not call CompleteAsync() or AbandonAsync() etc.
        //    // to avoid unnecessary exceptions.
        //}

        //static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        //{
        //    Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
        //    var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
        //    Console.WriteLine("Exception context for troubleshooting:");
        //    Console.WriteLine($"- Endpoint: {context.Endpoint}");
        //    Console.WriteLine($"- Entity Path: {context.EntityPath}");
        //    Console.WriteLine($"- Executing Action: {context.Action}");
        //    return Task.CompletedTask;
        //}
        #endregion
    }
}