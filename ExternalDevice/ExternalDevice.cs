using MATSys;
using MATSys.Commands;

namespace ExternalDevice
{
    public class ExternalDevice : ModuleBase
    {
        public ExternalDevice(object? configuration, ITransceiver? transceiver, INotifier? notifier, IRecorder? recorder, string aliasName = "") : base(configuration, transceiver, notifier, recorder, aliasName)
        {
        }

        [MATSysCommand]
        public TestItemResult TestMethod1()
        {
            var response = "THIS IS METHOD1 from EXTERNAL DEVICE";
            Base.Recorder.Write(response);
            return TestItemResult.Create(result:TestResultType.Skip,value: response);
        }
        [MATSysCommand]
        public TestItemResult TestMethod2()
        {
            var response = "THIS IS METHOD2 from EXTERNAL DEVICE";
            Base.Recorder.Write(response);
            return TestItemResult.Create(result:TestResultType.Skip,value: response);
        }
    }
}