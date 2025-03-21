using System;
using System.Collections.Generic;
using System.Windows;
using Tekla.Structures.Drawing;
using Microsoft.Win32;
using System.IO;

namespace SEI_Quick_Dim
{
    public partial class LevelMarkSettingsWindow : Window
    {
        // 基本设置
        public bool UseTwoPoints { get; private set; } = true;
        public bool IsLeftPosition { get; private set; } = true;
        public double OffsetDistance { get; private set; } = 300.0;
        public double VerticalOffset { get; private set; } = 200.0;
        public double FontHeight { get; private set; } = 2.5;
        public DrawingColors TextColor { get; private set; } = DrawingColors.Black;

        // 前缀和后缀列表
        public List<string> Prefixes { get; private set; } = new List<string>();
        public List<string> Postfixes { get; private set; } = new List<string>();

        // 默认设置文件路径（保存在UI中）
        private string _currentSettingsFilePath;

        // 添加一个事件，当应用设置时触发
        public event EventHandler SettingsApplied;

        public LevelMarkSettingsWindow()
        {
            InitializeComponent();

            // 加载默认设置
            LoadDefaultSettings();

            // 显示默认设置文件路径
            _currentSettingsFilePath = LevelMarkSettings.GetDefaultSettingsFilePath();
            UpdateSettingsPathDisplay();
        }

        /// <summary>
        /// 加载默认设置
        /// </summary>
        private void LoadDefaultSettings()
        {
            try
            {
                // 尝试从默认位置加载设置
                var settings = LevelMarkSettings.LoadSettings();
                ApplySettings(settings);
            }
            catch (Exception ex)
            {
                Logger.LogError("加载默认设置失败", ex);
                // 使用界面上的默认值
            }
        }

        /// <summary>
        /// 将设置应用到UI控件
        /// </summary>
        private void ApplySettings(LevelMarkSettings settings)
        {
            // 设置点数选择
            TwoPointsRadioButton.IsChecked = settings.UseTwoPoints;
            ThreePointsRadioButton.IsChecked = !settings.UseTwoPoints;

            // 设置位置
            LeftPositionRadioButton.IsChecked = settings.IsLeftPosition;
            RightPositionRadioButton.IsChecked = !settings.IsLeftPosition;

            // 设置距离和高度
            OffsetDistanceTextBox.Text = settings.OffsetDistance.ToString();
            VerticalOffsetTextBox.Text = settings.VerticalOffset.ToString();
            FontHeightTextBox.Text = settings.FontHeight.ToString();

            // 设置前缀
            if (settings.Prefixes.Count >= 1) Prefix1TextBox.Text = settings.Prefixes[0];
            if (settings.Prefixes.Count >= 2) Prefix2TextBox.Text = settings.Prefixes[1];
            if (settings.Prefixes.Count >= 3) Prefix3TextBox.Text = settings.Prefixes[2];
            if (settings.Prefixes.Count >= 4) Prefix4TextBox.Text = settings.Prefixes[3];
            if (settings.Prefixes.Count >= 5) Prefix5TextBox.Text = settings.Prefixes[4];
            if (settings.Prefixes.Count >= 6) Prefix6TextBox.Text = settings.Prefixes[5];

            // 设置后缀
            if (settings.Postfixes.Count >= 1) Postfix1TextBox.Text = settings.Postfixes[0];
            if (settings.Postfixes.Count >= 2) Postfix2TextBox.Text = settings.Postfixes[1];
            if (settings.Postfixes.Count >= 3) Postfix3TextBox.Text = settings.Postfixes[2];
            if (settings.Postfixes.Count >= 4) Postfix4TextBox.Text = settings.Postfixes[3];
            if (settings.Postfixes.Count >= 5) Postfix5TextBox.Text = settings.Postfixes[4];
            if (settings.Postfixes.Count >= 6) Postfix6TextBox.Text = settings.Postfixes[5];
        }

        /// <summary>
        /// 从UI控件获取当前设置
        /// </summary>
        private LevelMarkSettings GetCurrentSettings()
        {
            var settings = new LevelMarkSettings
            {
                UseTwoPoints = UseTwoPoints,
                IsLeftPosition = IsLeftPosition,
                OffsetDistance = OffsetDistance,
                VerticalOffset = VerticalOffset,
                FontHeight = FontHeight,
                Prefixes = new List<string>
                {
                    Prefix1TextBox.Text,
                    Prefix2TextBox.Text,
                    Prefix3TextBox.Text,
                    Prefix4TextBox.Text,
                    Prefix5TextBox.Text,
                    Prefix6TextBox.Text
                },
                Postfixes = new List<string>
                {
                    Postfix1TextBox.Text,
                    Postfix2TextBox.Text,
                    Postfix3TextBox.Text,
                    Postfix4TextBox.Text,
                    Postfix5TextBox.Text,
                    Postfix6TextBox.Text
                }
            };

            return settings;
        }

