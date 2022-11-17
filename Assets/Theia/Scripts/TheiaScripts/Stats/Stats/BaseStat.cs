using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    public abstract class BaseStat<Data> where Data : BaseData
    {
        /// <summary>
        /// Scriptable object with all stat data that is "plugged in" to the stat.
        /// </summary>
        [HideInInspector]
        public Data data;
        public string name => data.name;
        public string description => data.description;
        public virtual void Init(Data data) => this.data = data;
    }

    public interface iStat
    {
        string name { get; }
        int level { get; }
        void Update();
    }

    public interface iStatClient
    {
        void AddService(iStat service);
    }

    public interface iSkillClient
    {
        void AddService(iStat service, bool isSecondary);
    }

    public class StatList : List<iStat>
    {
        public int total
        {
            get
            {
                int total = 0;
                foreach (var stat in this) total += stat.level;
                return total;
            }
        }
    }
}
