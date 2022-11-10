using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


namespace Stats
{
    [Serializable]
    public abstract class Stat<Data> : IStat where Data : BaseData
    {
        [ReadOnly]
        public Data data;
        protected List<IStatObserver> observers = new List<IStatObserver>();
        public string name => data.name;
        public string description => data.description;
        [ShowInInspector, ReadOnly]
        public int level { get; protected set; }
        public Stat(Data data)
        {
            this.data = data;
        }
    }
}