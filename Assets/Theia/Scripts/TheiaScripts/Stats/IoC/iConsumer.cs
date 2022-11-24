namespace Stats.IoC
{
    public interface iConsumer<TProvider>
    {
        BaseData GetData();
        void Subscribe(TProvider provider);
        void Update(TProvider provider);

    }

    public interface iAttributeConsumer : iConsumer<iAttributeProvider> { }
    public interface iSkillConsumer : iConsumer<iSkillProvider> { }

    public interface iBodyPartConsumer : iConsumer<iBodyPartProvider> { }
}
