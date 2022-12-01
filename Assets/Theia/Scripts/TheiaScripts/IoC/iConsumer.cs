namespace Theia.IoC
{
    public interface iConsumer<TProvider>
    {
        void Subscribe(TProvider provider);
        void Notify(TProvider provider);
    }

    public interface iAttributeConsumer : iConsumer<iAttributeProvider> { }
    public interface iSkillConsumer : iConsumer<iSkillProvider> { }
    public interface iBodyPartConsumer : iConsumer<iBodyPartProvider> { }
    public interface iArmorConsumer : iConsumer<iArmorProvider> { }
}
