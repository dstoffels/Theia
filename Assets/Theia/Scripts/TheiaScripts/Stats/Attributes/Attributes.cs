using StatsOLD.Values;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace StatsOLD
{
    [RequireComponent(typeof(Skills)), DisallowMultipleComponent]
    public class Attributes : SerializedMonoBehaviour
    {
        [ReadOnly] public AttributeTemplate template; 
        [ReadOnly] public AttributeDict attributes = new AttributeDict();

        // Accessors //
        public int strength => attributes["Strength"].level;
        public int constitution => attributes["Strength"].level;
        public int agility => attributes["Agility"].level;
        public int dexterity => attributes["Dexterity"].level;
        public int acuity => attributes["Acuity"].level;
        public int intellect => attributes["Intellect"].level;
        public int discipline => attributes["Discipline"].level;
        public int ardor => attributes["Ardor"].level;

        public StartingLevels startingLevels;

        public void LoadAttributesFromTemplate()
        {
            var skills = GetComponent<Skills>();

            foreach (var data in template.attributeList)
                if(!attributes.ContainsKey(data.name))
                    attributes.Add(data.name, new Attribute(data, skills));
        }

        /*TESTING*/
        private void OnValidate()
        {
            LoadAttributesFromTemplate();
        }

        private void OnEnable()
        {
            SetStartingLevels();
        }

        [Button]
        void SetStartingLevels(int level = Global.Attribute.AVERAGE)
        {
            foreach (var att in attributes.Values)
                att.SetStartingLevel(level);
        }
    }

    // A unique dictionary for attributes, strictly for cleanliness/readability.
    public class AttributeDict : Dictionary<string, Attribute> { }
}