using Sirenix.OdinInspector;
using Stats;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Theia.Items.Armor
{
    [CreateAssetMenu(menuName = "Armor/Type")]
    public class ArmorType : BaseData
    {

        private static List<ArmorType> _cache;
        [ShowInInspector]
        public static List<ArmorType> all
        {
            get
            {
                if (_cache is null)
                {
                    ArmorType[] types = UnityEngine.Resources.LoadAll<ArmorType>("");
                    _cache = types.ToList();
                }
                return _cache;
            }
        }
    }
}
