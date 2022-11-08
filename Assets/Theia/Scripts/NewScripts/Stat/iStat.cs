using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Entities;

namespace Stats
{
    public interface IObserver
    {
        public void Update(StatEvent statEvent);

    }

    public interface IUserInterface
    {
        public void UpdateUI();
    }

    public interface IStatData
    {
        public int id { get; }
        public string name { get; }
        public string description { get; }
    }

    public interface IStat : IObserver
    {
        public int Level { get; }
        //public float progress { get; }

    }

    public struct StatEvent
    {
        public string name;
        public int value;
        public StatEvent(string statName, int newStatValue)
        {
            name = statName;
            value = newStatValue;
        }
    }

}