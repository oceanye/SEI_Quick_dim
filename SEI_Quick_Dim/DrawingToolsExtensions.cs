using System;
using System.Collections;
using System.Reflection;
using System.Windows;
using Tekla.Structures.Drawing;
using Tekla.Structures.Drawing.UI;
using TeklaPoint = Tekla.Structures.Geometry3d.Point;

namespace SEI_Quick_Dim
{
    /// <summary>
    /// 提供绘图工具扩展方法
    /// </summary>
    public static class DrawingToolsExtensions
    {
        /// <summary>
        /// 点和所属视图的数据结构
        /// </summary>
        public class PickedPoint
        {
            public TeklaPoint Point { get; set; }
            public ViewBase View { get; set; }
        }

        /// <summary>
        /// 从视图中获取边界点信息
        /// </summary>
        public class ViewBoundaryPoints
        {
            public TeklaPoint BeamBoundary { get; set; }
            public double SlabTopY { get; set; }
            public double SlabBottomY { get; set; }
            public bool IsValid => BeamBoundary != null;
        }

        /// <summary>
        /// 从视图中获取边界点
        /// </summary>
        /// <param name="view">要分析的视图</param>
        /// <returns>包含边界点信息的对象</returns>
        public static ViewBoundaryPoints GetBoundaryPointsFromView(ViewBase view)
        {
            try
            {
                if (view == null)
                {
                    Logger.LogError("视图为空", new ArgumentNullException(nameof(view)));
                    return new ViewBoundaryPoints();
                }

                var result = new ViewBoundaryPoints
                {
                    BeamBoundary = new TeklaPoint(0, 0, 0),
                    SlabTopY = double.MinValue,
                    SlabBottomY = double.MaxValue
                };

                // 获取视图中的所有对象
                var allObjects = view.GetAllObjects();
                if (allObjects == null || allObjects.GetSize() == 0)
                {
                    Logger.Log("视图中没有找到对象");
                    return result;
                }

                double minX = double.MaxValue;
                double minY = double.MaxValue;

                while (allObjects.MoveNext())
                {
                    var obj = allObjects.Current;
                    if (obj == null) continue;

                    try
                    {
                        // 获取对象的边界点
                        var minPoint = GetObjectMinPoint(obj);
                        var maxPoint = GetObjectMaxPoint(obj);

                        if (minPoint != null && maxPoint != null)
                        {
                            // 更新梁边界点（取最小x和y坐标）
                            if (minPoint.X < minX)
                            {
                                minX = minPoint.X;
                                result.BeamBoundary.X = minX;
                            }
                            if (minPoint.Y < minY)
                            {
                                minY = minPoint.Y;
                                result.BeamBoundary.Y = minY;
                            }

                            // 更新板顶部和底部Y坐标
                            if (maxPoint.Y > result.SlabTopY)
                            {
                                result.SlabTopY = maxPoint.Y;
                            }
                            if (minPoint.Y < result.SlabBottomY)
                            {
                                result.SlabBottomY = minPoint.Y;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"处理对象时出错: {ex.Message}", ex);
                    }
                }

                Logger.Log($"找到边界点 - 梁边界: ({result.BeamBoundary.X:F2}, {result.BeamBoundary.Y:F2}), " +
                         $"板顶部: {result.SlabTopY:F2}, 板底部: {result.SlabBottomY:F2}");

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError($"获取视图边界点时出错: {ex.Message}", ex);
                return new ViewBoundaryPoints();
            }
        }

        private static TeklaPoint GetObjectMinPoint(DrawingObject obj)
        {
            try
            {
                // 使用反射获取对象的最小点
                var minPointProperty = obj.GetType().GetProperty("MinimumPoint");
                if (minPointProperty != null)
                {
                    return minPointProperty.GetValue(obj) as TeklaPoint;
                }

                // 如果没有MinimumPoint属性，尝试获取StartPoint（对于线段等）
                var startPointProperty = obj.GetType().GetProperty("StartPoint");
                if (startPointProperty != null)
                {
                    return startPointProperty.GetValue(obj) as TeklaPoint;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static TeklaPoint GetObjectMaxPoint(DrawingObject obj)
        {
            try
            {
                // 使用反射获取对象的最大点
                var maxPointProperty = obj.GetType().GetProperty("MaximumPoint");
                if (maxPointProperty != null)
                {
                    return maxPointProperty.GetValue(obj) as TeklaPoint;
                }

                // 如果没有MaximumPoint属性，尝试获取EndPoint（对于线段等）
                var endPointProperty = obj.GetType().GetProperty("EndPoint");
                if (endPointProperty != null)
                {
                    return endPointProperty.GetValue(obj) as TeklaPoint;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 选择一个点并获取该点所在视图
        /// </summary>
        /// <param name="picker">绘图选择器</param>
        /// <param name="customPrompt">自定义提示文本</param>
        /// <returns>包含点坐标和所在视图的对象</returns>
        public static PickedPoint PickPointWithView(this Picker picker, string customPrompt = null)
        {
            try
            {
                string prompt = customPrompt ?? "请选择一个点";
                Logger.Log($"尝试选择点: {prompt}");

                // 使用Picker直接选择点
                object pickedInfo = picker.PickPoint(prompt);
                if (pickedInfo == null)
                {
                    Logger.Log("未选择点或点无效");
                    return null;
                }
                
                // 使用反射处理不同类型的返回值
                Type pickedType = pickedInfo.GetType();
                
                // 判断是否是元组类型
                if (pickedType.IsGenericType && pickedType.GetGenericTypeDefinition() == typeof(Tuple<,>))
                {
                    // 通过反射获取元组的Item1和Item2
                    PropertyInfo item1Property = pickedType.GetProperty("Item1");
                    PropertyInfo item2Property = pickedType.GetProperty("Item2");
                    
                    if (item1Property != null && item2Property != null)
                    {
                        object item1 = item1Property.GetValue(pickedInfo);
                        object item2 = item2Property.GetValue(pickedInfo);
                        
                        var tuplePoint = item1 as TeklaPoint;
                        var tupleView = item2 as ViewBase;
                        
                        if (tuplePoint != null && tupleView != null)
                        {
                            Logger.Log($"选择了点: X={tuplePoint.X}, Y={tuplePoint.Y}, Z={tuplePoint.Z}");
                            return new PickedPoint { Point = tuplePoint, View = tupleView };
                        }
                    }
                }
                
                // 如果不是元组或获取元组值失败，检查是否直接是Point类型
                var directPoint = pickedInfo as TeklaPoint;
                if (directPoint != null)
                {
                    ViewBase view = null;
                    
                    // 尝试获取当前视图
                    try
                    {
                        DrawingHandler drawingHandler = new DrawingHandler();
                        var activeDrawing = drawingHandler.GetActiveDrawing();
                        if (activeDrawing != null)
                        {
                            var sheet = activeDrawing.GetSheet();
                            if (sheet != null)
                            {
                                var views = sheet.GetAllViews();
                                if (views != null && views.GetSize() > 0)
                                {
                                    while (views.MoveNext())
                                    {
                                        var currentView = views.Current as ViewBase;
                                        if (currentView != null)
                                        {
                                            view = currentView;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("获取点所在视图时出错", ex);
                    }
                    
                    Logger.Log($"选择了点: X={directPoint.X}, Y={directPoint.Y}, Z={directPoint.Z}");
                    return new PickedPoint { Point = directPoint, View = view };
                }
                
                var ex2 = new Exception($"无法处理选择的点类型: {pickedType.FullName}");
                Logger.LogError("选择点时出错", ex2);
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError($"选择点时出错: {ex.Message}", ex);
                return null;
            }
        }
        
        /// <summary>
        /// 选择一个绘图对象
        /// </summary>
        /// <param name="picker">绘图选择器</param>
        /// <param name="customPrompt">自定义提示文本</param>
        /// <returns>选中的绘图对象</returns>
        public static DrawingObject PickObject(this Picker picker, string customPrompt = null)
        {
            try
            {
                string prompt = customPrompt ?? "请选择一个对象";
                Logger.Log($"尝试选择对象: {prompt}");

                try
                {
                    // 使用Picker直接选择对象
                    object pickedInfo = picker.PickObject(prompt);
                    if (pickedInfo == null)
                    {
                        Logger.Log("未选择对象或对象无效");
                        return null;
                    }
                    
                    // 使用反射处理不同类型的返回值
                    Type pickedType = pickedInfo.GetType();
                    
                    // 判断是否是元组类型
                    if (pickedType.IsGenericType && pickedType.GetGenericTypeDefinition() == typeof(Tuple<,>))
                    {
                        // 通过反射获取元组的Item1和Item2
                        PropertyInfo item1Property = pickedType.GetProperty("Item1");
                        
                        if (item1Property != null)
                        {
                            object item1 = item1Property.GetValue(pickedInfo);
                            var tupleObject = item1 as DrawingObject;
                            
                            if (tupleObject != null)
                            {
                                Logger.Log($"选择了对象: {tupleObject.GetType().Name}");
                                return tupleObject;
                            }
                        }
                    }
                    
                    // 如果不是元组或获取元组值失败，检查是否直接是DrawingObject类型
                    var directObject = pickedInfo as DrawingObject;
                    if (directObject != null)
                    {
                        Logger.Log($"选择了对象: {directObject.GetType().Name}");
                        return directObject;
                    }
                    
                    var ex2 = new Exception($"无法处理选择的对象类型: {pickedType.FullName}");
                    Logger.LogError("选择对象时出错", ex2);
                    return null;
                }
                catch (Exception ex)
                {
                    // 判断是否是用户取消操作的异常
                    if (ex.Message.Contains("cancel") || ex.Message.Contains("ESC"))
                    {
                        Logger.Log("用户取消了选择");
                        return null;
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"选择对象时出错: {ex.Message}", ex);
                throw;
            }
        }
    }
}
