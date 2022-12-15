using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JYAutoTester.Models
{
    public class AppSettingModel
    {
        private readonly JsonNode? jObj;

        public const string NlogDefaultSetting = @"{
    ""throwConfigExceptions"": true,
    ""targets"": {
      ""async"": true,
      ""logfile"": {
        ""type"": ""File"",
        ""fileName"": ""./${shortdate}.log""
      },
      ""logconsole"": {
        ""type"": ""Console""
      }
    },
    ""rules"": [
      {
        ""logger"": ""none"",
        ""minLevel"": ""Trace"",
        ""writeTo"": ""logfile""
      }
    ]
  }";
        public bool IsReferenceSectionExists
        {
            get
            {
                return jObj["MATSys"].AsObject().ContainsKey("References");
            }
        }

        public AppSettingModel()
        {
            jObj = JsonNode.Parse(File.ReadAllText("appsettings.json"));
        }

        public string[] ReadAssembliesInfos(string categoryName)
        {
            if (jObj["MATSys"]["References"].AsObject().ContainsKey(categoryName))
            {
                return jObj["MATSys"]["References"][categoryName].Deserialize<string[]>();
            }
            else
            {
                return Array.Empty<string>();
            }
        }

        public void UpdateAssembliesInfos(JsonNode node)
        {
            jObj["MATSys"]["References"] = node;
        }
        public IEnumerable<JsonNode> GetModulesInfo()
        {
            foreach (var item in jObj["MATSys"]["Modules"].AsArray())
            {
                yield return item;
            }
        }
        public void UpdateModulesInfo(JsonArray node)
        {
            jObj["MATSys"]["Modules"] = node;
        }
        public JsonNode GetNLogInfo()
        {
            JsonObject obj = new JsonObject();
            if (jObj.AsObject().ContainsKey("NLog"))
            {
                obj = jObj["NLog"].AsObject();
            }
            return obj;
        }
        public void UpdateNLogInfo(JsonNode node)
        {
            if (node != null)
            {
                jObj["NLog"] = node;
            }
            else
            {
                jObj.AsObject().Remove("NLog");
            }

        }

        public void OverwriteFile()
        {
            //backup
            if (File.Exists("appsettings_bak.json"))
            {
                File.Delete("appsettings_bak.json");
            }
            File.Copy("appsettings.json", "appsettings_bak.json");

            File.WriteAllText("appsettings.json", jObj.ToJsonString(new JsonSerializerOptions() { WriteIndented = true }));
        }

        public string GetScriptRootDirectory()
        {
            return jObj["MATSys"]["Runner"]["RootDirectory"].GetValue<string>();
        }
        public void UpdateRootDirectory(string path)
        {
            jObj["MATSys"]["Runner"]["RootDirectory"] = path;
        }
    }
}
