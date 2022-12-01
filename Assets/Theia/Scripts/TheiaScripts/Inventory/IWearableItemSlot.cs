using Items;

namespace InventoryStuff
{
    public interface IWearableItemSlot
    {
        iItem RemoveItem(iItem item);
        iItem WearItem(iItem newItem);
    }
}
