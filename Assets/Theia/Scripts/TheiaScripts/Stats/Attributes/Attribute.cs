using Sirenix.OdinInspector;
using Stats.IoC;

namespace Stats
{
    // TODO: Implement interfaces to decouple
    public interface iStatBuff { } // fixme: sort out stat buffs, look at uMMORPG methods

    [HideReferenceObjectPicker]
    public class Attribute : BaseStat<AttributeData>, iConsumer<int>, iProvider<int>
    {
        [ShowInInspector, ReadOnly]
        public int level { get; private set; }
        private void SetLevel()
        {
            int newLevel = startingLevel + raceModifier + skillBonus;
            if (level != newLevel)
            {
                level = newLevel;
                consumers.Notify(this);
            }
        }



        //starting level is intialized at character creation, customized manually and modified by species
        public int startingLevel { get; private set; } = 10;
            
        public int raceModifier { get; private set; } = 0;

        // The skill bonus is how an attribute grows. Skill levels are derived from two attributes each (skill base).
        // A skill's parent attributes earn skill points each time it levels up (20 skill points per level)
        public int skillBonus { get; private set; }

        private static readonly int FIRST_BONUS_AT = 10;
        private int nextBonusAt = FIRST_BONUS_AT;
        private int lastBonusAt = 0;

        public int skillPoints { get; private set; }
        private void SetSkillPoints()
        {
            skillPoints= 0;
            foreach (var skillVal in providers.Values)
                skillPoints += skillVal;
            SetSkillBonus();
        }

        private void SetSkillBonus()
        {
            if (needsUpdate)
            {
                skillBonus = 0;
                nextBonusAt = FIRST_BONUS_AT;
                lastBonusAt = 0;

                while (needsUpdate)
                {
                    lastBonusAt = nextBonusAt;
                    nextBonusAt += FIRST_BONUS_AT;
                    skillBonus++;
                }
            }
            SetLevel();
        }
        private bool needsUpdate => skillPoints >= nextBonusAt || skillPoints < lastBonusAt;


        //public void Load(int startingLevel, int raceModifier)
        //{
        //    this.startingLevel = startingLevel;
        //    this.raceModifier = raceModifier;
        //    SetLevel();
        //}


        // CONSUMER INTERFACE
        private Providers<int> providers = new Providers<int>();
        public void Subscribe(iProvider<int> provider)
        {
            if (data.Contains(provider.GetData()))
            {
                provider.AddConsumer(this);
                Update(provider);
            }
        }

        public void Update(iProvider<int> provider)
        {
            providers.Update(provider, this);
            SetSkillPoints();
        }

        // PROVIDER INTERFACE
        private Consumers<int> consumers = new Consumers<int>();
        public int GetValue(iConsumer<int> consumer) => level;
        public void AddConsumer(iConsumer<int> consumer) => consumers.Add(consumer);
        public BaseData GetData() => data;
    }

}