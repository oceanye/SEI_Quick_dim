using System;
using System.Windows;

namespace SEI_Quick_Dim
{
    /// <summary>
    /// ProgressWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ProgressWindow : Window
    {
        private int _totalItems;
        private int _processedItems;
        
        // Event that will be triggered when the user clicks Cancel
        public event EventHandler CancelRequested;
        
        /// <summary>
        /// 初始化进度窗口
        /// </summary>
        /// <param name="title">进度窗口标题</param>
        /// <param name="totalItems">要处理的项目总数</param>
        public ProgressWindow(string title, int totalItems)
        {
            InitializeComponent();
            
            TitleTextBlock.Text = title;
            _totalItems = totalItems;
            _processedItems = 0;
            
            // Initialize counter
            CounterTextBlock.Text = $"0/{_totalItems} 文件";
            
            // Initialize progress bar
            ProgressBar.Value = 0;
            
            // Set window title
            Title = title;
        }
        
        /// <summary>
        /// 更新进度信息
        /// </summary>
        /// <param name="percentage">完成百分比 (0-100)</param>
        /// <param name="status">当前状态文本</param>
        public void UpdateProgress(int percentage, string status)
        {
            // Update on UI thread
            Dispatcher.Invoke(() => {
                // Update progress bar
                ProgressBar.Value = percentage;
                
                // Update status text
                StatusTextBlock.Text = status;
                
                // Calculate processed items based on percentage
                _processedItems = (int)Math.Ceiling((_totalItems * percentage) / 100.0);
                CounterTextBlock.Text = $"{_processedItems}/{_totalItems} 文件";
            });
        }
        
        /// <summary>
        /// 更新已处理的项目数
        /// </summary>
        /// <param name="processed">已处理的项目数</param>
        public void UpdateItemCount(int processed)
        {
            // Update on UI thread
            Dispatcher.Invoke(() => {
                _processedItems = processed;
                CounterTextBlock.Text = $"{_processedItems}/{_totalItems} 文件";
                
                // Update progress bar too
                ProgressBar.Value = (_processedItems * 100) / _totalItems;
            });
        }
        
        /// <summary>
        /// 取消按钮点击处理
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Disable the cancel button to prevent multiple clicks
            CancelButton.IsEnabled = false;
            CancelButton.Content = "取消中...";
            
            // Trigger the cancel event
            CancelRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
