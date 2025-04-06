using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using TeklaDrawing = Tekla.Structures.Drawing;
using Tekla.Structures.Drawing;
using Tekla.Structures.Drawing.Tools;
using Tekla.Structures.Drawing.UI;
using TeklaModel = Tekla.Structures.Model;
using Tekla.Structures.Geometry3d;
using System.Windows.Markup;
using Tekla.Structures;
using Tekla.Structures.Dialog;
using Point = Tekla.Structures.Geometry3d.Point;
using Vector = Tekla.Structures.Geometry3d.Vector;
using WinForms = System.Windows.Forms;
using MessageBox = System.Windows.MessageBox;

namespace SEI_Quick_Dim
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Logger.Log("应用程序启动");
            
            // 在UI上显示日志文件路径
            Title = $"SEI_Quick_Dim - 日志文件: {Logger.LogFilePath}";
            
            // 在启动时检查Tekla连接状态
            CheckTeklaConnection();
        }
        
        private void CheckTeklaConnection()
        {
            try
            {
                Logger.Log("检查Tekla连接状态...");
                
                // 测试模型连接
                try
                {
                    Logger.Log("尝试连接到 Tekla Structures 模型...");
                    var model = new TeklaModel.Model();
                    if (model.GetConnectionStatus())
                    {
                        Logger.Log("成功连接到 Tekla Structures 模型");
                        StatusTextBlock.Text = "已连接到 Tekla Structures 模型";
                    }
                    else
                    {
                        Logger.Log("无法连接到 Tekla Structures 模型");
                        StatusTextBlock.Text = "未连接到 Tekla Structures 模型";
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("连接模型时发生错误", ex);
                    StatusTextBlock.Text = "连接模型时发生错误";
                }
                
                // 检查绘图连接
                TeklaDrawing.DrawingHandler drawingHandler = new TeklaDrawing.DrawingHandler();
                bool drawingConnected = drawingHandler.GetConnectionStatus();
                Logger.Log($"Tekla绘图连接状态: {(drawingConnected ? "已连接" : "未连接")}");
                
                if (drawingConnected)
                {
                    TeklaDrawing.Drawing activeDrawing = drawingHandler.GetActiveDrawing();
                    Logger.Log($"当前绘图: {(activeDrawing != null ? "已打开" : "未打开")}");
                    
                    if (activeDrawing != null)
                    {
                        Logger.Log($"绘图名称: {activeDrawing.Name}");
                        Logger.Log($"绘图类型: {activeDrawing.GetType().Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("检查Tekla连接状态时出错", ex);
            }
        }

        private void QuickLevelMarkButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("QuickLevelMarkButton_Click 被调用");
            StatusTextBlock.Text = "正在创建水平标高标记...";
            try
            {
                LevelMarkHandler.CreateLevelMark();
                StatusTextBlock.Text = "水平标高标记功能执行完成";
            }
            catch (Exception ex)
            {
                Logger.LogError("水平标高标记操作失败", ex);
                StatusTextBlock.Text = "水平标高标记操作失败";
                MessageBox.Show($"操作出错，详细信息请查看日志文件。\n\n日志文件位置: {Logger.LogFilePath}", 
                               "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void QuickDimensionButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("QuickDimensionButton_Click 被调用");
            StatusTextBlock.Text = "正在创建快速尺寸标注...";
            try
            {
                DimensionHandler.CreateDimension();
                StatusTextBlock.Text = "快速尺寸标注功能执行完成";
            }
            catch (Exception ex)
            {
                Logger.LogError("快速尺寸标注操作失败", ex);
                StatusTextBlock.Text = "快速尺寸标注操作失败";
                MessageBox.Show($"操作出错，详细信息请查看日志文件。\n\n日志文件位置: {Logger.LogFilePath}", 
                               "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BatchRebarMarkButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("BatchRebarMarkButton_Click 被调用");
            StatusTextBlock.Text = "正在创建批量钢筋标注...";
            try
            {
                RebarMarkHandler.CreateRebarMarks();
                StatusTextBlock.Text = "批量钢筋标注功能执行完成";
            }
            catch (Exception ex)
            {
                Logger.LogError("批量钢筋标注操作失败", ex);
                StatusTextBlock.Text = "批量钢筋标注操作失败";
                MessageBox.Show($"操作出错，详细信息请查看日志文件。\n\n日志文件位置: {Logger.LogFilePath}", 
                               "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void CheckConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("执行 CheckConnectionButton_Click");
            try
            {
                // 同时检查模型和绘图连接
                Logger.Log("检查Tekla连接状态...");
                var model = new TeklaModel.Model();
                TeklaDrawing.DrawingHandler drawingHandler = new TeklaDrawing.DrawingHandler();
                
                string statusMessage = "";
                bool modelConnected = model.GetConnectionStatus();
                bool drawingConnected = drawingHandler.GetConnectionStatus();
                
                statusMessage += $"模型连接: {(modelConnected ? "已连接" : "未连接")}\n";
                statusMessage += $"绘图连接: {(drawingConnected ? "已连接" : "未连接")}\n\n";
                
                Logger.Log($"Tekla模型连接状态: {(modelConnected ? "已连接" : "未连接")}");
                Logger.Log($"Tekla绘图连接状态: {(drawingConnected ? "已连接" : "未连接")}");
                
                // 检查版本
                string version = "未知";
                try
                {
                    version = model.GetInfo().ModelName; // 使用 ModelName 属性
                    statusMessage += $"Tekla版本: {version}\n";
                    Logger.Log($"Tekla版本: {version}");
                }
                catch (Exception vex)
                {
                    Logger.LogError("获取Tekla版本失败", vex);
                    statusMessage += "Tekla版本: 获取失败\n";
                }
                
                if (drawingConnected)
                {
                    TeklaDrawing.Drawing activeDrawing = drawingHandler.GetActiveDrawing();
                    if (activeDrawing != null)
                    {
                        Logger.Log($"当前绘图: {activeDrawing.Name}");
                        statusMessage += $"当前已打开绘图: {activeDrawing.Name}\n";
                        
                        // 尝试获取绘图类型和其他信息
                        Logger.Log($"绘图类型: {activeDrawing.GetType().Name}");
                        statusMessage += $"绘图类型: {activeDrawing.GetType().Name}\n";
                        
                        // 尝试获取图纸信息
                        try
                        {
                            var sheet = activeDrawing.GetSheet();
                            if (sheet != null)
                            {
                                Logger.Log($"成功获取绘图Sheet: {sheet.GetType().Name}");
                                statusMessage += $"绘图Sheet类型: {sheet.GetType().Name}\n";
                                
                                DrawingObjectEnumerator views = sheet.GetViews();
                                int viewCount = 0;
                                while (views.MoveNext())
                                {
                                    viewCount++;
                                    TeklaDrawing.ViewBase view = views.Current as TeklaDrawing.ViewBase;
                                    if (view != null)
                                    {
                                        Logger.Log($"视图类型: {view.GetType().Name}");
                                        statusMessage += $"视图类型: {view.GetType().Name}\n";
                                    }
                                }
                                Logger.Log($"绘图中共有 {viewCount} 个视图");
                                statusMessage += $"绘图中共有 {viewCount} 个视图\n";
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("获取图纸信息时出错", ex);
                        }
                    }
                    else
                    {
                        Logger.Log("未打开任何绘图");
                        statusMessage += "未打开任何绘图。";
                    }
                }
                else
                {
                    Logger.Log("未连接到Tekla绘图");
                    statusMessage += "未连接到Tekla绘图。";
                }
                
                MessageBox.Show(statusMessage + "\n\n详细信息请查看日志文件。\n" +
                               $"日志文件位置: {Logger.LogFilePath}", 
                               "Tekla连接状态", MessageBoxButton.OK, MessageBoxImage.Information);
                Logger.Log("显示Tekla连接状态消息");
            }
            catch (Exception ex)
            {
                Logger.LogError("检查连接状态时出错", ex);
                MessageBox.Show("检查连接状态时出错，详细信息请查看日志文件。\n\n" +
                               $"日志文件位置: {Logger.LogFilePath}", 
                               "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void BatchExportDwgButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Log("BatchExportDwgButton_Click 被调用");
            StatusTextBlock.Text = "选择包含Tekla项目的文件夹...";

            var folderDialog = new WinForms.FolderBrowserDialog();
            folderDialog.Description = "选择包含Tekla项目的文件夹";
            
            if (folderDialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                string selectedPath = folderDialog.SelectedPath;
                Logger.Log($"选择的文件夹路径: {selectedPath}");
                
                try
                {
                    await Task.Run(() => ProcessTeklaProjects(selectedPath));
                }
                catch (Exception ex)
                {
                    Logger.LogError("批量导出DWG时发生错误", ex);
                    StatusTextBlock.Text = "批量导出DWG时发生错误";
                    MessageBox.Show($"处理过程中发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ProcessTeklaProjects(string rootPath)
        {
            // 查找所有的 .db1 文件（Tekla模型文件）
            var modelFiles = Directory.GetFiles(rootPath, "*.db1", SearchOption.AllDirectories);
            
            foreach (var modelFile in modelFiles)
            {
                try
                {
                    string modelFolder = Path.GetDirectoryName(modelFile);
                    Logger.Log($"处理模型: {modelFolder}");
                    
                    // 更新UI状态
                    Dispatcher.Invoke(() => StatusTextBlock.Text = $"正在处理: {Path.GetFileName(modelFolder)}");

                    // 打开模型
                    var model = new TeklaModel.Model();
                    if (!model.GetConnectionStatus())
                    {
                        Logger.Log($"无法打开模型: {modelFolder}");
                        continue;
                    }

                    // 等待模型加载完成
                    Thread.Sleep(5000);

                    // 创建绘图处理器
                    var drawingHandler = new TeklaDrawing.DrawingHandler();
                    
                    // 获取所有绘图
                    TeklaDrawing.DrawingEnumerator drawings = drawingHandler.GetDrawings();
                    
                    // 创建导出文件夹
                    string exportFolder = Path.Combine(modelFolder, "DWG_Export");
                    Directory.CreateDirectory(exportFolder);

                    while (drawings.MoveNext())
                    {
                        var drawing = drawings.Current;
                        if (drawing != null)
                        {
                            string dwgPath = Path.Combine(exportFolder, $"{drawing.Name}.dwg");
                            Logger.Log($"导出绘图: {drawing.Name} 到 {dwgPath}");

                            try
                            {
                                // 打开绘图
                                drawingHandler.SetActiveDrawing(drawing);
                                
                                // 使用DrawingHandler导出DWG
                                var drawingObject = drawingHandler.GetActiveDrawing();
                                if (drawingObject != null)
                                {
                                    // 设置导出选项
                                    var fileName = Path.GetFileName(dwgPath);
                                    var dirName = Path.GetDirectoryName(dwgPath);
                                    
                                    // 创建DWG导出目录
                                    Directory.CreateDirectory(dirName);
                                    
                                    // 使用DrawingItem导出DWG
                                    var drawingItem = new Tekla.Structures.Catalogs.DrawingItem();
                                    if (drawingItem.Select(drawing.Name))
                                    {
                                        string exportPath = dwgPath;
                                        if (drawingItem.Export(ref exportPath))
                                        {
                                            Logger.Log($"成功导出: {dwgPath}");
                                        }
                                        else
                                        {
                                            Logger.Log($"导出失败: {dwgPath}");
                                            
                                            // 尝试在可能的位置查找文件
                                            var possiblePaths = new[]
                                            {
                                                Path.Combine(modelFolder, $"{drawing.Name}.dwg"),
                                                Path.Combine(modelFolder, "drawings", $"{drawing.Name}.dwg"),
                                                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Tekla Structures", "drawings", $"{drawing.Name}.dwg"),
                                                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Tekla Structures", $"{drawing.Name}.dwg"),
                                                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), $"{drawing.Name}.dwg"),
                                                Path.Combine(modelFolder, "..", "drawings", $"{drawing.Name}.dwg"),
                                                Path.Combine(modelFolder, "..", "..", "drawings", $"{drawing.Name}.dwg"),
                                                Path.Combine(modelFolder, "..", "..", $"{drawing.Name}.dwg"),
                                                exportPath
                                            };
                                            
                                            // 记录所有可能的路径
                                            Logger.Log($"搜索DWG文件路径: {string.Join(Environment.NewLine, possiblePaths)}");
                                            
                                            var sourceFile = possiblePaths.FirstOrDefault(File.Exists);
                                            if (sourceFile != null)
                                            {
                                                File.Copy(sourceFile, dwgPath, true);
                                                Logger.Log($"找到并复制文件从: {sourceFile}");
                                            }
                                            else
                                            {
                                                Logger.Log($"未找到源文件");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Logger.Log($"无法选择绘图: {drawing.Name}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.LogError($"导出绘图 {drawing.Name} 时发生错误", ex);
                            }
                        }
                    }

                    // 关闭模型
                    model.CommitChanges();
                }
                catch (Exception ex)
                {
                    Logger.LogError($"处理模型文件 {modelFile} 时发生错误", ex);
                }
            }

            // 完成处理
            Dispatcher.Invoke(() => 
            {
                StatusTextBlock.Text = "批量导出DWG完成";
                MessageBox.Show("批量导出DWG完成！", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        private void LogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string logFilePath = Logger.GetLogFilePath();
                if (File.Exists(logFilePath))
                {
                    // 使用默认程序打开日志文件
                    ProcessStartInfo startInfo = new ProcessStartInfo
                    {
                        FileName = logFilePath,
                        UseShellExecute = true
                    };
                    Process.Start(startInfo);
                    StatusTextBlock.Text = $"已打开日志文件: {logFilePath}";
                }
                else
                {
                    MessageBox.Show($"日志文件不存在: {logFilePath}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"打开日志文件失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}