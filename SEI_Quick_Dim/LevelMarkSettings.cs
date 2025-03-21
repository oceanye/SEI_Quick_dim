using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Windows;

namespace SEI_Quick_Dim
{
    /// <summary>
    /// 标高标记设置类，用于保存和读取标高标记的设置
    /// </summary>
    public class LevelMarkSettings
    {
        // 基本设置
        public bool UseTwoPoints { get; set; } = true;
        public bool IsLeftPosition { get; set; } = true;
        public double OffsetDistance { get; set; } = 50.0;
        public double VerticalOffset { get; set; } = 200.0;
        public double FontHeight { get; set; } = 2.5;
        
        // 前缀设置
        public List<string> Prefixes { get; set; } = new List<string> { "EL.", "OTM.", "EL.", "OTM.", "EL.", "OTM." };
        
        // 后缀设置
        public List<string> Postfixes { get; set; } = new List<string> { " BOB", " BOB", " TOC", " TOC", " BOC", " BOC" };
        
        // 设置文件的默认保存路径
        private static readonly string DefaultSettingsFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SEI_Quick_Dim");
            
        private static readonly string DefaultSettingsFile = Path.Combine(
            DefaultSettingsFolder, 
            "LevelMarkSettings.json");
            
        /// <summary>
        /// 保存设置到指定文件
        /// </summary>
        /// <param name="filePath">文件路径，如果为null则使用默认路径</param>
        /// <returns>保存是否成功</returns>
        public bool SaveSettings(string filePath = null)
        {
            try
            {
                string targetPath = filePath ?? DefaultSettingsFile;
                
                // 确保目录存在
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                
                // 序列化设置并保存
                string jsonString = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(targetPath, jsonString);
                
                Logger.Log($"标高标记设置已保存到: {targetPath}");
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError("保存标高标记设置失败", ex);
                return false;
            }
        }
        
        /// <summary>
        /// 从指定文件加载设置
        /// </summary>
        /// <param name="filePath">文件路径，如果为null则使用默认路径</param>
        /// <returns>加载的设置对象，如果加载失败则返回默认设置</returns>
        public static LevelMarkSettings LoadSettings(string filePath = null)
        {
            try
            {
                string targetPath = filePath ?? DefaultSettingsFile;
                
                // 检查文件是否存在
                if (!File.Exists(targetPath))
                {
                    Logger.Log($"设置文件不存在: {targetPath}，将使用默认设置");
                    return new LevelMarkSettings();
                }
                
                // 读取并反序列化设置
                string jsonString = File.ReadAllText(targetPath);
                var settings = JsonConvert.DeserializeObject<LevelMarkSettings>(jsonString);
                
                Logger.Log($"标高标记设置已从以下位置加载: {targetPath}");
                return settings;
            }
            catch (Exception ex)
            {
                Logger.LogError("加载标高标记设置失败", ex);
                return new LevelMarkSettings();
            }
        }
        
        /// <summary>
        /// 获取默认设置文件路径
        /// </summary>
        public static string GetDefaultSettingsFilePath()
        {
            return DefaultSettingsFile;
        }
    }
}
