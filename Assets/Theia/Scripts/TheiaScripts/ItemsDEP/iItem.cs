using Theia.Items.refactor;
using UnityEngine;

namespace Theia.Items.deprecated
{
    public interface iItem
    {
        Player owner { get; } // fixme: will have to eventually be replaced with 'entity' or an entity interface.
        IWearableItemSlot currentSlot { get; set; }
        int width { get; }
        int height { get; }
        int weight { get; }
        float volume { get; }
        void PickUp(Player player);
        void Drop();
        void Stow(Player player);
    }

    public interface IWearableItem : iItem
    {
        void Wear(Player player);
        void Remove();
    }
}