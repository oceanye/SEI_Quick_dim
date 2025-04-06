using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using Tekla.Structures.Drawing;
using Tekla.Structures.Drawing.UI;
using TeklaDrawing = Tekla.Structures.Drawing;
using Point = Tekla.Structures.Geometry3d.Point;
using DrawingLine = Tekla.Structures.Drawing.Line;

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
                    MessageBox.Show("请先打开一个绘图。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                Logger.Log($"成功获取活动绘图: {activeDrawing.Name}");
                
                // 获取当前视图
                Logger.Log("尝试获取当前视图...");
                ViewBase selectedView = GetCurrentView(activeDrawing);
                if (selectedView == null)
                {
                    Logger.Log("未找到当前视图");
                    MessageBox.Show("未找到当前视图。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                // 显示批量钢筋标记设置窗口
                Logger.Log("打开批量钢筋标记设置窗口...");
                var settingsWindow = new RebarMarkSettingsWindow(selectedView);
                
                if (settingsWindow.ShowDialog() != true)
                {
                    Logger.Log("用户取消了操作");
                    return;
                }
                
                // 获取用户设置
                var settings = settingsWindow.GetSettings();
                
                // 创建尺寸标注
                Logger.Log("开始创建尺寸标注...");
                CreateDimensionMarks(settings, selectedView, activeDrawing);
                
                // 提交绘图更改
                Logger.Log("提交绘图更改...");
                activeDrawing.CommitChanges();
                
                Logger.Log("批量钢筋标记创建完成");
            }
            catch (Exception ex)
            {
                Logger.LogError("批量钢筋标记创建过程中出错", ex);
                MessageBox.Show($"批量钢筋标记创建过程中出错: {ex.Message}\n\n详细信息请查看日志文件。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// 获取当前视图
        /// </summary>
        private static ViewBase GetCurrentView(Drawing drawing)
        {
            try
            {
                var sheet = drawing.GetSheet();
                if (sheet != null)
                {
                    DrawingObjectEnumerator views = sheet.GetAllViews();
                    if (views != null && views.GetSize() > 0)
                    {
                        while (views.MoveNext())
                        {
                            if (views.Current is ViewBase viewBase)
                            {
                                // 假设第一个视图是当前视图（可以改进为更智能的选择）
                                return viewBase;
                            }
                        }
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError("获取当前视图时出错", ex);
                return null;
            }
        }
        
        /// <summary>
        /// 创建尺寸标注
        /// </summary>
        private static void CreateDimensionMarks(RebarMarkSettings settings, ViewBase view, Drawing drawing)
        {
            try
            {
                Logger.Log("开始创建尺寸标注");
                
                // 创建梁边界点
                Point beamPoint = new Point(settings.XBeamMin, settings.YBeamMin, 0);
                Logger.Log($"梁边界点: X={settings.XBeamMin:F2}, Y={settings.YBeamMin:F2}");
                
                // 创建板底部点和顶部点（X坐标与梁边界点相同）
                Point slabBottomPoint = new Point(settings.XBeamMin, settings.YSlabBottom, 0);
                Point slabTopPoint = new Point(settings.XBeamMin, settings.YSlabTop, 0);
                Logger.Log($"板底部点: X={settings.XBeamMin:F2}, Y={settings.YSlabBottom:F2}");
                Logger.Log($"板顶部点: X={settings.XBeamMin:F2}, Y={settings.YSlabTop:F2}");
                
                // 添加点到列表
                List<Point> points = new List<Point>
                {
                    beamPoint,
                    slabBottomPoint,
                    slabTopPoint
                };
                
                // 创建尺寸标注
                Logger.Log("创建尺寸标注线...");
                CreateFallbackDimension(points, view, settings.TextHeight, settings.SlabId, settings.BeamSection, settings.RebarType, drawing);
                
                Logger.Log("尺寸标注创建完成");
            }
            catch (Exception ex)
            {
                Logger.LogError("创建尺寸标注时出错", ex);
                throw;
            }
        }
        
        /// <summary>
        /// 创建基本尺寸标注（不使用StraightDimensionSet，直接使用Lines和Text对象）
        /// </summary>
        private static void CreateFallbackDimension(List<Point> points, ViewBase view, double textHeight, string slabId, string beamSection, string rebarType, Drawing drawing)
        {
            try
            {
                Logger.Log($"开始创建基本尺寸标注，共 {points.Count} 个点");
                
                if (points.Count < 2)
                {
                    Logger.Log("点数量不足，无法创建尺寸标注");
                    return;
                }
                
                // 创建红色线
                var lineColor = DrawingColors.Red;
                var textColor = DrawingColors.Red;
                
                // 创建垂直尺寸线
                double lineOffset = 25.0; // 尺寸线偏移量
                
                // 创建尺寸线（从第一个点到最后一个点）
                DrawingLine dimensionLine = new DrawingLine(
                    view,
                    new Point(points[0].X - lineOffset, points[0].Y, 0),
                    new Point(points[0].X - lineOffset, points[points.Count - 1].Y, 0)
                );
                
                // 使用反射设置颜色属性
                var attributesObj = dimensionLine.Attributes;
                var colorProp = attributesObj.GetType().GetProperty("Color");
                if (colorProp != null)
                {
                    colorProp.SetValue(attributesObj, lineColor);
                }
                
                dimensionLine.Insert();
                Logger.Log("尺寸主线创建成功");
                
                // 为每个点创建延伸线
                for (int i = 0; i < points.Count; i++)
                {
                    DrawingLine extensionLine = new DrawingLine(
                        view,
                        new Point(points[i].X, points[i].Y, 0),
                        new Point(points[i].X - lineOffset - 5, points[i].Y, 0)
                    );
                    
                    // 使用反射设置颜色属性
                    attributesObj = extensionLine.Attributes;
                    colorProp = attributesObj.GetType().GetProperty("Color");
                    if (colorProp != null)
                    {
                        colorProp.SetValue(attributesObj, lineColor);
                    }
                    
                    extensionLine.Insert();
                    Logger.Log($"创建延伸线 {i+1}");
                }
                
                // 添加尺寸值文本
                for (int i = 0; i < points.Count - 1; i++)
                {
                    // 计算两点间的距离
                    double distance = Math.Abs(points[i+1].Y - points[i].Y);
                    
                    // 文本位置在两点中间
                    Point textPoint = new Point(
                        points[0].X - lineOffset - 10,
                        (points[i].Y + points[i+1].Y) / 2,
                        0
                    );
                    
                    Text dimensionText = new Text(
                        view,
                        textPoint,
                        $"{distance:F0}"
                    );
                    
                    // 使用反射设置颜色和高度属性
                    var textAttributesObj = dimensionText.Attributes;
                    var textColorProp = textAttributesObj.GetType().GetProperty("Color");
                    var textHeightProp = textAttributesObj.GetType().GetProperty("Height");
                    
                    if (textColorProp != null)
                    {
                        textColorProp.SetValue(textAttributesObj, textColor);
                    }
                    
                    if (textHeightProp != null)
                    {
                        textHeightProp.SetValue(textAttributesObj, textHeight);
                    }
                    
                    dimensionText.Insert();
                    Logger.Log($"创建尺寸文本: {distance:F0}");
                }
                
                // 添加信息文本
                double infoTextOffsetX = -80; // 信息文本X偏移量
                
                // 添加板ID信息
                if (!string.IsNullOrEmpty(slabId))
                {
                    Point slabIdPoint = new Point(points[2].X + infoTextOffsetX, points[2].Y + 10, 0);
                    Text slabIdText = new Text(view, slabIdPoint, $"板ID: {slabId}");
                    
                    // 使用反射设置颜色和高度属性
                    var textAttributesObj = slabIdText.Attributes;
                    var textColorProp = textAttributesObj.GetType().GetProperty("Color");
                    var textHeightProp = textAttributesObj.GetType().GetProperty("Height");
                    
                    if (textColorProp != null)
                    {
                        textColorProp.SetValue(textAttributesObj, textColor);
                    }
                    
                    if (textHeightProp != null)
                    {
                        textHeightProp.SetValue(textAttributesObj, textHeight);
                    }
                    
                    slabIdText.Insert();
                    Logger.Log($"创建板ID文本: {slabId}");
                }
                
                // 添加梁截面信息
                if (!string.IsNullOrEmpty(beamSection))
                {
                    Point beamSectionPoint = new Point(points[2].X + infoTextOffsetX, points[2].Y + 10 + textHeight * 1.5, 0);
                    Text beamSectionText = new Text(view, beamSectionPoint, $"梁截面: {beamSection}");
                    
                    // 使用反射设置颜色和高度属性
                    var textAttributesObj = beamSectionText.Attributes;
                    var textColorProp = textAttributesObj.GetType().GetProperty("Color");
                    var textHeightProp = textAttributesObj.GetType().GetProperty("Height");
                    
                    if (textColorProp != null)
                    {
                        textColorProp.SetValue(textAttributesObj, textColor);
                    }
                    
                    if (textHeightProp != null)
                    {
                        textHeightProp.SetValue(textAttributesObj, textHeight);
                    }
                    
                    beamSectionText.Insert();
                    Logger.Log($"创建梁截面文本: {beamSection}");
                }
                
                // 添加钢筋型号信息
                if (!string.IsNullOrEmpty(rebarType))
                {
                    Point rebarTypePoint = new Point(points[2].X + infoTextOffsetX, points[2].Y + 10 + textHeight * 3, 0);
                    Text rebarTypeText = new Text(view, rebarTypePoint, $"钢筋型号: {rebarType}");
                    
                    // 使用反射设置颜色和高度属性
                    var textAttributesObj = rebarTypeText.Attributes;
                    var textColorProp = textAttributesObj.GetType().GetProperty("Color");
                    var textHeightProp = textAttributesObj.GetType().GetProperty("Height");
                    
                    if (textColorProp != null)
                    {
                        textColorProp.SetValue(textAttributesObj, textColor);
                    }
                    
                    if (textHeightProp != null)
                    {
                        textHeightProp.SetValue(textAttributesObj, textHeight);
                    }
                    
                    rebarTypeText.Insert();
                    Logger.Log($"创建钢筋型号文本: {rebarType}");
                }
                
                Logger.Log("基本尺寸标注创建完成");
            }
            catch (Exception ex)
            {
                Logger.LogError("创建基本尺寸标注时出错", ex);
                throw;
            }
        }
    }
}
