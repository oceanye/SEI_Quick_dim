# SEI_Quick_Dim - Tekla结构快速标注工具

SEI_Quick_Dim是一个基于Tekla Structures API的绘图标注工具，旨在简化Tekla结构绘图中的标注工作。

## 功能特性

该应用程序提供以下功能：

1. **快速标高标记** - 允许用户在绘图中创建标高标记，可设置垂直偏移、字体高度、前缀和后缀等参数
2. **快速尺寸标注** - 允许用户通过选择起点和终点快速创建尺寸标注
3. **批量钢筋标记** - 允许用户选择并标记多个钢筋对象
4. **Tekla连接检查** - 提供详细的Tekla连接状态信息，包括模型和绘图连接状态

## 技术细节

- 开发语言：C#
- 框架：.NET Framework 4.8
- 用户界面：WPF (Windows Presentation Foundation)
- 依赖：Tekla Structures API (2024版本)

## 日志系统

应用程序内置了全面的日志系统，可记录程序执行过程中的各种信息：

- API连接状态
- 用户操作
- 错误处理
- 对象创建和修改

日志文件位于应用程序目录下的`SEI_Quick_Dim_Log.txt`。

## 如何使用

1. 启动Tekla Structures并打开一个模型
2. 打开需要标注的绘图
3. 运行SEI_Quick_Dim应用程序
4. 选择所需的功能按钮
5. 根据提示进行操作

## 注意事项

- 使用前请确保Tekla Structures已正常启动并打开了一个绘图
- 本应用程序经过简化，以确保与Tekla Structures 2024 API的兼容性
- 所有操作都有详细的日志记录，如遇问题请查看日志文件

## 版本历史

- v1.0 - 基础版本，包含连接检查和基础标注功能框架
