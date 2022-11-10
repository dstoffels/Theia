using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Stats;
using Sirenix.OdinInspector;

namespace Entities
{
    public class Entity : NetworkBehaviour
    {
        public Entity() { }

        [Button]
        private void Init()
        {
            Attributes attributes = GetComponent<Attributes>();
            Skills skills = GetComponent<Skills>();
            Debug.Log(skills["Unarmed"].level);
        }


    }

    
}
