// E:\hungryHub\hungry-fast-food\admin-panel\HungryFastFoodAdmin\Utils\ConfigManager.cs

using System;
using System.IO;
using System.Xml;

namespace HungryFastFoodAdmin
{
    public static class ConfigManager
    {
        public static string GetAppSetting(string key, string defaultValue = "")
        {
            try
            {
                string assemblyName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? "HungryFastFoodAdmin";
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{assemblyName}.dll.config");
                
                if (!File.Exists(configPath))
                {
                    configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{assemblyName}.exe.config");
                    if (!File.Exists(configPath))
                    {
                        configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App.config");
                        if (!File.Exists(configPath))
                        {
                            return defaultValue;
                        }
                    }
                }

                XmlDocument doc = new XmlDocument();
                doc.Load(configPath);
                XmlNode node = doc.SelectSingleNode($"/configuration/appSettings/add[@key='{key}']");
                if (node != null && node.Attributes != null && node.Attributes["value"] != null)
                {
                    return node.Attributes["value"].Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ConfigManager error reading key {key}: {ex.Message}");
            }
            return defaultValue;
        }
    }
}
