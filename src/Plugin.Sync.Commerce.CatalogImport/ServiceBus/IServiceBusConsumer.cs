using System.Threading.Tasks;

namespace Plugin.Sync.Commerce.CatalogImport.ServiceBus
{
    public interface IServiceBusConsumer
    {
        void RegisterOnMessageHandlerAndReceiveMessages();
        Task CloseQueueAsync();
    }
}