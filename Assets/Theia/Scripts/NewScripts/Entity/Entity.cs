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
        public RaceData race;
        public Attributes attributes;
        public Skills skills;
        public Entity() { }

        [Button]
        private void Init()
        {
            attributes = GetComponent<Attributes>();
            attributes.Init();

            skills = GetComponent<Skills>();
            skills.Init();
        }

        private void OnValidate()
        {
            Init();
        }
    }

    
}
