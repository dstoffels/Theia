using Theia.Stats.attributes;
using Theia.Stats.anatomy;

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

    public interface iWeightProvider
    {
        int GetWeight();
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

    public interface iArmorProvider : iProvider<iArmorConsumer>
    {
        int GetDamageReduction(BodyPartData bodypart);
        int GetHindrance();
    }
}
