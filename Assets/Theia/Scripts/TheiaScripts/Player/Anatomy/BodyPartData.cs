using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Theia.IoC;
using UnityEngine;
using Theia.Stats.vitals;


namespace Theia.Stats.anatomy
{
    [CreateAssetMenu(menuName = "Anatomy/Body Part")]
    [InfoBox("Must have only Strength & Constitution in secondary attributes.")]
    public class BodyPartData : VitalData
    {
        public override int GetMax(IntProviders providers) => base.GetMax(providers) / 2;
        public bool critical;
        public int vulnerability;
        public BodyPartData parent;

        public override bool Contains(BaseData stat) => stat == parent || base.Contains(stat);
    }
}
