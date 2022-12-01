namespace Items
{
    interface IContainer
    {
        bool StowItem(iItem newItem);
        iItem TakeItem(iItem item);
    }
}