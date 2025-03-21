using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tekla.Structures.Drawing;
using Tekla.Structures.Drawing.UI;
using Tekla.Structures.Geometry3d;
using TeklaDrawing = Tekla.Structures.Drawing;

namespace SEI_Quick_Dim
{
    /// <summary>
    /// 提供绘图工具的扩展方法
    /// </summary>
    public static class DrawingToolsExtensions
    {
        /// <summary>
        /// 用于存储picker选择点和视图的结果
        /// </summary>
        public class PickedPoint
        {
            public Point Point { get; set; }
            public TeklaDrawing.ViewBase ViewBase { get; set; }

            public PickedPoint(Point point, TeklaDrawing.ViewBase viewBase)
            {
                Point = point;
                ViewBase = viewBase;
            }
        }

        /// <summary>
        /// 选择一个点并返回点和所在视图
        /// </summary>
        public static PickedPoint PickPointWithView(this Picker picker, string customPrompt = null)
        {
            Logger.Log("尝试使用Picker选择点");
            try
            {
                // 在Tekla界面上直接显示提示文字
                string prompt = customPrompt ?? "请在绘图中点击以选择点";
                var result = picker.PickPoint(prompt);
                if (result != null)
                {
                    return new PickedPoint(result.Item1, result.Item2);
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError("使用Picker选择点时出错", ex);
                return null;
            }
        }

        /// <summary>
        /// 选择一个对象
        /// </summary>
        public static DrawingObject PickObject(this Picker picker, string prompt)
        {
            Logger.Log($"尝试使用Picker选择对象: {prompt}");
            try
            {
                var result = picker.PickObject(prompt);
                if (result != null)
                {
                    return result.Item1;
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError("使用Picker选择对象时出错", ex);
                return null;
            }
        }
    }
}
