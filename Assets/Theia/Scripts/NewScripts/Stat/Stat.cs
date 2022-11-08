using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


namespace Stats
{

    public abstract class Stat<Data> : IStat where Data : StatData
    {
        public Data data;
        protected List<IStatObserver> observers = new List<IStatObserver>();
        public int level { get; protected set; }
        public string name => data.name;
        public string description => data.description;
        public Stat(Data data)
        {
            this.data = data;
        }
    }

    public abstract class StatData : SerializedScriptableObject
    {
        [TextArea(3, 10), PropertyOrder(999)]
        public string description;
    }

}