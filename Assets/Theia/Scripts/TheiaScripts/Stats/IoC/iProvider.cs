namespace Stats.IoC
{
    public interface iProvider<TConsumer>
    {
        BaseData GetData();
        void AddConsumer(TConsumer consumer);
    }

    public interface iLevelProvider<TConsumer> : iProvider<TConsumer>
    {
        int GetLevel();
    }

    public interface iAttributeProvider : iLevelProvider<iAttributeConsumer> { }
    public interface iSkillProvider: iLevelProvider<iSkillConsumer>
    {
        int GetSkillPoints(AttributeData requestor);
    }

    public interface iBodyPartProvider : iLevelProvider<iBodyPartConsumer>
    {
        bool GetCrippled();

    }
}
