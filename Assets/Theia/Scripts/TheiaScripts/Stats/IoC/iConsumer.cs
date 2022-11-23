namespace Stats.IoC
{
    public interface iConsumer<T>
    {
        BaseData GetData();
        void Subscribe(iProvider<T> provider);
        void Update(iProvider<T> provider);
    }
}
