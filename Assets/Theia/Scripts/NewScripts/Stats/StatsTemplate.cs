using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


namespace Stats
{
    public class StatsTemplate<TData> : SerializedScriptableObject where TData : BaseData
    {
        [ListDrawerSettings(Expanded = true)]
        public List<TData> data = new List<TData>();
    }
}
