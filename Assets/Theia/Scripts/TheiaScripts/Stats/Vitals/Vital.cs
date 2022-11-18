using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Stats.Values;

namespace Stats
{
    /// <summary>
    /// Vitals include Stamina, Mana, Blood and Temperature, which derive from the Vital abstract class,
    /// </summary>
    [RequireComponent(typeof(Attributes))]
    public abstract class Vital : SerializedMonoBehaviour, IVital
    {
        protected float _current;
        [ShowInInspector]
        public virtual float level
        {
            get => _current;
            set { _current = Mathf.Clamp(value, min, max); StartRecovery(); }
        }
        public abstract float max { get; }
        public abstract float min { get; }
        protected abstract float threshold { get; }
        public abstract float debility { get; }

        /*RECOVERY*/
        protected bool isRecovering;
        protected abstract float pointsPerPulse { get; }

        public virtual void StartRecovery()
        {
            if (!isRecovering)
                StartCoroutine(Recover());
        }

        protected virtual IEnumerator Recover()
        {
            var pulse = new WaitForSecondsRealtime(Recovery.PULSE_TIME);
            isRecovering = true;

            while (level != max)
            {
                level += pointsPerPulse;
                yield return pulse;
            }

            isRecovering = false;
        }

        protected Attributes _att;
        protected Attributes att => _att ?? (_att = GetComponent<Attributes>());

        private void Start()
        {
            StartRecovery();
        }
    }
}