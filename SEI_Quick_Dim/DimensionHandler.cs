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
using Grid = Tekla.Structures.Drawing.Grid; // Explicitly specify Tekla Grid to avoid ambiguity

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
                
                // 提示用户选择两个轴网
                Logger.Log("请选择两个轴网 (Grid) 对象");
                MessageBox.Show("请选择两个轴网 (Grid) 对象", "选择轴网", MessageBoxButton.OK, MessageBoxImage.Information);
                
                List<Grid> selectedGrids = new List<Grid>();
                
                // 选择第一个轴网
                try
                {
                    DrawingObject pickedObject = DrawingToolsExtensions.PickObject(picker, "请选择第一个轴网");
                    if (pickedObject is Grid grid1)
                    {
                        selectedGrids.Add(grid1);
                        Logger.Log("已选择第一个轴网");
                    }
                    else
                    {
                        MessageBox.Show("所选对象不是轴网，请重新选择。");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("选择第一个轴网时出错", ex);
                    MessageBox.Show($"选择轴网时出错：{ex.Message}");
                    return;
                }
                
                // 选择第二个轴网
                try
                {
                    DrawingObject pickedObject = DrawingToolsExtensions.PickObject(picker, "请选择第二个轴网");
                    if (pickedObject is Grid grid2)
                    {
                        selectedGrids.Add(grid2);
                        Logger.Log("已选择第二个轴网");
                    }
                    else
                    {
                        MessageBox.Show("所选对象不是轴网，请重新选择。");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("选择第二个轴网时出错", ex);
                    MessageBox.Show($"选择轴网时出错：{ex.Message}");
                    return;
                }
                
                // 使用PickObject方法多次选择几何对象
                Logger.Log("请选择要标注的几何对象");
                MessageBox.Show("请依次选择要标注尺寸的几何对象，完成后按ESC键。", "选择对象", MessageBoxButton.OK, MessageBoxImage.Information);
                
                List<DrawingObject> selectedGeometricObjects = new List<DrawingObject>();
                bool continueSelecting = true;
                
                while (continueSelecting)
                {
                    try
                    {
                        DrawingObject pickedObject = DrawingToolsExtensions.PickObject(picker, "请选择要标注尺寸的几何对象（完成后按ESC键）");
                        if (pickedObject != null)
                        {
                            selectedGeometricObjects.Add(pickedObject);
                            Logger.Log($"添加几何对象：{pickedObject.GetType().Name}");
                        }
                    }
                    catch (Exception)
                    {
                        // 用户按ESC结束选择，这是正常流程
                        continueSelecting = false;
                        Logger.Log("用户完成了几何对象选择");
                    }
                }
                
                // 合并所有对象
                List<DrawingObject> allSelectedObjects = new List<DrawingObject>();
                allSelectedObjects.AddRange(selectedGrids.Cast<DrawingObject>());
                allSelectedObjects.AddRange(selectedGeometricObjects);
                
                if (allSelectedObjects.Count < 2)
                {
                    Logger.Log("选择的对象不足，无法创建尺寸标注。");
                    MessageBox.Show("至少需要选择2个对象来创建尺寸标注。");
                    return;
                }
                
                Logger.Log($"总共选择了 {allSelectedObjects.Count} 个对象。");
                
                // 显示尺寸标注设置窗口
                var settingsWindow = new DimensionSettingsWindow();
                settingsWindow.SetSelectedObjects(allSelectedObjects);
                
                if (settingsWindow.ShowDialog() != true)
                {
                    Logger.Log("用户取消了尺寸标注操作。");
                    return;
                }
                
                // 获取用户设置
                _settings = settingsWindow.GetSettings();
                
                // 获取所有中心点
                List<Point> centerPoints = _settings.CenterPoints;
                
                if (centerPoints.Count < 2)
                {
                    Logger.Log("有效的中心点不足，无法创建尺寸标注。");
                    MessageBox.Show("有效的中心点不足，无法创建尺寸标注。");
                    return;
                }
                
                // 询问用户选择标注方向
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
                
                // 创建直线尺寸标注
                CreateDimensionLineSet(centerPoints, directionPoint.Point, isHorizontal, _settings.TextHeight, selectedView);
                
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
                    // 尝试使用反射获取圆弧中心点
                    try 
                    {
                        PropertyInfo centerProp = arc.GetType().GetProperty("CenterPoint");
                        if (centerProp != null)
                        {
                            return centerProp.GetValue(arc) as Point;
                        }
                    }
                    catch { }
                    
                    // 如果反射失败，使用起点和终点的中点
                    return new Point(
                        (arc.StartPoint.X + arc.EndPoint.X) / 2,
                        (arc.StartPoint.Y + arc.EndPoint.Y) / 2,
                        0
                    );
                }
                else if (obj is Circle circle)
                {
                    // 尝试使用反射获取圆心
                    try 
                    {
                        PropertyInfo centerProp = circle.GetType().GetProperty("CenterPoint");
                        if (centerProp != null)
                        {
                            return centerProp.GetValue(circle) as Point;
                        }
                    }
                    catch { }
                }
                else if (obj is Rectangle rect)
                {
                    // 尝试使用反射获取矩形的角点
                    try 
                    {
                        PropertyInfo cornerPointsProp = rect.GetType().GetProperty("CornerPoints");
                        if (cornerPointsProp != null)
                        {
                            var cornerPoints = cornerPointsProp.GetValue(rect) as ArrayList;
                            if (cornerPoints != null && cornerPoints.Count >= 4)
                            {
                                Point p0 = cornerPoints[0] as Point;
                                Point p2 = cornerPoints[2] as Point;
                                
                                if (p0 != null && p2 != null)
                                {
                                    return new Point(
                                        (p0.X + p2.X) / 2,
                                        (p0.Y + p2.Y) / 2,
                                        0
                                    );
                                }
                            }
                        }
                    }
                    catch { }
                }
                else if (obj is Polygon polygon)
                {
                    // 尝试获取多边形的点集合
                    try 
                    {
                        PropertyInfo pointsProp = polygon.GetType().GetProperty("Points");
                        if (pointsProp != null)
                        {
                            var points = pointsProp.GetValue(polygon) as ArrayList;
                            if (points != null && points.Count > 0)
                            {
                                double sumX = 0;
                                double sumY = 0;
                                
                                foreach (Point point in points)
                                {
                                    sumX += point.X;
                                    sumY += point.Y;
                                }
                                
                                return new Point(sumX / points.Count, sumY / points.Count, 0);
                            }
                        }
                    }
                    catch { }
                }
                else if (obj is Grid grid)
                {
                    // 对于轴网，使用反射获取标签位置
                    try
                    {
                        PropertyInfo labelPosProp = grid.GetType().GetProperty("LabelPosition");
                        if (labelPosProp != null)
                        {
                            var labelPos = labelPosProp.GetValue(grid) as Point;
                            if (labelPos != null)
                            {
                                return labelPos;
                            }
                        }
                        
                        // 如果没有LabelPosition属性，尝试获取轴网线的位置
                        PropertyInfo lineProp = grid.GetType().GetProperty("GridLine");
                        if (lineProp != null)
                        {
                            var gridLine = lineProp.GetValue(grid) as Line;
                            if (gridLine != null)
                            {
                                return new Point(
                                    (gridLine.StartPoint.X + gridLine.EndPoint.X) / 2,
                                    (gridLine.StartPoint.Y + gridLine.EndPoint.Y) / 2,
                                    0
                                );
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"获取轴网位置时出错: {ex.Message}");
                    }
                }
                else if (obj is ModelObject modelObj)
                {
                    // 尝试使用反射获取模型对象的外框
                    try
                    {
                        PropertyInfo outlineProp = modelObj.GetType().GetProperty("BoundingBox");
                        if (outlineProp != null)
                        {
                            var outline = outlineProp.GetValue(modelObj);
                            PropertyInfo minPointProp = outline.GetType().GetProperty("MinPoint");
                            PropertyInfo maxPointProp = outline.GetType().GetProperty("MaxPoint");
                            
                            if (minPointProp != null && maxPointProp != null)
                            {
                                Point minPoint = minPointProp.GetValue(outline) as Point;
                                Point maxPoint = maxPointProp.GetValue(outline) as Point;
                                
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
                    }
                    catch { }
                }
                
                // 尝试获取对象的最小最大点
                try
                {
                    MethodInfo minMaxPointsMethod = obj.GetType().GetMethod("GetMinMaxPoints");
                    if (minMaxPointsMethod != null)
                    {
                        var minMaxPoints = minMaxPointsMethod.Invoke(obj, null) as ArrayList;
                        if (minMaxPoints != null && minMaxPoints.Count >= 2)
                        {
                            Point minPoint = minMaxPoints[0] as Point;
                            Point maxPoint = minMaxPoints[1] as Point;
                            
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
                }
                catch (Exception ex)
                {
                    Logger.Log($"计算对象中心点时出错: {ex.Message}");
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
                
                // 获取所有视图并检查对象是否在视图中
                DrawingObjectEnumerator views = activeDrawing.GetSheet().GetAllViews();
                while (views.MoveNext())
                {
                    if (views.Current is TeklaDrawing.ViewBase viewBase)
                    {
                        try
                        {
                            // 使用反射检查对象是否在视图中
                            MethodInfo isObjectInViewMethod = viewBase.GetType().GetMethod("IsObjectInView");
                            if (isObjectInViewMethod != null)
                            {
                                bool isInView = (bool)isObjectInViewMethod.Invoke(viewBase, new object[] { drawingObject });
                                if (isInView)
                                {
                                    return viewBase;
                                }
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
        /// 创建尺寸标注线组
        /// </summary>
        private static void CreateDimensionLineSet(List<Point> points, Point dimensionLinePoint, bool isHorizontal, double textHeight, TeklaDrawing.ViewBase view)
        {
            if (points.Count < 2)
            {
                Logger.Log("点数不足，无法创建尺寸标注。");
                return;
            }

            try
            {
                // 创建尺寸标注线
                PointList pointList = new PointList();
                foreach (var point in points)
                {
                    pointList.Add(point);
                }
                
                // 确定尺寸线的位置点（偏移一定距离）
                Point dimensionPosition = new Point(dimensionLinePoint);
                double offset = 50; // 偏移量（单位：mm）
                
                // 基于所选对象确定放置尺寸线的位置
                if (isHorizontal)
                {
                    // 水平尺寸线，在Y方向上偏移
                    Point firstPoint = points.First();
                    Point lastPoint = points.Last();
                    
                    if (dimensionPosition.Y < (firstPoint.Y + lastPoint.Y) / 2)
                    {
                        // 尺寸线位于对象下方
                        double minY = points.Min(p => p.Y);
                        dimensionPosition.Y = minY - offset;
                    }
                    else
                    {
                        // 尺寸线位于对象上方
                        double maxY = points.Max(p => p.Y);
                        dimensionPosition.Y = maxY + offset;
                    }
                    
                    // 确保X坐标在范围内
                    dimensionPosition.X = (points.First().X + points.Last().X) / 2;
                }
                else
                {
                    // 垂直尺寸线，在X方向上偏移
                    Point firstPoint = points.First();
                    Point lastPoint = points.Last();
                    
                    if (dimensionPosition.X < (firstPoint.X + lastPoint.X) / 2)
                    {
                        // 尺寸线位于对象左侧
                        double minX = points.Min(p => p.X);
                        dimensionPosition.X = minX - offset;
                    }
                    else
                    {
                        // 尺寸线位于对象右侧
                        double maxX = points.Max(p => p.X);
                        dimensionPosition.X = maxX + offset;
                    }
                    
                    // 确保Y坐标在范围内
                    dimensionPosition.Y = (points.First().Y + points.Last().Y) / 2;
                }
                
                // 回退到手动创建尺寸线（因为DistanceSet可能不可用）
                CreateFallbackDimension(points, dimensionPosition, isHorizontal, textHeight, view);
            }
            catch (Exception ex)
            {
                Logger.LogError("创建尺寸标注组出错", ex);
                MessageBox.Show($"创建尺寸标注组出错: {ex.Message}");
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
