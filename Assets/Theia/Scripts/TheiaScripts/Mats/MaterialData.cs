using Sirenix.OdinInspector;

namespace Mats
{
    // Defined as "Mat" to avoid ambiguity with Unity's "Material" class. Mats scriptable objects store modifying stats,
    // which are combined with Item Data on Item gameobjects to yield an item's stats. This allows for a wide variety of
    // crafting possibilities without forcing us to manually create fixed item prefabs for each possibility.
    public abstract class MaterialData : SerializedScriptableObject
    {
        public float density;
        public float hindranceMultiplier;
        public float damageMultiplier;
        public float durability;
        public float conductivity;
    }
}