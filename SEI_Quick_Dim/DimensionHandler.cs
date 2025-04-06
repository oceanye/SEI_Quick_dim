using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using Tekla.Structures.Drawing;
using Tekla.Structures.Drawing.UI;
using DrawingColors = Tekla.Structures.Drawing.DrawingColors;
using TeklaDrawing = Tekla.Structures.Drawing;
using Point = Tekla.Structures.Geometry3d.Point;
using DrawingLine = Tekla.Structures.Drawing.Line;

namespace SEI_Quick_Dim
{
    /// <summary>
    /// 负责处理尺寸标注相关操作
    /// </summary>
    public class DimensionHandler
    {
        private static DrawingHandler _drawingHandler = new DrawingHandler();
        private static DimensionSettings _settings = new DimensionSettings();

        /// <summary>
        /// 创建快速尺寸标注
        /// </summary>
        public static void CreateDimension()
        {
            try
            {
                Logger.Log("开始创建尺寸标注");
                
                // 获取当前绘图
                var currDrawing = _drawingHandler.GetActiveDrawing();
                if (currDrawing == null)
                {
                    MessageBox.Show("请先打开一个绘图。");
                    Logger.Log("无法获取活动绘图。");
                    return;
                }
                
                // 获取视图
                var picker = _drawingHandler.GetPicker();
                TeklaDrawing.ViewBase selectedView = null;
                
                // 先获取视图，避免后续多次选择不在同一视图的问题
                try
                {
                    var pickedPoint = DrawingToolsExtensions.PickPointWithView(picker, "请在要创建尺寸标注的视图中选择一个点");
                    if (pickedPoint == null)
                    {
                        Logger.Log("未选择点或无法获取视图。");
                        return;
                    }
                    
                    selectedView = pickedPoint.View;
                    Logger.Log($"选择的视图已获取");
                }
                catch (Exception ex)
                {
                    Logger.LogError("选择初始点时出错", ex);
                    MessageBox.Show($"选择点时出错：{ex.Message}");
                    return;
                }
                
                // 使用PickObjectsArea方法框选对象
                Logger.Log("请框选要标注的对象");
                List<DrawingObject> selectedObjects = SelectObjectsWithArea(picker, selectedView);
                
                if (selectedObjects == null || selectedObjects.Count == 0)
                {
                    Logger.Log("未选择任何对象。");
                    MessageBox.Show("未选择任何对象。");
                    return;
                }
                
                Logger.Log($"选择了 {selectedObjects.Count} 个对象。");
                
                // 显示尺寸标注设置窗口
                var settingsWindow = new DimensionSettingsWindow();
                settingsWindow.SetSelectedObjects(selectedObjects);
                
                if (settingsWindow.ShowDialog() != true)
                {
                    Logger.Log("用户取消了尺寸标注操作。");
                    return;
                }
                
                // 获取用户设置
                _settings = settingsWindow.GetSettings();
                
                // 获取所有中心点
                List<Point> centerPoints = _settings.CenterPoints;
                
                if (centerPoints.Count == 0)
                {
                    Logger.Log("没有有效的中心点，无法创建尺寸标注。");
                    MessageBox.Show("没有有效的中心点，无法创建尺寸标注。");
                    return;
                }
                
                // 排序中心点（按X或Y坐标）
                Logger.Log("选择尺寸线的方向（垂直或水平）。");
                DrawingToolsExtensions.PickedPoint directionPoint = null;
                try
                {
                    directionPoint = DrawingToolsExtensions.PickPointWithView(picker, "请选择尺寸线的方向点（选择点的位置决定创建水平还是垂直尺寸线）");
                    if (directionPoint == null)
                    {
                        Logger.Log("未选择方向点。");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("选择方向点时出错", ex);
                    MessageBox.Show($"选择方向点时出错：{ex.Message}");
                    return;
                }
                
                // 根据方向点判断是水平还是垂直尺寸线
                Point firstCenterPoint = centerPoints.First();
                double distanceX = Math.Abs(directionPoint.Point.X - firstCenterPoint.X);
                double distanceY = Math.Abs(directionPoint.Point.Y - firstCenterPoint.Y);
                
                bool isHorizontal = distanceY > distanceX;
                
                if (isHorizontal)
                {
                    // 水平尺寸线，按Y坐标排序
                    centerPoints = centerPoints.OrderBy(p => p.Y).ToList();
                    Logger.Log("创建水平尺寸线，按Y坐标排序中心点。");
                }
                else
                {
                    // 垂直尺寸线，按X坐标排序
                    centerPoints = centerPoints.OrderBy(p => p.X).ToList();
                    Logger.Log("创建垂直尺寸线，按X坐标排序中心点。");
                }
                
                // 创建尺寸线
                CreateFallbackDimension(centerPoints, directionPoint.Point, isHorizontal, _settings.TextHeight, selectedView);
                
                // 记录成功创建
                Logger.Log("成功创建尺寸标注。");
            }
            catch (Exception ex)
            {
                Logger.LogError("创建尺寸标注时出错", ex);
                MessageBox.Show($"创建尺寸标注时出错：{ex.Message}\n{ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// 使用框选方式选择对象
        /// </summary>
        /// <param name="picker">绘图对象选择器</param>
        /// <param name="view">当前视图</param>
        /// <returns>选中的对象列表</returns>
        private static List<DrawingObject> SelectObjectsWithArea(Picker picker, TeklaDrawing.ViewBase view)
        {
            Logger.Log("尝试使用框选方式选择对象");
            List<DrawingObject> selectedObjects = new List<DrawingObject>();
            
            try
            {
                // 尝试使用反射调用PickObjectsArea方法
                var pickAreaMethod = picker.GetType().GetMethod("PickObjectsArea");
                if (pickAreaMethod != null)
                {
                    var result = pickAreaMethod.Invoke(picker, new object[] { "请框选要标注尺寸的对象" });
                    if (result is ArrayList areaPickResult && areaPickResult.Count > 0)
                    {
                        Logger.Log($"框选成功，获取到 {areaPickResult.Count} 个对象");
                        
                        foreach (var obj in areaPickResult)
                        {
                            if (obj is DrawingObject drawingObject)
                            {
                                selectedObjects.Add(drawingObject);
                                Logger.Log($"添加对象：{drawingObject.GetType().Name}");
                            }
                        }
                        
                        return selectedObjects;
                    }
                }
                
                // 如果PickObjectsArea不可用，回退到多次单选
                Logger.Log("框选方法不可用，回退到单对象选择模式");
                MessageBox.Show("框选功能不可用，将使用单对象选择模式。\n请依次选择要标注尺寸的对象，完成后按ESC键。");
                
                bool continueSelecting = true;
                while (continueSelecting)
                {
                    try
                    {
                        var pickResult = DrawingToolsExtensions.PickObject(picker, "请选择要标注尺寸的对象（完成后按ESC键）");
                        if (pickResult != null)
                        {
                            selectedObjects.Add(pickResult);
                            Logger.Log($"添加对象：{pickResult.GetType().Name}");
                        }
                        else
                        {
                            continueSelecting = false;
                        }
                    }
                    catch (Exception)
                    {
                        continueSelecting = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("选择对象时出错", ex);
                MessageBox.Show($"选择对象时出错: {ex.Message}");
            }
            
            return selectedObjects;
        }
        
        /// <summary>
        /// 计算对象的中心点
        /// </summary>
        /// <param name="obj">绘图对象</param>
        /// <returns>中心点坐标</returns>
        public static Point CalculateObjectCenter(DrawingObject obj)
        {
            try
            {
                Logger.Log($"计算对象中心点: {obj.GetType().Name}");
                
                if (obj is Line line)
                {
                    // 线段的中心是起点和终点的中点
                    return new Point(
                        (line.StartPoint.X + line.EndPoint.X) / 2,
                        (line.StartPoint.Y + line.EndPoint.Y) / 2,
                        0
                    );
                }
                else if (obj is Arc arc)
                {
                    // 弧的中心是其圆心（通过反射获取）
                    try
                    {
                        var centerProperty = arc.GetType().GetProperty("CenterPoint");
                        if (centerProperty != null)
                        {
                            var center = centerProperty.GetValue(arc) as Point;
                            if (center != null)
                            {
                                return center;
                            }
                        }
                    }
                    catch { }
                    
                    // 如果无法获取中心，使用起点和终点的中点
                    return new Point(
                        (arc.StartPoint.X + arc.EndPoint.X) / 2,
                        (arc.StartPoint.Y + arc.EndPoint.Y) / 2,
                        0
                    );
                }
                else if (obj is Circle circle)
                {
                    // 圆的中心是其圆心（通过反射获取）
                    try
                    {
                        var centerProperty = circle.GetType().GetProperty("CenterPoint");
                        if (centerProperty != null)
                        {
                            var center = centerProperty.GetValue(circle) as Point;
                            if (center != null)
                            {
                                return center;
                            }
                        }
                    }
                    catch { }
                }
                else if (obj is Rectangle rect)
                {
                    // 尝试通过反射获取矩形的边界点
                    try
                    {
                        // 尝试获取MinimumPoint和MaximumPoint属性
                        var minPointProperty = rect.GetType().GetProperty("MinimumPoint");
                        var maxPointProperty = rect.GetType().GetProperty("MaximumPoint");
                        
                        if (minPointProperty != null && maxPointProperty != null)
                        {
                            var minPoint = minPointProperty.GetValue(rect) as Point;
                            var maxPoint = maxPointProperty.GetValue(rect) as Point;
                            
                            if (minPoint != null && maxPoint != null)
                            {
                                return new Point(
                                    (minPoint.X + maxPoint.X) / 2,
                                    (minPoint.Y + maxPoint.Y) / 2,
                                    0
                                );
                            }
                        }
                    }
                    catch { }
                }
                else if (obj is Polygon polygon)
                {
                    // 多边形中心计算，尝试通过反射获取点集合
                    try
                    {
                        var pointsProperty = polygon.GetType().GetProperty("Points");
                        if (pointsProperty != null)
                        {
                            var points = pointsProperty.GetValue(polygon);
                            IEnumerable<Point> pointList = null;
                            
                            if (points is IEnumerable<Point> typedPoints)
                            {
                                pointList = typedPoints;
                            }
                            else if (points is ArrayList arrayList)
                            {
                                pointList = arrayList.Cast<Point>();
                            }
                            
                            if (pointList != null && pointList.Any())
                            {
                                double sumX = 0;
                                double sumY = 0;
                                int count = 0;
                                
                                foreach (Point point in pointList)
                                {
                                    sumX += point.X;
                                    sumY += point.Y;
                                    count++;
                                }
                                
                                if (count > 0)
                                {
                                    return new Point(sumX / count, sumY / count, 0);
                                }
                            }
                        }
                    }
                    catch { }
                }
                else if (obj is ModelObject modelObj)
                {
                    // 模型对象，尝试反射获取中心点
                    try
                    {
                        var centerProperty = modelObj.GetType().GetProperty("Center");
                        if (centerProperty != null)
                        {
                            var center = centerProperty.GetValue(modelObj) as Point;
                            if (center != null)
                            {
                                return center;
                            }
                        }
                        
                        // 如果没有Center属性，尝试获取位置
                        var locationProperty = modelObj.GetType().GetProperty("Location");
                        if (locationProperty != null)
                        {
                            var location = locationProperty.GetValue(modelObj) as Point;
                            if (location != null)
                            {
                                return location;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"获取模型对象中心点失败: {ex.Message}");
                    }
                }
                
                // 其他对象类型，尝试使用GetCenter方法
                try
                {
                    var getCenterMethod = obj.GetType().GetMethod("GetCenter");
                    if (getCenterMethod != null)
                    {
                        var center = getCenterMethod.Invoke(obj, null) as Point;
                        if (center != null)
                        {
                            return center;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"调用GetCenter方法失败: {ex.Message}");
                }
                
                Logger.Log($"无法计算对象中心点: {obj.GetType().Name}");
                return null;
            }
            catch (Exception ex)
            {
                Logger.Log($"计算对象中心点异常: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 获取绘图对象所在的视图
        /// </summary>
        private static TeklaDrawing.ViewBase GetViewOfDrawingObject(DrawingObject drawingObject)
        {
            try
            {
                // 获取当前绘图中的所有视图
                DrawingHandler drawingHandler = new DrawingHandler();
                Drawing activeDrawing = drawingHandler.GetActiveDrawing();
                if (activeDrawing == null)
                {
                    return null;
                }
                
                DrawingObjectEnumerator views = activeDrawing.GetSheet().GetAllViews();
                while (views.MoveNext())
                {
                    if (views.Current is TeklaDrawing.ViewBase viewBase)
                    {
                        try
                        {
                            // 尝试使用IsObjectInView方法
                            MethodInfo isObjectInViewMethod = viewBase.GetType().GetMethod("IsObjectInView");
                            if (isObjectInViewMethod != null)
                            {
                                bool isInView = (bool)isObjectInViewMethod.Invoke(viewBase, new object[] { drawingObject });
                                if (isInView)
                                {
                                    return viewBase;
                                }
                            }
                            else
                            {
                                // 如果找不到方法，使用其他方式判断
                                // 例如，检查对象的坐标是否在视图区域内
                                // 这部分需要根据实际情况完善
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"检查对象所在视图时出错: {ex.Message}", ex);
                        }
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError("获取对象所在视图出错", ex);
                return null;
            }
        }
        
        /// <summary>
        /// 备用方法：创建尺寸标注
        /// </summary>
        private static void CreateFallbackDimension(List<Point> points, Point dimensionLinePoint, bool isHorizontal, double textHeight, TeklaDrawing.ViewBase view)
        {
            if (points.Count < 2)
            {
                Logger.Log("点数不足，无法创建尺寸标注。");
                return;
            }

            try
            {
                // 绘制尺寸线
                DrawingLine dimensionLine = new DrawingLine(view, points.First(), points.Last());
                dimensionLine.Attributes.Line.Color = DrawingColors.Red;
                dimensionLine.Insert();
                Logger.Log("插入主尺寸线");
                
                // 绘制标注线和文字
                for (int i = 0; i < points.Count - 1; i++)
                {
                    Point startPoint = points[i];
                    Point endPoint = points[i + 1];
                    
                    // 计算两点之间的距离
                    double distance;
                    if (isHorizontal)
                    {
                        distance = Math.Abs(endPoint.Y - startPoint.Y);
                    }
                    else
                    {
                        distance = Math.Abs(endPoint.X - startPoint.X);
                    }
                    
                    // 绘制两个点之间的连接线
                    DrawingLine connectorLine = new DrawingLine(view, startPoint, endPoint);
                    connectorLine.Attributes.Line.Color = DrawingColors.Red;
                    connectorLine.Insert();
                    
                    // 计算文本位置
                    Point textPosition = new Point(
                        (startPoint.X + endPoint.X) / 2,
                        (startPoint.Y + endPoint.Y) / 2,
                        0
                    );
                    
                    Text dimensionText = new Text(view, textPosition, $"{distance:F0}");
                    dimensionText.Attributes.Font.Height = textHeight; // 使用用户设置的文本高度
                    dimensionText.Attributes.Font.Color = DrawingColors.Red;
                    dimensionText.Insert();
                    
                    Logger.Log($"插入尺寸标注：点 {i+1} 到点 {i+2}，距离 {distance:F0}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("创建尺寸标注出错", ex);
                MessageBox.Show($"创建尺寸标注出错: {ex.Message}");
            }
        }
    }
}
