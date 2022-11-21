using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using InventoryStuff.Armor;
using StatsOLD;


namespace Stats
{
    // TODO: need to sort out methods for randomly selecting organs, randomly selecting upper/lower organs and ensuring that if a crippled organ is selected it will rerun the method or select the next in line.
    [RequireComponent(typeof(Attributes), typeof(HitPoints), typeof(Armor))]
    public class Anatomy : SerializedMonoBehaviour
    {
        [PropertyOrder(-2)]
        public AnatomyTemplate template;

        [DictionaryDrawerSettings(IsReadOnly = true)]
        public Organs organs = new Organs();

        // Impairment is the total debilitation across all Organs, subtracted from any combat abilities when performed.
        [ShowInInspector]
        public float impairment => GetDebilityFromOrgans(); 

        [ShowInInspector]
        public float totalBloodLoss => GetBloodLossFromOrgans();


        [Button, PropertyOrder(-1)]
        public void LoadAnatomyTemplate()
        {
            var attributes = GetComponent<Attributes>();
            var hp = GetComponent<HitPoints>();
            var armor = GetComponent<Armor>();

            foreach (var organData in template.organList)
            {
                if(!organs.ContainsKey(organData))
                    organs.Add(organData, new Organ(organData, this, hp, armor));
            }
        }

        private float GetDebilityFromOrgans()
        {
            var total = 0f;
            foreach (var organ in organs.Values)
                total += organ.debility;
            return total;
        }

        private float GetBloodLossFromOrgans()
        {
            var total = 0f;
            foreach (var organ in organs.Values)
                total += organ.bloodLossPerPulse;
            return total;
        }

        /*COMPONENT REFERENCES*/
        Blood _blood;
        Blood blood => _blood ?? (_blood = GetComponent<Blood>());

        void OnValidate()
        {
            LoadAnatomyTemplate();
        }
    }

    // unique dictionary for Anatomy
    public class Organs : Dictionary<OrganData, Organ> { }
}