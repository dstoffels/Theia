namespace Stats.IoC
{
    public interface iProvider<T>
    {
        BaseData GetData();
        T GetValue(iConsumer<T> consumer);
        void AddConsumer(iConsumer<T> consumer);
    }
}
