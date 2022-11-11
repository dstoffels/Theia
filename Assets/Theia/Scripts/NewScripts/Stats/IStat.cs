using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Entities;

namespace Stats
{
    public interface IStat
    {
        public string name { get; }
        public string description { get; }
        public int level { get; }
        //public float progress { get; }

    }
    public interface IStatSubject
    {
        public string name { get; }
        public void Attach(IStatObserver observer);
        public void Detach(IStatObserver observer);
        public void NotifyDependents();

    }


    public interface IStatObserver
    {
        public string name { get; }
        public void Update(StatValue statValue);
    }

    

    public struct StatValue
    {
        public string name;
        public int value;
        public StatValue(string statName, int value)
        {
            name = statName;
            this.value = value;
        }
    }

    public struct StatValues
    {
        public int this[string key] => statValues[key];
        public int this[BaseData key] => statValues[key.name];

        private Dictionary<string, int> statValues;
        private void Init()
        {
            if (statValues is null) statValues = new Dictionary<string, int>();
        }

        public int Total
        {
            get
            {
                Init();
                int total = 0;
                foreach (var value in statValues.Values) total += value;
                return total;
            }
        }

        public void Set(StatValue statValue)
        {
            Init();
            if (!statValues.ContainsKey(statValue.name)) statValues.Add(statValue.name, statValue.value);
            else statValues[statValue.name] = statValue.value;
        }
    }

}