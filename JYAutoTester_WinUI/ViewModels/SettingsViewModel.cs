using CommunityToolkit.Mvvm.ComponentModel;
using JYAutoTester.Models;
using JYAutoTester.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JYAutoTester.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private Models.AppSettingModel _model = new Models.AppSettingModel();
        private JsonSerializerOptions opt = new JsonSerializerOptions()
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        private string _nlog = "";
        private static IEnumerable<string> _extRefCategories = new List<string>(new string[]{
                "Modules",
                "Transceivers",
                "Notifiers",
                "Recorders",
                "Runners",
                "Analyzers"
            });
        private string scriptDirectory = @".\scripts";
        public ObservableCollection<ModuleContext> ModuleInfos { get; set; }
        public ObservableCollection<AssemblieInfo> ExternalReferences { get; set; }
        public string ScriptRootDirectory
        {
            get => scriptDirectory;
            set => SetProperty(ref scriptDirectory, value);
        }
        public string NLogSetting
        {
            get => _nlog;
            set => SetProperty(ref _nlog, value);
        }
        public SettingsViewModel()
        {
            ExternalReferences = new ObservableCollection<AssemblieInfo>(LoadExternalReference());
            ModuleInfos = new ObservableCollection<ModuleContext>(ListModuleInfos());
            NLogSetting = LoadNlogSettingFromFile();
            ScriptRootDirectory = _model.GetScriptRootDirectory();

        }

        public void Commit()
        {
            WriteExtRefSection();
            WriteModuleInfo();
            WriteNLogSetting();
            _model.UpdateRootDirectory(ScriptRootDirectory);
            _model.OverwriteFile();
        }

        #region External Assemblies related
        public void AddNewExternalReference(string category, string content)
        {
            var instance = ExternalReferences.FirstOrDefault(x => x.Name == category);
            if (instance != null)
            {
                instance.Files.Add(new AssemblieInfo() { Name = content });
            }
        }

        public void WriteExtRefSection()
        {
            JsonObject jObj = new JsonObject();
            foreach (var item in ExternalReferences)
            {
                if (item.Files.Count != 0)
                {
                    var node = new JsonArray();
                    foreach (var path in item.Files.Select(x => x.Name).ToArray())
                    {
                        node.Add(path);
                    }
                    jObj.Add(item.Name, node);
                }
            }
            _model.UpdateAssembliesInfos(jObj);
        }

        private IEnumerable<AssemblieInfo> LoadExternalReference()
        {

            foreach (var item in _extRefCategories)
            {
                yield return ParseExternalAssembly(item);
            }


        }
        private AssemblieInfo ParseExternalAssembly(string category)
        {
            var assem = new AssemblieInfo();
            assem.Name = category;
            foreach (var item in _model.ReadAssembliesInfos(category))
            {
                assem.Files.Add(new AssemblieInfo { Name = item });
            }

            return assem;
        }

        #endregion

        #region Modules information related
        private IEnumerable<ModuleContext> ListModuleInfos()
        {
            foreach (var item in _model.GetModulesInfo())
            {
                yield return ParseModuleInfoFromJson(item);
            }
        }
        private ModuleContext ParseModuleInfoFromJson(JsonNode item)
        {
            var mod = new ModuleContext();
            mod.Alias = item["Alias"].GetValue<string>();
            mod.Type = item["Type"].GetValue<string>();
            mod.RawString = item.ToJsonString(opt);
            var trans = new PluginContext();
            if (item.AsObject().ContainsKey("Transceiver") && item["Transceiver"].AsObject().ContainsKey("Type"))
            {
                trans.Type = item["Transceiver"]["Type"].GetValue<string>();
                trans.RawString = item["Transceiver"].ToJsonString(opt);
            }
            mod.Transceiver = trans;
            var not = new PluginContext();
            if (item.AsObject().ContainsKey("Notifier") && item["Notifier"].AsObject().ContainsKey("Type"))
            {
                not.Type = item["Notifier"]["Type"].GetValue<string>();
                not.RawString = item["Notifier"].ToJsonString(opt);
            }
            mod.Notifier = not;
            var rec = new PluginContext();
            if (item.AsObject().ContainsKey("Recorder") && item["Recorder"].AsObject().ContainsKey("Type"))
            {
                rec.Type = item["Recorder"]["Type"].GetValue<string>();
                rec.RawString = item["Recorder"].ToJsonString(opt);
            }
            mod.Recorder = rec;
            return mod;
        }
        public void UpdateModuleInfo(int idx, string content)
        {
            var config = JsonNode.Parse(content);
            var updateValue = ParseModuleInfoFromJson(config);
            ModuleInfos[idx].Alias = updateValue.Alias;
            ModuleInfos[idx].Type = updateValue.Type;
            ModuleInfos[idx].Transceiver = updateValue.Transceiver;
            ModuleInfos[idx].Notifier = updateValue.Notifier;
            ModuleInfos[idx].Recorder = updateValue.Recorder;
            ModuleInfos[idx].RawString = updateValue.RawString;
        }
        public void WriteModuleInfo()
        {
            JsonArray jArr = new JsonArray();
            foreach (var mod in ModuleInfos)
            {
                jArr.Add(JsonObject.Parse(mod.RawString));
            }
            _model.UpdateModulesInfo(jArr);
        }

        public void CreateNewModule()
        {
            var code = DateTime.Now.GetHashCode();
            var str = $"{{\"Alias\":\"{code}\",\"Type\":\"UNKNOWN\"}}";
            var mod = ParseModuleInfoFromJson(JsonNode.Parse(str));
            ModuleInfos.Add(mod);
        }
        #endregion

        #region NLog related
        public string LoadNlogSettingFromFile()
        {
            return _model.GetNLogInfo().ToJsonString(opt);
        }
        public string LoadDefaultSetting()
        {
            return AppSettingModel.NlogDefaultSetting;
        }

        public void WriteNLogSetting()
        {
            if (string.IsNullOrEmpty(NLogSetting))
            {
                _model.UpdateNLogInfo(null);
            }
            else
            {
                _model.UpdateNLogInfo(JsonObject.Parse(NLogSetting));
            }

        }
        #endregion
        #region ScriptRootDirectory related

        #endregion
    }


    public class ModuleContext : ObservableObject
    {
        private string _raw = "";
        private string _alias = "";
        private string _type = "";
        private PluginContext _trans;
        private PluginContext _not;
        private PluginContext _rec;
        public PluginContext Transceiver
        {
            get => _trans;
            set => SetProperty(ref _trans, value);
        }

        public PluginContext Notifier
        {
            get => _not;
            set => SetProperty(ref _not, value);
        }

        public PluginContext Recorder
        {
            get => _rec;
            set => SetProperty(ref _rec, value);
        }

        public string Alias
        {
            get => _alias;
            set => SetProperty(ref _alias, value);
        }

        public string Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }
        public string RawString
        {
            get => _raw;
            set => SetProperty(ref _raw, value);
        }

    }

    public class PluginContext
    {
        public string Type { get; set; } = "Empty";
        public string RawString { get; set; } = "{}";
    }




}
