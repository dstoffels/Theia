namespace Stats
{
    public interface IVital
    {
        float current { get; set; }
        float debility { get; }
        float max { get; }
        float min { get; }

        void StartRecovery();
    }
}