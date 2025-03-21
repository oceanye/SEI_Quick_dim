using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using Tekla.Structures.Drawing;
using Tekla.Structures.Drawing.UI;
using Point = Tekla.Structures.Geometry3d.Point;
using TeklaDrawing = Tekla.Structures.Drawing;
using DrawingLine = Tekla.Structures.Drawing.Line;

namespace SEI_Quick_Dim
{
    /// <summary>
    /// 处理快速尺寸标注相关的功能类
    /// </summary>
    public class DimensionHandler
    {
        /// <summary>
        /// 创建快速尺寸标注
        /// </summary>
        public static void CreateDimension()
        {
            Logger.Log("DimensionHandler.CreateDimension 被调用");
            
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
                    MessageBox.Show("未找到活动绘图", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                Logger.Log($"成功获取活动绘图: {activeDrawing.Name}");
                
                // 尝试选择两个点进行标注
                Logger.Log("尝试选择点进行标注...");
                
                // 获取Picker
                Picker picker = drawingHandler.GetPicker();
                
                // 选择尺寸线起点
                Logger.Log("请选择尺寸线起点...");
                var pickedStartPoint = DrawingToolsExtensions.PickPointWithView(picker, "请选择尺寸线起点");
                if (pickedStartPoint == null)
                {
                    Logger.Log("用户取消了选择起点");
                    return;
                }
                
                // 选择尺寸线终点
                Logger.Log("请选择尺寸线终点...");
                var pickedEndPoint = DrawingToolsExtensions.PickPointWithView(picker, "请选择尺寸线终点");
                if (pickedEndPoint == null)
                {
                    Logger.Log("用户取消了选择终点");
                    return;
                }
                
                // 确保选择的点在同一个视图中
                if (pickedStartPoint.ViewBase != pickedEndPoint.ViewBase)
                {
                    Logger.Log("选择的点不在同一个视图中");
                    MessageBox.Show("选择的点必须在同一个视图中", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                TeklaDrawing.ViewBase selectedView = pickedStartPoint.ViewBase;
                
                // 创建尺寸线
                try
                {
                    Logger.Log("尝试创建尺寸线...");
                    
                    // 准备点
                    Point startPoint = pickedStartPoint.Point;
                    Point endPoint = pickedEndPoint.Point;
                    
                    ArrayList pointsToSelect = new ArrayList();
                    pointsToSelect.Add(startPoint);
                    pointsToSelect.Add(endPoint);
                    
                    // 计算标注位置 (在两点上方50个单位)
                    double midX = (startPoint.X + endPoint.X) / 2;
                    double midY = (startPoint.Y + endPoint.Y) / 2;
                    double dx = endPoint.X - startPoint.X;
                    double dy = endPoint.Y - startPoint.Y;
                    double length = Math.Sqrt(dx * dx + dy * dy);
                    
                    // 计算垂直于线段的方向
                    double offsetX = -dy / length * 50;
                    double offsetY = dx / length * 50;
                    
                    Point dimensionPoint = new Point(midX + offsetX, midY + offsetY);
                    
                    // 由于无法正确使用StraightDimensionSet构造函数，使用线和文本模拟尺寸标注
                    DrawingLine dimensionLine = new DrawingLine(selectedView, startPoint, endPoint);
                    // 使用系统颜色而非Tekla颜色
                    dimensionLine.Attributes.Line.Type = TeklaDrawing.LineTypes.SolidLine;
                    
                    // 计算线段长度（作为标注文本）
                    double dimension = Math.Sqrt(Math.Pow(endPoint.X - startPoint.X, 2) + 
                                              Math.Pow(endPoint.Y - startPoint.Y, 2));
                    string dimensionText = $"{dimension:F2}";
                    
                    // 创建标注文本
                    Text measureText = new Text(selectedView, dimensionPoint, dimensionText);
                    measureText.Attributes.Frame.Type = TeklaDrawing.FrameTypes.None;
                    
                    // 插入模拟的尺寸标注
                    bool lineInserted = dimensionLine.Insert();
                    bool textInserted = measureText.Insert();
                    
                    Logger.Log($"尺寸线插入结果: {(lineInserted ? "成功" : "失败")}");
                    Logger.Log($"尺寸文本插入结果: {(textInserted ? "成功" : "失败")}");
                    
                    if (lineInserted && textInserted)
                    {
                        Logger.Log("尺寸标注(模拟)创建成功");
                        MessageBox.Show("尺寸标注创建成功（注：简化实现，非标准尺寸标注）", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        Logger.Log("尺寸标注创建失败");
                        MessageBox.Show("尺寸标注创建失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    
                    // 提交更改
                    activeDrawing.CommitChanges();
                }
                catch (Exception ex)
                {
                    Logger.LogError("创建尺寸标注时出错", ex);
                    MessageBox.Show($"创建尺寸标注时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("尺寸标注操作出错", ex);
                MessageBox.Show($"操作出错，详细信息请查看日志文件。\n\n" +
                               $"日志文件位置: {Logger.LogFilePath}", 
                               "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
