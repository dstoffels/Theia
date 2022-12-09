namespace Theia.Items.Base
{
    public interface iItem
    {
        string name { get; }
        int weight { get; }

        ItemSize size { get; }
    }
}
