namespace Stats.IoC
{
    public interface iConsumer<T>
    {
        BaseData data { get; }
        void Subscribe(iProvider<T> provider);
        void Update(iProvider<T> provider);
    }
}
