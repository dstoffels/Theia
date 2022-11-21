using Sirenix.OdinInspector;
using UnityEngine;
using Stats.Values;
using static UnityEngine.Mathf;
using System.Collections;
using InventoryStuff.Armor;
using StatsOLD;

namespace Stats
{
    [HideReferenceObjectPicker]
    [RequireComponent(typeof(HitPoints), typeof(Anatomy), typeof(Armor))]
    public class Organ
    {
        private float _condition;
        [ShowInInspector]
        public float condition
        {
            get { return _condition; }
            set 
            { 
                _condition = Clamp(value, hp.min, hp.max);
                StartRecovery();
            }
        }

        [ShowInInspector]
        public float debility => Max(0, hp.debilityThreshold - condition);


        [ShowInInspector] public float protection => GetArmorProtection();
        [ShowInInspector] public float fullProtection => GetArmorProtection(true);

        private float GetArmorProtection(bool fullProtection = false)
        {
            if (armor.protectedOrgans.ContainsKey(data))
            {
                var protectedOrgan = armor.protectedOrgans[data];
                return fullProtection ? protectedOrgan.fullProtection : protectedOrgan.protection;
            }
            else return 0;
        }

        public void ReduceHP(float amount) => condition -= amount;

        /*STATE*/
        private bool isBurned; // damage w/out bleeding. fixme: will this one bool represent hot & cold?

        public bool parentIsCrippled { get; set; }

        public bool isCrippled => condition == hp.min || parentIsCrippled;

        /*BLOOD LOSS*/
        public float bloodLossPerPulse => GetBloodLoss();

        private float GetBloodLoss()
        {
            // Percentage of maximum hit points at which bleeding begins if the current hit point value drops below this threshold. 
            float bleedThreshold = hp.max * Global.Organ.BLEED_THR_MULTIPLIER;

            // Percentage of the maximum possbile blood loss based on how far below the threshold the current hit point value is.
            float severity = Min(0, (condition - bleedThreshold) / bleedThreshold);

            float maxbloodLossPerPulse = data.maxBloodLossPerMin / 60f * Recovery.PULSE_TIME;

            return maxbloodLossPerPulse * severity;
        }

        /*RECOVERY*/
        public void StartRecovery()
        {
            if (!isRecovering)
                hp.StartCoroutine(Recover());
        }

        private bool isRecovering;

        IEnumerator Recover()
        {
            var pulse = new WaitForSecondsRealtime(Recovery.PULSE_TIME);

            isRecovering = true;
            CrippleDependents();

            while (!isCrippled && condition < hp.max)
            {
                condition += hp.recoveredPerPulse;
                yield return pulse;
            }
            isRecovering = false;
            CrippleDependents();
        }

        void CrippleDependents()
        {
            if(data.dependents != null)
            {
                foreach (var organ in data.dependents)
                {
                    if (isCrippled)
                        anatomy.organs[organ].parentIsCrippled = true;
                    else
                    {
                        anatomy.organs[organ].parentIsCrippled = false;
                        anatomy.organs[organ].StartRecovery();
                    }
                }
            }
        }

        [HideInInspector] public OrganData data;

        /*COMPONENT REFERENCES*/
        [HideInInspector] public Anatomy anatomy;
        [HideInInspector] public HitPoints hp;
        [HideInInspector] public Armor armor;

        public Organ(OrganData data, Anatomy anatomy, HitPoints hp, Armor armor)
        {
            this.data = data;
            this.anatomy = anatomy;
            this.hp = hp;
            this.armor = armor;
        }
    }
}