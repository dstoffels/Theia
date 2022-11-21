namespace StatsOLD
{
    public interface IVital
    {
        float level { get; set; }
        float debility { get; }
        float max { get; }
        float min { get; }

        void StartRecovery();
    }
}