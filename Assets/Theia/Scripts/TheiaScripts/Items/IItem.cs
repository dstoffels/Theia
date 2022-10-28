using InventoryStuff;
using UnityEngine;

namespace Items
{
    public interface IItem
    {
        Player owner { get; } // fixme: will have to eventually be replaced with 'entity' or an entity interface.
        IWearableItemSlot currentSlot { get; set; }
        int width { get; }
        int height { get; }
        float weight { get; }
        float volume { get; }
        void PickUp(Player player);
        void Drop();
        void Stow(Player player);
    }

    public interface IWearableItem : IItem
    {
        void Wear(Player player);
        void Remove();
    }
}