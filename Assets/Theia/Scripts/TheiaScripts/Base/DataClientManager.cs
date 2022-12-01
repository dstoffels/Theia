using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Theia
{
    public abstract class DataClientManager<TData, TClient> : SerializedMonoBehaviour
        where TClient : DataClient<TData>, new()
        where TData: BaseData
    {
        /// <summary>
        /// path is used to load all assets of type TData from the resources folder. 
        /// Empty by default, but can be overridden to explicitly define a folder to load from, in the event inherited
        /// assets are loaded unintentially.
        /// </summary>
        protected virtual string assetPath => "";

        public TClient this[TData data]
        {
            get
            {
                Init();
                return cache[data.name];
            }
        }

        public TClient this[string key]
        {
            get
            {
                Init();
                return cache[key];
            }
        }

        public TClient[] all
        {
            get
            {
                Init();
                TClient[] clients = new TClient[cache.Count];
                cache.Values.CopyTo(clients, 0);
                return clients;
            }
        }

        [ShowInInspector, DictionaryDrawerSettings(IsReadOnly = true, KeyLabel = "", ValueLabel = ""), HideLabel]
        private Dictionary<string, TClient> cache;

        private void Init()
        {
            if (cache == null)
            {
                cache = new Dictionary<string, TClient>();
                TData[] datas = Resources.LoadAll<TData>(assetPath);
                foreach (var data in datas)
                    InitCallback(data);
            }
        }

        protected virtual void InitCallback(TData data)
        {
            var client = new TClient();
            client.Init(data);
            cache.Add(data.name, client);
        }

        private void OnValidate()
        {
            Init();
        }
    }
}
