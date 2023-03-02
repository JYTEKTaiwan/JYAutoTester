// See https://aka.ms/new-console-template for more information
using MATSys.Commands;
using MATSys.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;

namespace ConsoleDEMO
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            //configure host
            var host = Host.CreateDefaultBuilder().ConfigureLogging(x => x.ClearProviders()).UseMATSys().Build();

            //start the host
            host.RunAsync().Wait(500);

            //get runner
            var runner = host.Services.GetRunner();

            //register event from runner
            runner.BeforeScriptStarts += Runner_BeforeScriptStarts;
            runner.BeforeTestItemStarts += Runner_BeforeTestItemStarts;
            runner.AfterSubTestItemComplete += Runner_AfterSubTestItemComplete;
            runner.AfterScriptStops += Runner_AfterScriptStops;

            //start execution
            Console.WriteLine("Press ANY key to start....");
            Console.ReadKey();
            var result=runner.RunTest();
            
            //stop the host
            host.StopAsync();

            //export log
            Console.WriteLine("Press ANY key to export report...");
            Console.ReadKey();
            Export(result);

        }

        private static void Runner_BeforeScriptStarts(MATSys.Hosting.Scripting.TestScriptContext script)
        {
            var cnt = script.Setup.Count + script.Test.Count + script.Teardown.Count;
            Console.WriteLine($"{cnt} TestItems are found");
            Console.WriteLine($"Ready to Execute...");
            Console.WriteLine("=============START===============");
        }

        private static void Runner_AfterScriptStops(JsonArray item)
        {
            Console.WriteLine("=============FINISHED===============");
        }

        private static void Runner_AfterSubTestItemComplete(MATSys.Hosting.Scripting.TestItem item, JsonNode result)
        {
            var res = result["Result"].ToString();
            switch (res)
            {
                case "Pass":
                    Console.BackgroundColor = ConsoleColor.Green;

                    break;
                case "Fail":
                    Console.BackgroundColor = ConsoleColor.Red;
                    break;
                default:
                    break;
            }
            Console.WriteLine($"{res}");
            Console.ResetColor();
        }

        private static void Runner_BeforeTestItemStarts(MATSys.Hosting.Scripting.TestItem item)
        {
            var name = item.Executer.Value.CommandString.AsObject().First().Key;
            Console.Write($"{name}...\t");
        }

        private static void Export(JsonArray result)
        {

            Console.WriteLine("Saving to TXT");
            //save to txt
            var str=(result.ToJsonString(new System.Text.Json.JsonSerializerOptions() { WriteIndented = true }));
            File.WriteAllText(@".\report.txt", str);

            Console.WriteLine("Saving to CSV");
            //save to csv
            List<string> contents =new List<string>();
            foreach (var item in result)
            {
                var content = "";
                foreach (var prop in item.AsObject().ToArray())
                {
                    content += prop.Value==null?"":prop.Value.ToJsonString()+",";
                }
                contents.Add(content);
            }
            File.WriteAllLines(@".\report.csv", contents.ToArray());

            Console.WriteLine("=============EXPORT DONE===============");

        }
    }
}

