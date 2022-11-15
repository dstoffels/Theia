using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    [CreateAssetMenu(menuName = "Skill")]
    public class SkillData : BaseData
    {
        [ShowInInspector]
        public static int FIRST_LEVEL_AT;
        public AttributeData primaryAttribute;
        public AttributeData secondaryAttribute;
    }
}
