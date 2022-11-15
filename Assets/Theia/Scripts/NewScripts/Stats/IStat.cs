using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Entities;

namespace Stats
{

    public interface INameable
    {
        public string name { get; }
    }
    public interface IStat : INameable
    {
        public string description { get; }
        public int level { get; }
    }
    public interface IStatSubject : INameable
    {
        public void Attach(IStatObserver observer);
        public void Detach(IStatObserver observer);
        public void NotifyDependents();

    }


    public interface IStatObserver : INameable
    {
        public void Update(StatValue statValue);
    }

    /// <summary>
    /// An immutable key-value pair (string-float) for holding stat values . Used as argument when updating IStatObservers.
    /// </summary>
    public struct StatValue
    {
        public string name { get; private set; }
        public int value { get; private set; }
        
        public StatValue(string name, int value)
        {
            this.name = name;
            this.value = value;
        }
    }

    /// <summary>
    /// A string/int dictionary for StatMangers to track the values of other stats.
    /// </summary>
    public class StatValues : Dictionary<string, int>
    {
        public int this[BaseData key] => this[key.name];
        public int Total
        {
            get
            {
                int total = 0;
                foreach (var value in Values) total += value;
                return total;
            }
        }

        public void Add(StatValue stat)
        {
            try { this[stat.name] = stat.value; }
            catch { Add(stat.name, stat.value); }
        }
    }
}