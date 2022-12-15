// See https://aka.ms/new-console-template for more information
using MATSys.Hosting;
using MATSys.Plugins.CSVRecorder;
using Microsoft.Extensions.Hosting;
using System.Reflection;

Console.WriteLine("Hello, World!");


Console.WriteLine(typeof(CSVRecorder).AssemblyQualifiedName);
var a=Assembly..CreateQualifiedName("MATSys.Plugins.CSVRecorder", "MATSys.Plugins.CSVRecorder.CSVRecorder");
var g=Type.GetType(a);

Console.ReadKey();
