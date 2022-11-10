using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Stats
{
    // Network behaviours can't utilize generics, so each stat collection component
    // (attributes, skills, vitals) must manually implement "dictionary" functionality :(
    // Only a handful of members can be inherited. Sorry for the mess! 
    public abstract class StatManager : Mirror.NetworkBehaviour
    {
        public StatsTemplate<BaseData> statsTemplate;
        protected List<string> keys = new List<string>();
        public List<string> Keys => keys;

        public bool ContainsKey(string key) => keys.Contains(key);

        //public T[] GetAllScriptableObjects<T>() where T : BaseData
        //{
        //    string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);

        //    T[] array = new T[guids.Length];

        //    for (int i = 0; i < guids.Length; i++)
        //    {
        //        string path = AssetDatabase.GUIDToAssetPath(guids[i]);

        //        array[i] = AssetDatabase.LoadAssetAtPath<T>(path);
        //    }

        //    return array;
        //}

    }
}
