namespace Stats.IoC
{
    public interface iProvider<T, TConsumer>
    {
        BaseData GetData();
        T GetState(TConsumer consumer);
        void AddConsumer(TConsumer consumer);
    }

    public interface iAttributeProvider: iProvider<int, iAttributeConsumer> { }
    public interface iSkillProvider: iProvider<int, iSkillConsumer> { }

    public interface iBodyPartProvider : iProvider<bool, iBodyPartConsumer> { }
}
