using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Stats.Anatomy
{
    [HideReferenceObjectPicker]
    public class BodyPart : VitalBase<BodyPartData>
    {
        [Button]
        public void Damage(int amt = 5)
        {
            level -= amt;
            isRecovering = level >= 0;
        }

        [ShowInInspector]
        public bool crippled => level == min;
    }
}
