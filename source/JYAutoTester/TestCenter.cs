using MATSys;
using MATSys.Commands;
using Microsoft.Extensions.Configuration;

namespace JYAutoTester;

public class TestCenter : ModuleBase
{
    private int cnt = 0;
    public TestCenter(object? configuration, ITransceiver? transceiver, INotifier? notifier, IRecorder? recorder, string aliasName = "") : base(configuration, transceiver, notifier, recorder, aliasName)
    {
    }

    public override void Load(IConfigurationSection section)
    {

    }

    public override void Load(object configuration)
    {

    }

    [MATSysCommand]
    public int TestMethodA()
    {
        Interlocked.Increment(ref cnt);
        return cnt;
    }

}