        /// <summary>
        /// 更新设置文件路径显示
        /// </summary>
        private void UpdateSettingsPathDisplay()
        {
            SettingsFilePathTextBlock.Text = $"当前设置文件: {_currentSettingsFilePath}";
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("用户点击了确定按钮");

            // 应用设置但不关闭窗口
            ApplySettings();
            
            // 触发设置应用事件
            SettingsApplied?.Invoke(this, EventArgs.Empty);
        }

        // 新增方法，用于应用设置但不关闭窗口
        public void ApplySettings()
        {
            // 更新UseTwoPoints属性，基于单选按钮选择
            UseTwoPoints = TwoPointsRadioButton.IsChecked ?? true;
            // 更新IsLeftPosition属性，基于单选按钮选择
            IsLeftPosition = LeftPositionRadioButton.IsChecked ?? true;
            
            // 更新数值属性
            if (double.TryParse(OffsetDistanceTextBox.Text, out double offsetDistance))
                OffsetDistance = offsetDistance;
            
            if (double.TryParse(VerticalOffsetTextBox.Text, out double verticalOffset))
                VerticalOffset = verticalOffset;
                
            if (double.TryParse(FontHeightTextBox.Text, out double fontHeight))
                FontHeight = fontHeight;

            // 收集前缀
            Prefixes.Clear();
            Prefixes.Add(Prefix1TextBox.Text);
            Prefixes.Add(Prefix2TextBox.Text);
            Prefixes.Add(Prefix3TextBox.Text);
            Prefixes.Add(Prefix4TextBox.Text);
            Prefixes.Add(Prefix5TextBox.Text);
            Prefixes.Add(Prefix6TextBox.Text);

            // 收集后缀
            Postfixes.Clear();
            Postfixes.Add(Postfix1TextBox.Text);
            Postfixes.Add(Postfix2TextBox.Text);
            Postfixes.Add(Postfix3TextBox.Text);
            Postfixes.Add(Postfix4TextBox.Text);
            Postfixes.Add(Postfix5TextBox.Text);
            Postfixes.Add(Postfix6TextBox.Text);

            // 自动保存当前设置
            try
            {
                var settings = GetCurrentSettings();
                settings.SaveSettings();
                Logger.Log("确定前自动保存设置成功");
            }
            catch (Exception ex)
            {
                Logger.LogError("确定前自动保存设置失败", ex);
            }
        }

        // 创建关闭窗口的方法
        public void CloseWindow()
        {
            DialogResult = true;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("用户点击了关闭按钮");
            CloseWindow();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("用户点击了取消按钮");
            DialogResult = false;
            Close();
        }

        private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settings = GetCurrentSettings();
                bool result = settings.SaveSettings();

                if (result)
                {
                    _currentSettingsFilePath = LevelMarkSettings.GetDefaultSettingsFilePath();
                    UpdateSettingsPathDisplay();
                    MessageBox.Show($"设置已成功保存到默认位置:\n{_currentSettingsFilePath}", "保存成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("保存设置失败，请查看日志了解详情。", "保存失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("保存设置失败", ex);
                MessageBox.Show($"保存设置时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settings = LevelMarkSettings.LoadSettings();
                ApplySettings(settings);

                _currentSettingsFilePath = LevelMarkSettings.GetDefaultSettingsFilePath();
                UpdateSettingsPathDisplay();

                MessageBox.Show($"设置已成功从默认位置加载:\n{_currentSettingsFilePath}", "加载成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Logger.LogError("加载设置失败", ex);
                MessageBox.Show($"加载设置时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON文件|*.json",
                Title = "保存标高标记设置",
                DefaultExt = "json",
                FileName = "LevelMarkSettings.json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var settings = GetCurrentSettings();
                    bool result = settings.SaveSettings(saveFileDialog.FileName);

                    if (result)
                    {
                        _currentSettingsFilePath = saveFileDialog.FileName;
                        UpdateSettingsPathDisplay();
                        MessageBox.Show($"设置已成功保存到:\n{_currentSettingsFilePath}", "保存成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("保存设置失败，请查看日志了解详情。", "保存失败", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("保存设置到指定文件失败", ex);
                    MessageBox.Show($"保存设置时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadFromButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON文件|*.json",
                Title = "加载标高标记设置",
                DefaultExt = "json"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var settings = LevelMarkSettings.LoadSettings(openFileDialog.FileName);
                    ApplySettings(settings);

                    _currentSettingsFilePath = openFileDialog.FileName;
                    UpdateSettingsPathDisplay();

                    MessageBox.Show($"设置已成功从以下位置加载:\n{_currentSettingsFilePath}", "加载成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Logger.LogError("从指定文件加载设置失败", ex);
                    MessageBox.Show($"加载设置时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
