using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace Stats
{
    public class Attributes : StatManager<Attribute, AttributeData>
    {
        protected override void GenerateStatFromData(AttributeData data) => Add(data.name, new Attribute(data));

        public void Init(StatValues attValues, RaceData race)
        {
            // initializer for loading character data
        }


    }


}
