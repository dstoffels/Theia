using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    // Network behaviours can't utilize generics, so each stat collection component
    // (attributes, skills, vitals) must manually implement "dictionary" functionality :(
    [RequireComponent(typeof(Attributes))]
    public class Skills : StatManager
    {
        [InfoBox("Skill Data added here will automatically generate the entity's attributes.")]
        public List<SkillData> data = new List<SkillData>();
        public Skill this[string key]
        {
            get { return skills[keys.IndexOf(key)]; }
            set { skills[keys.IndexOf(key)] = value; }
        }


        [ShowInInspector]
        List<Skill> skills = new List<Skill>();
        public List<Skill> Values => skills;

        public void Add(string key, Skill value)
        {
            if (!ContainsKey(key))
            {
                keys.Add(key);
                skills.Add(value);
            }
        }

        public void Add(KeyValuePair<string, Skill> item)
        {
            if (!ContainsKey(item.Key))
            {
                keys.Add(item.Key);
                skills.Add(item.Value);
            }
        }

        public void Clear()
        {
            keys.Clear();
            skills.Clear();
        }

        public bool Contains(KeyValuePair<string, Skill> item) => keys.Contains(item.Key) && skills.Contains(item.Value);

        public void Init()
        {
            Clear();
            foreach (var item in data) Add(item.name, new Skill(item));
            Attributes attributes = GetComponent<Attributes>();
            foreach (var att in attributes.Values) 
                foreach (var skill in Values) 
                    if (att.data == skill.data.primaryAttribute || att.data == skill.data.secondaryAttribute)
                    {
                        att.Attach(skill);
                        skill.Attach(att);
                        att.NotifyDependents();
                        skill.NotifyDependents();
                    }
        }

        private void OnValidate()
        {
            if (skills.Count == 0) Init();
        }

        [Button]
        void ResetValues()
        {
            foreach (var skill in skills) skill.AddXP(-2000000000);

        }
    }
}
