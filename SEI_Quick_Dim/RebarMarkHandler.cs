using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using Tekla.Structures.Drawing;
using Tekla.Structures.Drawing.UI;
using TeklaDrawing = Tekla.Structures.Drawing;

namespace SEI_Quick_Dim
{
    /// <summary>
    /// 处理批量钢筋标注相关的功能类
    /// </summary>
    public class RebarMarkHandler
    {
        /// <summary>
        /// 创建钢筋标记 - 从主窗口调用的方法
        /// </summary>
        public static void CreateRebarMark()
        {
            // 调用实际的实现方法
            CreateRebarMarks();
        }
        
        /// <summary>
        /// 创建批量钢筋标注
        /// </summary>
        public static void CreateRebarMarks()
        {
            Logger.Log("RebarMarkHandler.CreateRebarMarks 被调用");
            
            try
            {
                // 检查Tekla连接状态
                Logger.Log("检查Tekla连接状态...");
                
                // 获取当前绘图
                Logger.Log("获取DrawingHandler...");
                DrawingHandler drawingHandler = new DrawingHandler();
                
                Logger.Log("获取当前活动绘图...");
                Drawing activeDrawing = drawingHandler.GetActiveDrawing();
                if (activeDrawing == null)
                {
                    Logger.Log("未找到活动绘图");
                    return;
                }
                
                Logger.Log($"成功获取活动绘图: {activeDrawing.Name}");
                
                // 获取picker
                Logger.Log("获取Picker...");
                Picker picker = drawingHandler.GetPicker();
                
                // 提示用户选择要标记的钢筋
                Logger.Log("提示用户选择钢筋...");
                DrawingObject pickedObject = DrawingToolsExtensions.PickObject(picker, "请选择需要标记的钢筋");
                
                if (pickedObject == null)
                {
                    Logger.Log("用户取消了选择");
                    return;
                }
                
                // 检查选择的对象是否为钢筋
                if (!(pickedObject is ReinforcementBase))
                {
                    Logger.Log($"选择的对象不是钢筋，而是 {pickedObject.GetType().Name}");
                    MessageBox.Show("请选择钢筋对象", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                ReinforcementBase reinforcement = pickedObject as ReinforcementBase;
                
                // 提示用户选择标记位置
                Logger.Log("提示用户选择标记位置...");
                var pickedPoint = DrawingToolsExtensions.PickPointWithView(picker, "请选择标记位置");
                
                if (pickedPoint == null)
                {
                    Logger.Log("用户取消了选择标记位置");
                    return;
                }
                
                // 创建钢筋标记
                try
                {
                    // 创建钢筋标记
                    Logger.Log("创建钢筋标记...");
                    
                    // 创建标记
                    Mark mark = null;
                    try
                    {
                        // 使用可能的构造函数
                        Type markType = typeof(Mark);
                        mark = (Mark)Activator.CreateInstance(markType, new object[] { pickedPoint.ViewBase });
                        mark.InsertionPoint = pickedPoint.Point;
                        Logger.Log("成功创建Mark对象");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("创建Mark对象失败", ex);
                        throw;
                    }
                    
                    // 添加钢筋对象 - 使用反射动态设置和处理标记
                    try
                    {
                        // 反射查找和调用AddObject方法
                        var addObjectMethod = mark.GetType().GetMethod("AddObject");
                        if (addObjectMethod != null)
                        {
                            addObjectMethod.Invoke(mark, new object[] { reinforcement });
                            Logger.Log("成功添加钢筋对象到标记");
                        }
                        else
                        {
                            Logger.Log("无法找到AddObject方法");
                        }
                        
                        // 设置标记类型 - 使用反射
                        var attributesType = mark.Attributes.GetType();
                        var markTypeProperty = attributesType.GetProperty("MarkType");
                        if (markTypeProperty != null)
                        {
                            // 尝试获取和设置MarkType枚举
                            try 
                            {
                                var markTypeEnum = Type.GetType("Tekla.Structures.Drawing.MarkType");
                                if (markTypeEnum != null && markTypeEnum.IsEnum)
                                {
                                    var rebarMarkValue = Enum.Parse(markTypeEnum, "REBAR_MARK");
                                    markTypeProperty.SetValue(mark.Attributes, rebarMarkValue);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError("设置MarkType属性时出错", ex);
                            }
                        }
                        
                        // 设置标记内容
                        var contentProperty = attributesType.GetProperty("Content");
                        if (contentProperty != null)
                        {
                            var content = contentProperty.GetValue(mark.Attributes);
                            var contentType = content.GetType();
                            
                            // 清除现有内容
                            var clearMethod = contentType.GetMethod("Clear");
                            if (clearMethod != null)
                            {
                                clearMethod.Invoke(content, null);
                            }
                            
                            // 添加钢筋尺寸和数量信息 - 简化处理，实际中应通过反射添加
                            // 这里实现为基本的文本元素
                            try
                            {
                                var addTextMethod = contentType.GetMethod("Add", new Type[] { typeof(TextElement) });
                                if (addTextMethod != null)
                                {
                                    addTextMethod.Invoke(content, new object[] { new TextElement("钢筋尺寸") });
                                    addTextMethod.Invoke(content, new object[] { new TextElement(" ") });
                                    addTextMethod.Invoke(content, new object[] { new TextElement("数量") });
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError("添加标记内容时出错", ex);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("设置钢筋标记属性时出错", ex);
                    }
                    
                    // 插入标记
                    bool insertResult = mark.Insert();
                    Logger.Log($"钢筋标记插入结果: {(insertResult ? "成功" : "失败")}");
                    
                    if (insertResult)
                    {
                        Logger.Log("钢筋标记创建成功");
                    }
                    else
                    {
                        Logger.Log("钢筋标记创建失败");
                    }
                    
                    // 提交更改
                    activeDrawing.CommitChanges();
                }
                catch (Exception ex)
                {
                    Logger.LogError("创建钢筋标记时出错", ex);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("钢筋标注操作出错", ex);
            }
        }
    }
}
