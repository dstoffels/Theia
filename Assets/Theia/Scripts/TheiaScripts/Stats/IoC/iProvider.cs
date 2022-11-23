namespace Stats.IoC
{
    public interface iProvider<T>
    {
        BaseData data { get; }
        T GetValue(iConsumer<T> consumer);
        void AddConsumer(iConsumer<T> consumer);
    }
}
