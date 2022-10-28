using Items;

namespace InventoryStuff
{
    public interface IWearableItemSlot
    {
        IItem RemoveItem(IItem item);
        IItem WearItem(IItem newItem);
    }
}
