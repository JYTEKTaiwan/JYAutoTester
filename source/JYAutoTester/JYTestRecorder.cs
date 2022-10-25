using MATSys;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JYAutoTester;

public class JYTestRecorder : IRecorder
{
    public string Name => nameof(JYTestRecorder);

    public JObject Export()
    {
        throw new NotImplementedException();
    }

    public string Export(Formatting format = Formatting.Indented)
    {
        throw new NotImplementedException();
    }

    public void Load(IConfigurationSection section)
    {
        throw new NotImplementedException();
    }

    public void Load(object configuration)
    {
        throw new NotImplementedException();
    }

    public void StartService(CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public void StopService()
    {
        throw new NotImplementedException();
    }

    public void Write(object data)
    {
        throw new NotImplementedException();
    }

    public Task WriteAsync(object data)
    {
        throw new NotImplementedException();
    }
}