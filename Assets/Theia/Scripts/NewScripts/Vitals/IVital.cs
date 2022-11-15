using System.Collections;

namespace Stats
{
    public interface IVital : IStat
    {
        new float level { get; }
        float max { get; }
        float min { get; }
        float threshold { get; }
        float debility { get; }

        IEnumerator Recover();
    }
}