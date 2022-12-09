namespace Theia.Items.Base
{
    public interface iContainer : iItem
    {
        iItem StowItem(iItem newItem);
        iItem RemoveItem(iItem item);
    }
}