using MATSys;
using MATSys.Commands;
using MATSys.Hosting;
using MATSys.Hosting.Scripting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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
