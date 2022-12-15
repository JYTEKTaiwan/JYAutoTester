using MATSys;
using Microsoft.Extensions.Configuration;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MATSys.Plugins
{
    public class TextRecorder : IRecorder
    {
        public string Name => nameof(TextRecorder);

        public JsonObject Export()
        {
            return new JsonObject();
        }

        public string Export(bool indented = true)
        {
            return Export().ToJsonString(new JsonSerializerOptions() { WriteIndented=indented});
        }

        public void Load(IConfigurationSection section)
        {
        }

        public void Load(object configuration)
        { }

        public void StartService(CancellationToken token)
        {
        }

        public void StopService()
        {
        }

        public void Write(object data)
        {
            var text = $"[{DateTime.Now}]{JsonSerializer.Serialize(data)}\r\n";
            File.AppendAllText("log.txt",text);
        }

        public Task WriteAsync(object data)
        {
           return Task.Run(() => Write(data));
        }
    }
}