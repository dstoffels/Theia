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
        protected List<string> keys = new List<string>();
        public List<string> Keys => keys;

        public bool ContainsKey(string key) => keys.Contains(key);

    }
}
