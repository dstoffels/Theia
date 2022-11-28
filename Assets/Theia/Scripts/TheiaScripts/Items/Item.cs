using UnityEngine;
using Sirenix.OdinInspector;
using InventoryStuff;
using UnityEngine.Events;

namespace Items
{
    public abstract class Item<Material, ItemData> : SerializedMonoBehaviour, IItem where ItemData : Items.ItemData
        where Material : Mats.Mat
    {
        public Material material;
        public ItemData data;

        public int width => data.size.width;
        public int height => data.size.height;
        public float volume => data.size.volume;
        [ShowInInspector]
        public float weight => data.baseWeight * material.density;

        public Player owner => transform.parent.GetComponent<Player>(); // fixme: will eventually need to change PlayerTEMP to 'Entity' or an entity interface.
        public bool currentlyOwned => owner != null;

        [ShowInInspector]
        public IWearableItemSlot currentSlot { get; set; }


        public void Craft(ItemData item, Material mat)
        {
            material = mat;
            data = item;
            name = mat.name + " " + item.name;
        }

        [Button]
        public void Drop()
        {
            if(currentSlot != null)
                currentSlot.RemoveItem(this);
            transform.SetParent(null);
            GetComponent<MeshRenderer>().enabled = true;
            GetComponent<BoxCollider>().enabled = true;
        }

        [Button]
        public void PickUp(Player player)
        {
            //player.gear.GrabItem(this);
            transform.SetParent(player.transform);
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<BoxCollider>().enabled = false;
            transform.localPosition = Vector3.zero;
        }

        public void Stow(Player player)
        {
            throw new System.NotImplementedException();
        }
    }
}
