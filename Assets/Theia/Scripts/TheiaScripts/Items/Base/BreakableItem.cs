﻿using Sirenix.OdinInspector;
using Theia.Items.Resources;
using UnityEngine;

namespace Theia.Items.Base
{
    public abstract class BreakableItem<TData> : MaterialItem<TData>, iDamageable
        where TData: ItemData
    {
        [ShowInInspector]
        public float quality { get; set; }
        [ShowInInspector]
        public int condition { get; private set; }
        [ShowInInspector]
        public int durability => (int)(material.durability * quality);
        public bool broken => condition == 0;
        public void Damage(int amt) => condition = Mathf.Max(0, condition - amt);

        [Button]
        public void Init(float quality = 1)
        {
            base.Init();
            this.quality = quality;
            condition = durability;

        }
    }
}
