using MATSys;
using MATSys.Hosting;
using MATSys.Hosting.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static JYAutoTester_Winform.Models.ExecutionModel;

namespace JYAutoTester_Winform.Models
{
    internal class ExecutionModel
    {
        private readonly IHost _host;
        public List<TestItem> TestItems { get; set; }=new List<TestItem>();
        public ExecutionModel()
        {
            var at=AppDomain.CurrentDomain.GetAssemblies();
            _host = Host.CreateDefaultBuilder().UseMATSys().Build();
            var runner = _host.Services.GetRunner();
            TestItems.AddRange(runner.TestScript.Setup);
            TestItems.AddRange(runner.TestScript.Test);
            TestItems.AddRange(runner.TestScript.Teardown);
            _host.RunAsync().Wait(500);
        }


        public IRunner GetRunner()
        {
            return _host.Services.GetRunner();
        }

        public void Start()
        {
            GetRunner().RunTestAsync();
        }
        public void Stop()
        {
            GetRunner().StopTest();

        }

        ~ExecutionModel()
        {
            _host.StopAsync();
        }


    }

    internal class Test
    {
        internal class PluginLoader : AssemblyLoadContext
        {
            private AssemblyDependencyResolver resolver;

            public PluginLoader(string? name, bool isCollectible = true) : base(name, isCollectible)
            {
                resolver = new AssemblyDependencyResolver(name!);
            }

            protected override Assembly? Load(AssemblyName assemblyName)
            {
                string assemblyPath = resolver.ResolveAssemblyToPath(assemblyName)!;
                if (assemblyPath != null)
                {
                    return LoadFromAssemblyPath(assemblyPath);
                }
                return null;
            }

            protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
            {
                string libraryPath = resolver.ResolveUnmanagedDllToPath(unmanagedDllName)!;
                if (libraryPath != null)
                {
                    return LoadUnmanagedDllFromPath(libraryPath);
                }

                return IntPtr.Zero;
            }
        }

        public IModule Module { get;}
        public Test()
        {
            var p = @"C:\Users\JYTW\source\repos\JYTEKTaiwan\JYAutoTester\EmptyLibraryNet6\bin\Debug\net6.0\EmptyLibraryNet6.dll";
            var loader = new PluginLoader(p);
            var assem = loader.LoadFromAssemblyPath(p);
            Type t = ParseType("USB101Test");
            var obj = (Activator.CreateInstance(t, new object[] { null, null, null, null, "RR" }));
            Module=(IModule)obj;
        }
        static Type ParseType(string typeString)
        {
            var assems = AppDomain.CurrentDomain.GetAssemblies();
            Type t = null;
            //check if section in the json configuration exits
            if (!string.IsNullOrEmpty(typeString))
            {
                if (typeString.Contains("."))
                {
                    //look up in the GAC
                    var dummy = Type.GetType(Assembly.CreateQualifiedName(typeString, typeString));
                    if (dummy != null)
                    {
                        t = dummy;
                    }
                }
                else
                {
                    //search all types in all assemblies
                    foreach (var assem in assems)
                    {

                        var dummy = assem.GetTypes().FirstOrDefault(x => x.Name.ToLower() == $"{typeString}".ToLower());
                        if (dummy == null)
                        {
                            continue;
                        }
                        else
                        {
                            t = dummy;
                            break;
                        }
                    }

                }

            }
            return t;
        }

    }
}
