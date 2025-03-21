using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using Tekla.Structures.Drawing;
using Tekla.Structures.Drawing.UI;
//using static Tekla.Structures.Datatype.Distance;
using static Tekla.Structures.Drawing.LeaderLine;
using static Tekla.Structures.Drawing.LevelMark;
using Point = Tekla.Structures.Geometry3d.Point;
using TeklaDrawing = Tekla.Structures.Drawing;

namespace SEI_Quick_Dim
{
    /// <summary>
    /// 处理水平标高标记相关的功能类
    /// </summary>
    public class LevelMarkHandler
    {
        /// <summary>
        /// 创建水平标高标记
        /// </summary>
        public static void CreateLevelMark()
        {
            Logger.Log("LevelMarkHandler.CreateLevelMark 被调用");
            LevelMarkSettingsWindow settingsWindow = null;

            try
            {
                // 打开标高标记设置窗口
                Logger.Log("打开标高标记设置窗口...");
                settingsWindow = new LevelMarkSettingsWindow();
                
                // 订阅SettingsApplied事件
                settingsWindow.SettingsApplied += (s, e) => {
                    try {
                        // 每次应用设置后，立即进行标高标记创建
                        CreateLevelMarkWithSettings(settingsWindow);
                    }
                    catch (Exception ex) {
                        Logger.LogError("创建标高标记时发生错误", ex);
                    }
                };
                
                // 显示窗口，但不需要等待关闭
                settingsWindow.Show();
            }
            catch (Exception ex)
            {
                Logger.LogError("打开标高标记设置窗口失败", ex);
                MessageBox.Show($"打开标高标记设置窗口失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 使用指定设置创建标高标记
        /// </summary>
        private static void CreateLevelMarkWithSettings(LevelMarkSettingsWindow settingsWindow)
        {
            // 检查Tekla连接
            Logger.Log("检查Tekla连接状态...");
            
            // 获取DrawingHandler
            Logger.Log("获取DrawingHandler...");
            DrawingHandler drawingHandler = new DrawingHandler();
            
            Logger.Log("获取当前活动绘图...");
            Drawing activeDrawing = drawingHandler.GetActiveDrawing();
            if (activeDrawing == null)
            {
                Logger.Log("未找到活动绘图");
                MessageBox.Show("未找到活动绘图", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            Logger.Log($"成功获取活动绘图: {activeDrawing.Name}");
            
            // 获取Picker
            Logger.Log("获取Picker...");
            Picker picker = drawingHandler.GetPicker();
            
            // 基于设置决定选择点的数量
            int pointCount = settingsWindow.UseTwoPoints ? 2 : 3;
            Logger.Log($"将选择{pointCount}个点");
            
            // 用于收集用户选择的点
            List<(ViewBase ViewBase, Point Point)> pickedPoints = new List<(ViewBase, Point)>();
            
            // 提示用户选择点
            for (int i = 0; i < pointCount; i++)
            {
                Logger.Log($"提示用户选择第{i+1}个标高位置...");
                
                var pickedPoint = DrawingToolsExtensions.PickPointWithView(picker, $"请选择第{i+1}个标高位置");
                
                if (pickedPoint == null)
                {
                    Logger.Log("用户取消了选择");
                    return;
                }
                
                pickedPoints.Add((pickedPoint.ViewBase, pickedPoint.Point));
                Logger.Log($"用户选择的点{i+1}: X={pickedPoint.Point.X}, Y={pickedPoint.Point.Y}, Z={pickedPoint.Point.Z}");
            }
            
            // 为每个点创建标高标记
            Logger.Log("开始创建标高标记...");
            int successCount = 0;
            
            for (int pointIndex = 0; pointIndex < pickedPoints.Count; pointIndex++)
            {
                ViewBase viewBase = pickedPoints[pointIndex].ViewBase;
                Point basePoint = pickedPoints[pointIndex].Point;
                
                // 获取Z坐标作为标高值
                string levelText = "+" + (Math.Round(basePoint.Z / 10.0, 3) * 10).ToString("0.000");
                
                // 创建两个标高标记
                for (int markIndex = 0; markIndex < 2; markIndex++)
                {
                    try
                    {
                        // 决定当前使用的前缀和后缀索引
                        int prefixIndex = Math.Min(pointIndex * 2 + markIndex, settingsWindow.Prefixes.Count - 1);
                        int postfixIndex = Math.Min(pointIndex * 2 + markIndex, settingsWindow.Postfixes.Count - 1);
                        
                        string prefix = settingsWindow.Prefixes[prefixIndex];
                        string postfix = settingsWindow.Postfixes[postfixIndex];
                        
                        // 计算垂直偏移，第二个标记向下偏移
                        double yOffset = (markIndex == 1) ? -settingsWindow.VerticalOffset : 0;
                        double xOffset = settingsWindow.IsLeftPosition ? -settingsWindow.OffsetDistance : settingsWindow.OffsetDistance;

                        // 标记点位置 - 实际标记的位置为用户选择的点，仅添加垂直偏移
                        Point markPoint = new Point(
                            basePoint.X-1500+xOffset,
                            basePoint.Y+1500+yOffset*3, 
                            basePoint.Z);
                        
                        // 确定LeaderLine的偏移方向

                        // 创建LeaderLinePlacing的点 - 应用偏移量
                        Point leaderLinePoint = new Point(
                            basePoint.X ,  // 在这里应用水平偏移
                            basePoint.Y  ,  // 保持与标记点同样的垂直位置
                            basePoint.Z );
                        
                        Logger.Log($"创建标高标记 - 点{pointIndex+1}标记{markIndex+1}，前缀={prefix}，后缀={postfix}");
                        Logger.Log($"  标记位置: X={markPoint.X}, Y={markPoint.Y}");
                        Logger.Log($"  箭头位置: X={leaderLinePoint.X}, Y={leaderLinePoint.Y}, 偏移: {xOffset}");
                        
                        // 创建标高标记
                        LeaderLinePlacing leaderLine = new LeaderLinePlacing(leaderLinePoint);
                        LevelMark levelMark = new LevelMark(viewBase, markPoint, leaderLinePoint);
                        
                        // 设置标高标记属性
                        levelMark.Attributes.Font.Height = settingsWindow.FontHeight;


                        if (markIndex == 1)
                            // 设置 Leader Line 为无
                            levelMark.Attributes.Frame.Color =(DrawingColors)152;

                        // 设置单位为米 (m)。不同版本中可能是 Meter 或 Meters，视 API 而定
                        levelMark.Attributes.Unit.Unit = Units.Meters;

                        levelMark.Attributes.UsePositiveSignForPositiveLevels = true;

                        //levelMark.Attributes.ArrowHead.
                        // 设置小数点位数为 3，即 1/1000
                        levelMark.Attributes.Unit.Format = (FormatTypes)8;// FormatTypes.ThreeDecimals;
                        

                        // 设置数值格式为 ###.###
                        // 若此属性无效或略有差异，可通过检查 API 文档确定具体名称
                        //levelMark.Attributes.TextFormat = "###.###";



                        // 使用反射设置前缀、后缀和文本
                        var attributes = levelMark.Attributes;
                        var attributesType = attributes.GetType();




                        
                        // 设置前缀
                        var prefixProperty = attributesType.GetProperty("Prefix");
                        if (prefixProperty != null)
                        {
                            prefixProperty.SetValue(attributes, prefix);
                            Logger.Log($"设置标高标记前缀: {prefix}");
                        }
                        
                        // 设置后缀
                        var postfixProperty = attributesType.GetProperty("Postfix");
                        if (postfixProperty != null)
                        {
                            postfixProperty.SetValue(attributes, postfix);
                            Logger.Log($"设置标高标记后缀: {postfix}");
                        }

                        // 设置文本
                        //var textProperty = attributesType.GetProperty("Text");
                        //if (textProperty != null)
                        //{
                        //    textProperty.SetValue(attributes, levelText);
                        //    Logger.Log($"设置标高标记文本: {levelText}");
                        //}
                        //else
                        //{
                        //    var formatProperty = attributesType.GetProperty("Format");
                        //    if (formatProperty != null)
                        //    {
                        //        formatProperty.SetValue(attributes, levelText);
                        //        Logger.Log($"设置标高标记Format: {levelText}");
                        //    }
                        //}


                        //levelMark.InsertionPoint = new Point(basePoint.X+xOffset,basePoint.Y+xOffset,basePoint.Z);

                        // 应用属性
                        levelMark.Attributes = attributes;
                        
                        // 插入标高标记
                        bool insertResult = levelMark.Insert();
                        Logger.Log($"标高标记插入结果: {(insertResult ? "成功" : "失败")}");
                        
                        if (insertResult)
                        {
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"创建标高标记[点{pointIndex+1}-标记{markIndex+1}]时出错", ex);
                        
                        // 尝试使用备选方法 - 仅在标高标记创建失败时使用

                        Logger.LogError("创建标高标记失败，无备选方法", ex);

                    }
                }
            }
            
            // 提交更改
            activeDrawing.CommitChanges();
            
            // 显示结果
            if (successCount > 0)
            {
                Logger.Log($"成功创建{successCount}个标高标记");
                // 不再显示消息框，使流程更连续
            }
            else
            {
                Logger.Log("未能创建任何标高标记");
                MessageBox.Show("未能创建任何标高标记，请查看日志了解详情。", "失败", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
