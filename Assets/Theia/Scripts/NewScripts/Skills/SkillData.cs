using UnityEngine;

namespace Stats
{
    [CreateAssetMenu(menuName = "Skill")]
    public class SkillData : BaseData
    {
        public AttributeData primaryAttribute;
        public AttributeData secondaryAttribute;
    }
}
