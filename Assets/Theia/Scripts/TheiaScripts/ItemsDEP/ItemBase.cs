using UnityEngine;
using Sirenix.OdinInspector;
using Theia.Items.Base;
using UnityEngine.Events;
using Theia.Stats.gear;

//namespace Theia.Items.Base
//{
//    public abstract class ItemBase<TMaterial, TItemData> : SerializedMonoBehaviour, iItem
//        where TItemData : ItemData
//        where TMaterial : Mats.MaterialData
//    {
//        public TMaterial material;
//        public TItemData data;

//        public int width => data.size.width;
//        public int height => data.size.height;
//        public float volume => data.size.volume;
//        [ShowInInspector]
//        public int weight => (int)(data.baseWeight * material.density);

//        public Player owner => transform.parent.GetComponent<Player>(); // fixme: will eventually need to change PlayerTEMP to 'Entity' or an entity interface.
//        public bool currentlyOwned => owner != null;

//        [ShowInInspector]
//        public iWearableItemSlot currentSlot { get; set; }

//        int iItem.volume => throw new System.NotImplementedException();

//        public void Craft(TItemData item, TMaterial mat)
//        {
//            material = mat;
//            data = item;
//            name = mat.name + " " + item.name;
//        }

//        [Button]
//        public void Drop()
//        {
//            if(currentSlot != null)
//                currentSlot.Remove(this);
//            transform.SetParent(null);
//            GetComponent<MeshRenderer>().enabled = true;
//            GetComponent<BoxCollider>().enabled = true;
//        }

//        [Button]
//        public void PickUp(Player player)
//        {
//            //player.gear.GrabItem(this);
//            transform.SetParent(player.transform);
//            GetComponent<MeshRenderer>().enabled = false;
//            GetComponent<BoxCollider>().enabled = false;
//            transform.localPosition = Vector3.zero;
//        }

//        public void Stow(Player player)
//        {
//            throw new System.NotImplementedException();
//        }
//    }
//}
