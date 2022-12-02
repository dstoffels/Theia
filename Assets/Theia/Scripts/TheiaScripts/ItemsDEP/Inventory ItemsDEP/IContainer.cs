namespace Theia.Items.Base
{
    interface IContainer
    {
        bool StowItem(iItem newItem);
        iItem TakeItem(iItem item);
    }
}