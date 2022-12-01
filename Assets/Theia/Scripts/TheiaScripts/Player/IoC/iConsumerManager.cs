namespace Theia.IoC
{
    public interface iConsumerManager<TProvider>
    {
        void SubscribeAll(iProviderManager<TProvider> providerManager);
    }
}
