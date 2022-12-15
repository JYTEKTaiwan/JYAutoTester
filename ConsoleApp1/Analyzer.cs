using MATSys.Hosting;
using System.Text.Json;

namespace JYAutoTesterDEMO
{
    internal static class Analyzer
    {
        public static bool SmallerThan(this AnalyzingData data, double thredshold)
        {
            return JsonSerializer.Deserialize<double>(data.Value) < thredshold;
        }

        public static bool Between(this AnalyzingData data, double low, double high)
        {
            var v = JsonSerializer.Deserialize<double>(data.Value);
            return v > low && v < high;
        }
    }


}
