using Sirenix.OdinInspector;
using Stats;
using System.Collections.Generic;
using UnityEngine;

namespace Abilities
{
    public abstract class Ability : SerializedScriptableObject
    {
        [Tooltip("The required skill level to perform this ability without penalty."), Title("Ability")]
        public float difficulty;
        public float staminaCost;
        public float manaCost;
        public Ability replaces;
        public Sprite image;

        [TextArea(2, 10), PropertyOrder(999),Title("Tooltip"),HideLabel]
        public string tooltip;
    }
}