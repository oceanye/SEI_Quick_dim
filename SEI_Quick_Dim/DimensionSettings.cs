using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Point = Tekla.Structures.Geometry3d.Point;

namespace SEI_Quick_Dim
{
    /// <summary>
    /// 尺寸标注设置类，用于存储尺寸标注相关设置
    /// </summary>
    public class DimensionSettings : INotifyPropertyChanged
    {
        private double _textHeight = 3.5;
        private string _selectedObjectsInfo = "未选择任何对象";
        private int _selectedObjectsCount = 0;
        private string _centerPointsInfo = "尚未计算中心点";

        /// <summary>
        /// 尺寸标注文本高度
        /// </summary>
        public double TextHeight
        {
            get { return _textHeight; }
            set
            {
                if (_textHeight != value)
                {
                    _textHeight = value;
                    OnPropertyChanged("TextHeight");
                }
            }
        }

        /// <summary>
        /// 已选择对象的信息
        /// </summary>
        public string SelectedObjectsInfo
        {
            get { return _selectedObjectsInfo; }
            set
            {
                if (_selectedObjectsInfo != value)
                {
                    _selectedObjectsInfo = value;
                    OnPropertyChanged("SelectedObjectsInfo");
                }
            }
        }

        /// <summary>
        /// 已选择对象的数量
        /// </summary>
        public int SelectedObjectsCount
        {
            get { return _selectedObjectsCount; }
            set
            {
                if (_selectedObjectsCount != value)
                {
                    _selectedObjectsCount = value;
                    OnPropertyChanged("SelectedObjectsCount");
                }
            }
        }

        /// <summary>
        /// 中心点信息
        /// </summary>
        public string CenterPointsInfo
        {
            get { return _centerPointsInfo; }
            set
            {
                if (_centerPointsInfo != value)
                {
                    _centerPointsInfo = value;
                    OnPropertyChanged("CenterPointsInfo");
                }
            }
        }

        /// <summary>
        /// 存储计算的中心点
        /// </summary>
        public List<Point> CenterPoints { get; set; } = new List<Point>();

        /// <summary>
        /// 属性变更事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 触发属性变更事件
        /// </summary>
        /// <param name="propertyName">变更的属性名</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
