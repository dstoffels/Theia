using System.Collections;

namespace Theia.Stats.vitals
{
    public interface iVital
    {
        int level { get; }
        int max { get; }
        int min { get; }
        int threshold { get; }
        int impairment { get; }
        bool recovering { get; }
        int recoveryRate { get; }
        IEnumerator Recover();
    }
}
