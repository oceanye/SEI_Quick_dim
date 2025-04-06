using System;
using System.Collections.Generic;
using System.Windows;
using Tekla.Structures.Drawing;
using Tekla.Structures.Drawing.UI;
using Point = Tekla.Structures.Geometry3d.Point;

namespace SEI_Quick_Dim
{
    /// <summary>
    /// RebarMarkSettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class RebarMarkSettingsWindow : Window
    {
        private RebarMarkSettings _settings;
        private DrawingHandler _drawingHandler;
        private Picker _picker;
        private ViewBase _selectedView;

        /// <summary>
        /// 构造函数
        /// </summary>
        public RebarMarkSettingsWindow(ViewBase selectedView)
        {
            InitializeComponent();
            _settings = new RebarMarkSettings();
            DataContext = _settings;
            
            _drawingHandler = new DrawingHandler();
            _picker = _drawingHandler.GetPicker();
            _selectedView = selectedView;
            
            // 设置视图信息
            if (_selectedView != null)
            {
                // 使用反射获取视图标识符
                try
                {
                    // 尝试通过反射获取视图标识符
                    var idProperty = _selectedView.GetType().GetProperty("Identifier");
                    if (idProperty != null)
                    {
                        object idValue = idProperty.GetValue(_selectedView);
                        if (idValue != null)
                        {
                            _settings.SelectedViewId = idValue.ToString();
                        }
                        else
                        {
                            _settings.SelectedViewId = "未知ID";
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("获取视图标识符时出错", ex);
                    _settings.SelectedViewId = "未知ID";
                }

                // 自动分析视图中的对象
                AnalyzeView();
            }
        }

        /// <summary>
        /// 获取视图名称
        /// </summary>
        private string GetViewName(ViewBase view)
        {
            try
            {
                if (view == null)
                {
                    return "未知视图";
                }

                // 尝试获取视图名称
                var nameProperty = view.GetType().GetProperty("Name");
                if (nameProperty != null)
                {
                    var name = nameProperty.GetValue(view)?.ToString();
                    if (!string.IsNullOrEmpty(name))
                    {
                        return name;
                    }
                }

                // 获取视图类型
                string viewType = view.GetType().Name;
                
                // 如果无法获取名称，返回视图类型和标识符
                return $"{viewType} ID: {view.GetHashCode()}";
            }
            catch (Exception ex)
            {
                Logger.LogError("获取视图名称时出错", ex);
                return $"视图 ID: {view.GetHashCode()}";
            }
        }

        /// <summary>
        /// 分析视图中的对象并更新边界点
        /// </summary>
        private void AnalyzeView()
        {
            try
            {
                if (_selectedView == null)
                {
                    MessageBox.Show("没有选中的视图", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var boundaryPoints = DrawingToolsExtensions.GetBoundaryPointsFromView(_selectedView);
                if (boundaryPoints.IsValid)
                {
                    _settings.XBeamMin = boundaryPoints.BeamBoundary.X;
                    _settings.YBeamMin = boundaryPoints.BeamBoundary.Y;
                    _settings.YSlabBottom = boundaryPoints.SlabBottomY;
                    _settings.YSlabTop = boundaryPoints.SlabTopY;
                    
                    Logger.Log("已分析视图并更新边界点");
                    MessageBox.Show("已成功分析视图并更新边界点。", 
                        "分析完成", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("在视图中没有找到有效的对象", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("分析视图时出错", ex);
                MessageBox.Show($"分析视图时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 分析视图按钮点击事件
        /// </summary>
        private void AnalyzeViewButton_Click(object sender, RoutedEventArgs e)
        {
            AnalyzeView();
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
        /// 获取当前设置
        /// </summary>
        public RebarMarkSettings GetSettings()
        {
            return _settings;
        }

        /// <summary>
        /// 获取选中的视图
        /// </summary>
        public ViewBase GetSelectedView()
        {
            return _selectedView;
        }
    }
}
