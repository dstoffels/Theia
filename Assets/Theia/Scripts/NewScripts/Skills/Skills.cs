using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    [RequireComponent(typeof(Attributes))]
    public class Skills : StatManager<Skill, SkillData>
    {
        public override void Init()
        {
            base.Init();
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

        protected override void GenerateStatFromData(SkillData data) => Add(data.name, new Skill(data));

        [Button]
        void ResetValues()
        {
            foreach (var skill in values) skill.AddXP(-2000000000);

        }
    }
}
