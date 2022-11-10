using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;


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
        public static T[] GetAllScriptableObjects<T>() where T : BaseData
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);

            T[] array = new T[guids.Length];

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                array[i] = AssetDatabase.LoadAssetAtPath<T>(path);
            }

            return array;
        }
    }
}
