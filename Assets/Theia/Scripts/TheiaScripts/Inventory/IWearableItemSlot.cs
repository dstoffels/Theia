using Theia.Items.Base;
namespace Theia.Stats.gear
{
    public interface iWearableItemSlot<TItem>
        where TItem : iItem
    {
        TItem Equip(TItem item);
        TItem Remove(TItem item);
    }
}
