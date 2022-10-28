using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    [RequireComponent(typeof(Attributes)), DisallowMultipleComponent]
    public class Skills : SerializedMonoBehaviour
    {
        public SkillsTemplate template;

        [DictionaryDrawerSettings(IsReadOnly = true)]
        public Skillset skillset = new Skillset();

        // Used for initializing an entity's skillset with a predefined template. This is useful for creating custom skillsets
        // for different monsters.
        [Button]
        private void LoadSkillTemplate()
        {
            var attributes = GetComponent<Attributes>();

            foreach (var skill in template.skillsList)
                if (!skillset.ContainsKey(skill))
                    skillset.Add(skill, new Skill(skill, attributes));

            // remove any skills in the skillset that aren't in the template.
            var trash = new List<SkillData>();

            foreach (var skill in skillset.Keys)
                if (!template.skillsList.Contains(skill))
                    trash.Add(skill);

            foreach (var skill in trash)
                skillset.Remove(skill);
        }

        public void SetSkillBases()
        {
            foreach (var skill in skillset.Values)
                skill.SetSkillBase();
        }

        [Button(Style = ButtonStyle.Box)]
        private void AddXP(int xp, SkillData skill) => skillset[skill].AddXp(xp);

        private void OnValidate()
        {
            LoadSkillTemplate();
        }
    }

    // unique dictionary for skills, strictly for cleanliness/readability.
    public class Skillset : Dictionary<SkillData, Skill> { }
}
