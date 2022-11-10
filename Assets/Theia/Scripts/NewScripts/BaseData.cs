using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


namespace Stats
{
    public abstract class BaseData : SerializedScriptableObject
    {
        [TextArea(3, 20), PropertyOrder(999)]
        public string description;

        /// <summary>
        /// Returns an array of all BaseData assets in the project of a specified type.
        /// </summary>
        /// <typeparam name="T"></typeparam>

    }
}
