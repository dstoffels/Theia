using UnityEngine;

namespace Stats
{
    [CreateAssetMenu(menuName = "Skill")]
    public class SkillData : StatData
    {
        public AttributeData primaryAttribute;
        public AttributeData secondaryAttribute;
    }
}
