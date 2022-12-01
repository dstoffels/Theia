using Theia.Items.deprecated;

namespace Theia.Items.refactor
{
    public interface IWearableItemSlot
    {
        iItem RemoveItem(iItem item);
        iItem WearItem(iItem newItem);
    }
}
