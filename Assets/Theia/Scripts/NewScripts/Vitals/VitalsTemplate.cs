using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    [CreateAssetMenu(menuName = "Vitals Template")]
    public class VitalsTemplate : StatsTemplate<VitalData> { }
}