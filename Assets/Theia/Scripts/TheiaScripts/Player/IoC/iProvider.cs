using Theia.Stats.attributes;

namespace Theia.IoC
{
    public interface iData
    {
        BaseData GetData();
    }
    public interface iProvider<TConsumer> : iData
    {
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
