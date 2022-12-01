using Sirenix.OdinInspector;
using Stats.Values;
using UnityEngine;
using Stats;

namespace StatsOLD
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Attributes))]
    public class Celerity : SerializedMonoBehaviour
    {
        [ShowInInspector]
        public float multiplier => 1 + (agilityModifier + debilitation) / 50;

        const float ardorMod = 4f;
        float ardorOffset => att.ardor / ardorMod; // ardor offsets debilitation

        float agilityModifier => att.agility - Global.Attribute.AVERAGE; // each agility point = 1%
        float debilitation => Mathf.Min(0, temp.level + ardorOffset); // debilitation is a penalty derived from negative temperature (cold)


        Temperature _temp;
        Temperature temp => _temp ?? (_temp = GetComponent<Temperature>());

        Attributes _att;
        Attributes att => _att ?? (_att = GetComponent<Attributes>());
    }
}