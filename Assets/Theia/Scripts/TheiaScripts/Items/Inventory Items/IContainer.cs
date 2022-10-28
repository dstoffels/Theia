namespace Items
{
    interface IContainer
    {
        bool StowItem(IItem newItem);
        IItem TakeItem(IItem item);
    }
}