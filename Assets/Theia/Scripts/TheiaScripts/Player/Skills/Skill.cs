using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Theia.IoC;
using Theia.Stats.attributes;

namespace Theia.Stats.skills
{
    public interface ISkillBuff { } // fixme: sort out stat buffs, look at uMMORPG methods

    [HideReferenceObjectPicker]
    public class Skill : DataClient<SkillData>, iSkillProvider, iAttributeConsumer
    {
        [ShowInInspector, ReadOnly]
        public int level { get; private set; }

        private void SetLevel()
        {
            int newLevel = aptitude + proficiency;
            if (level != newLevel)
            {
                level= newLevel;
                consumers.Notify(this);
            }
        }
        

        // Aptitude is derived from a skill's parent attributes (primary & secondary). 
        public int aptitude { get; private set; }

        private void SetAptitude()
        {
            aptitude = attributes.Reduce(att =>
                data.primaryAttribute == att.Key ?
                    att.Value :
                data.secondaryAttribute == att.Key ?
                    att.Value / 2 :
                0
            );
            SetLevel();
        }

        // Players earn xp in a skill by using constituent abilities, leveling up the skill's proficiency at exponential increments.
        //[ShowInInspector, ReadOnly]
        public int xp { get; private set; }

        [Button(Style = ButtonStyle.FoldoutButton)]
        public void AddXp(int amount=5000)
        {
            xp = Mathf.Max(0, xp + amount);

            SetProficiency();
        }

        public int proficiency { get; private set; }

        private static readonly float FIRST_BONUS_AT = 1000;
        private static readonly float BONUS_MODIFIER = 1.02f;
        private float nextBonusAt, xpRequired = FIRST_BONUS_AT;
        private float lastBonusAt = 0;

        private bool needsUpdate => xp >= nextBonusAt || xp < lastBonusAt;

        private void SetProficiency()
        {
            if (needsUpdate)
            {
                lastBonusAt = proficiency = 0;
                nextBonusAt = xpRequired = FIRST_BONUS_AT;

                while (needsUpdate)
                {
                    lastBonusAt = nextBonusAt;
                    xpRequired *= BONUS_MODIFIER;
                    nextBonusAt += xpRequired;
                    proficiency++;
                }
                SetLevel();
            }
        }

        //CONSUMER INTERFACE
        private IntProviders attributes = new IntProviders();
        public void Subscribe(iAttributeProvider provider)
        {
            if (data.Contains(provider.GetData()))
            {
                provider.AddConsumer(this);
                Notify(provider);
            }
        }

        public void Notify(iAttributeProvider provider)
        {
            attributes.Update(provider.GetData(), provider.GetLevel());
            SetAptitude();
        }



        // PROVIDER INTERFACE
        private SkillConsumers consumers = new SkillConsumers();
        public void AddConsumer(iSkillConsumer consumer) => consumers.Add(consumer);

        public BaseData GetData() => data;

        public int GetSkillPoints(AttributeData requestor) =>
            data.primaryAttribute == requestor ?
                proficiency * 2 :
            data.secondaryAttribute == requestor ?
                proficiency :
            0;

        public int GetLevel() => level;
    }
}