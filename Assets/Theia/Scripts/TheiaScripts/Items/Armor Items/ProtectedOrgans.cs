using System.Collections.Generic;
using Items.Armor;
using Stats;

namespace InventoryStuff.Armor
{
    /// <summary>
    /// A dynamic dictionary of organs and the total amount of protection for them, 
    /// based on armor the player is currently wearing.
    /// <para>Used as a property that's accessed by individual organs in the player's anatomy using its Get() method.</para>
    /// </summary>
    public class ProtectedOrgans : Dictionary<OrganData, Protection>
    {
        public static ProtectedOrgans Get(ArmorSlots armorSlots)
        {
            var dict = new ProtectedOrgans();
            foreach (var slot in armorSlots.Values)
            {
                BuildDict(dict, slot.padded);
                BuildDict(dict, slot.maille);
                BuildDict(dict, slot.heavy);
            }
            return dict;
        }

        static void BuildDict(ProtectedOrgans dict, IArmorItem armor)
        {
            if(armor != null)
            {
                foreach (var organ in armor.coverage)
                {
                    if (dict.ContainsKey(organ)) dict[organ].AddProtection(armor.protection);
                    if (!dict.ContainsKey(organ)) dict.Add(organ, new Protection(armor.protection));
                    if (armor.isImmuneToSoftSpots) dict[organ].AddFullProtection(armor.protection);
                }
            }
        }
    }

    public class Protection
    {
        public float protection;
        public float fullProtection;

        public void AddProtection(float amt) => protection += amt;
        public void AddFullProtection(float amt) => fullProtection += amt;

        public Protection(float armorProtection)
        {
            protection = armorProtection;
        }
    }
}