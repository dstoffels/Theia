using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Stats.IoC;
using UnityEditor;
using UnityEngine;


namespace Stats.Anatomy
{
    [CreateAssetMenu(menuName = "Anatomy/Body Part")]
    [InfoBox("Must have only Strength & Constitution in secondary attributes.")]
    public class BodyPartData : VitalData
    {
        public override int GetMax(Providers<int> providers) => base.GetMax(providers) / 2;
        public bool isEssential;
        public int vulnerability;
        public BodyPartData parent;

    }
}
