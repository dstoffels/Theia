namespace Theia.Items.deprecated
{
    interface IContainer
    {
        bool StowItem(iItem newItem);
        iItem TakeItem(iItem item);
    }
}