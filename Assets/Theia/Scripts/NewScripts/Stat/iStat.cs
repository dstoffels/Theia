using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Entities;

namespace Stats
{
    public interface IStatSubject
    {
        public void Attach(IStatObserver observer);
        public void Detach(IStatObserver observer);
        public void NotifyDependents();

    }

    public interface IStat
    {
        public string name { get; }
        public string description { get; }
        public int level { get; }
        //public float progress { get; }

    }

    public interface IStatObserver
    {
        public string name { get; }
        public void Update(StatEvent statEvent);
    }

    public struct StatEvent
    {
        public string name;
        public int value;
        public StatEvent(string statName, int value)
        {
            name = statName;
            this.value = value;
        }
    }

}