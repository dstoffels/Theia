using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


namespace Stats
{
    [CreateAssetMenu(menuName = "Vital")]
    public class VitalData : BaseData
    {
        [TitleGroup("Globals"), ShowInInspector]
        public static float RECOVERY_PULSE = 0.033f;
        [ShowInInspector]
        public static float POINTS_RECOVERED_PER_SEC = 2f;
        [ShowInInspector]
        public float avgVitalPtsPerPulse => POINTS_RECOVERED_PER_SEC * RECOVERY_PULSE;

        [TitleGroup("Attributes")]
        public AttributeData primaryAttribute;
        [ListDrawerSettings(Expanded =true)]
        public List<AttributeData> secondaryAttributes;
        public AttributeData recoveryAttribute;

        [TitleGroup("Info")]
        public bool isFullScale;
        public bool isEquilibrium;
    }
}
