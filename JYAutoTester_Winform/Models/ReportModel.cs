using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using CsvHelper;

namespace JYAutoTester_Winform.Models
{
    internal class ReportModel
    {
        private JsonNode sourceData;

        public JsonNode Load(string path)
        {
            var str= File.ReadAllText(path);
            sourceData = JsonArray.Parse(str);
            return sourceData;
        }

        public async Task SaveAsCSV(string path,IEnumerable data)
        {
            using (var writer = new StreamWriter(path))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(data);
            }
        }
    }
}
