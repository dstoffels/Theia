using Sirenix.OdinInspector;
using UnityEngine;

namespace Theia
{
    public abstract class DataClientBehaviour<TData> : MonoBehaviour
        where TData : BaseData
    {
        /// <summary>
        /// StatData Scriptable object that is "plugged in" to the stat to provide le data.
        /// </summary>
        public TData data;
        public string description => data.description;
        public virtual void Init()
        {
            name = data.name;
        }
    }
}
