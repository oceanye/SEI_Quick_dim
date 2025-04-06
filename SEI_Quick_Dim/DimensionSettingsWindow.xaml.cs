using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Tekla.Structures.Drawing;
using Point = Tekla.Structures.Geometry3d.Point;

namespace SEI_Quick_Dim
{
    /// <summary>
    /// DimensionSettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class DimensionSettingsWindow : Window
    {
        private DimensionSettings _settings;
        private List<DrawingObject> _selectedObjects;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public DimensionSettingsWindow()
        {
            InitializeComponent();
            _settings = new DimensionSettings();
            DataContext = _settings;
        }
        
        /// <summary>
        /// 确认按钮点击事件
        /// </summary>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
        
        /// <summary>
        /// 取消按钮点击事件
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        
        /// <summary>
        /// 设置选中的对象并计算中心点
        /// </summary>
        /// <param name="objects">选中的对象集合</param>
        public void SetSelectedObjects(List<DrawingObject> objects)
        {
            _selectedObjects = objects;
            _settings.SelectedObjectsCount = objects.Count;
            
            StringBuilder sb = new StringBuilder();
            Dictionary<string, int> objectTypes = new Dictionary<string, int>();
            
            // 统计对象类型
            foreach (var obj in objects)
            {
                string typeName = GetObjectTypeName(obj);
                if (objectTypes.ContainsKey(typeName))
                {
                    objectTypes[typeName]++;
                }
                else
                {
                    objectTypes[typeName] = 1;
                }
            }
            
            // 构建对象类型描述
            foreach (var pair in objectTypes)
            {
                sb.AppendLine($"{pair.Key}: {pair.Value} 个");
            }
            
            _settings.SelectedObjectsInfo = sb.ToString();
            
            // 计算所有对象的中心点
            CalculateCenterPoints(objects);
        }
        
        /// <summary>
        /// 计算所有对象的中心点
        /// </summary>
        private void CalculateCenterPoints(List<DrawingObject> objects)
        {
            _settings.CenterPoints.Clear();
            StringBuilder centerPointsInfo = new StringBuilder();
            
            int index = 1;
            foreach (var obj in objects)
            {
                Point centerPoint = DimensionHandler.CalculateObjectCenter(obj);
                if (centerPoint != null)
                {
                    _settings.CenterPoints.Add(centerPoint);
                    centerPointsInfo.AppendLine($"点 {index}: ({centerPoint.X:F2}, {centerPoint.Y:F2}) - {GetObjectTypeName(obj)}");
                    index++;
                }
            }
            
            if (_settings.CenterPoints.Count == 0)
            {
                _settings.CenterPointsInfo = "无法计算任何对象的中心点";
            }
            else
            {
                _settings.CenterPointsInfo = centerPointsInfo.ToString();
            }
        }
        
        /// <summary>
        /// 获取对象类型名称
        /// </summary>
        /// <param name="obj">绘图对象</param>
        /// <returns>对象类型名称</returns>
        private string GetObjectTypeName(DrawingObject obj)
        {
            if (obj is Line) return "线段";
            if (obj is Arc) return "弧线";
            if (obj is Circle) return "圆形";
            if (obj is Rectangle) return "矩形";
            if (obj is Polygon) return "多边形";
            if (obj is Symbol) return "符号";
            if (obj is Text) return "文本";
            if (obj is ReferenceModel) return "参考模型";
            if (obj is ModelObject) return "模型对象";
            if (obj is ReinforcementBase) return "钢筋";
            if (obj is Part) return "构件";
            if (obj is Bolt) return "螺栓";
            if (obj is Weld) return "焊缝";
            if (obj is Grid) return "轴网";
            
            return obj.GetType().Name;
        }
        
        /// <summary>
        /// 获取当前设置
        /// </summary>
        /// <returns>尺寸标注设置</returns>
        public DimensionSettings GetSettings()
        {
            return _settings;
        }
    }
}
