using System;
using System.IO;
using System.Text;
using System.Threading;

namespace SEI_Quick_Dim
{
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static string _logFilePath;

        static Logger()
        {
            // 设置日志文件路径，放在应用程序同一目录下
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _logFilePath = Path.Combine(appDirectory, "SEI_Quick_Dim_Log.txt");
            
            // 初始化日志文件
            try
            {
                // 创建或清空日志文件
                File.WriteAllText(_logFilePath, $"=== SEI_Quick_Dim 日志开始 {DateTime.Now} ===\r\n");
                Log("日志系统初始化成功");
            }
            catch (Exception ex)
            {
                // 无法写入日志文件时，无法记录日志
                Console.WriteLine($"无法初始化日志文件: {ex.Message}");
            }
        }

        public static string LogFilePath => _logFilePath;
        
        // 添加额外的方法以便与使用 GetLogFilePath 的代码兼容
        public static string GetLogFilePath()
        {
            return _logFilePath;
        }

        public static void Log(string message)
        {
            try
            {
                lock (_lock)
                {
                    // 将日志添加到文件
                    using (StreamWriter writer = new StreamWriter(_logFilePath, true, Encoding.UTF8))
                    {
                        string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
                        writer.WriteLine(logMessage);
                        writer.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                // 如果记录日志时出错，记录到控制台
                Console.WriteLine($"写入日志出错: {ex.Message}");
            }
        }

        public static void LogError(string message, Exception exception)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"错误: {message}");
            sb.AppendLine($"异常类型: {exception.GetType().Name}");
            sb.AppendLine($"异常消息: {exception.Message}");
            sb.AppendLine($"堆栈跟踪: {exception.StackTrace}");

            // 记录内部异常
            if (exception.InnerException != null)
            {
                sb.AppendLine("内部异常:");
                sb.AppendLine($"  类型: {exception.InnerException.GetType().Name}");
                sb.AppendLine($"  消息: {exception.InnerException.Message}");
                sb.AppendLine($"  堆栈: {exception.InnerException.StackTrace}");
            }

            Log(sb.ToString());
        }
    }
}
