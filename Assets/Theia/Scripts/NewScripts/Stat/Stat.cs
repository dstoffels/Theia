using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Entities;

namespace Stats {

    public abstract class Stat : IStat
    {
        public IStatData data;
        public int Level { get; protected set; }
        public abstract void Update(StatEvent statEvent);
        protected abstract bool NeedsUpdate();
    }

}