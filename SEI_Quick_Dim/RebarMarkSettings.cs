using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Tekla.Structures.Drawing;
using Point = Tekla.Structures.Geometry3d.Point;

namespace SEI_Quick_Dim
{
    /// <summary>
    /// 批量钢筋标记设置类，用于存储钢筋标记相关设置
    /// </summary>
    public class RebarMarkSettings : INotifyPropertyChanged
    {
        private string _slabId = "";
        private string _beamSection = "";
        private string _rebarType = "";
        private string _selectedViewId = "";
        private string _viewName = "未选择视图";
        private double _textHeight = 3.5;
        private double _xBeamMin = 0;
        private double _yBeamMin = 0;
        private double _ySlabBottom = 0;
        private double _ySlabTop = 0;

        /// <summary>
        /// 板的ID
        /// </summary>
        public string SlabId
        {
            get { return _slabId; }
            set
            {
                if (_slabId != value)
                {
                    _slabId = value;
                    OnPropertyChanged("SlabId");
                }
            }
        }

        /// <summary>
        /// 梁的截面
        /// </summary>
        public string BeamSection
        {
            get { return _beamSection; }
            set
            {
                if (_beamSection != value)
                {
                    _beamSection = value;
                    OnPropertyChanged("BeamSection");
                }
            }
        }

        /// <summary>
        /// 钢筋的型号
        /// </summary>
        public string RebarType
        {
            get { return _rebarType; }
            set
            {
                if (_rebarType != value)
                {
                    _rebarType = value;
                    OnPropertyChanged("RebarType");
                }
            }
        }

        /// <summary>
        /// 选中的视图ID
        /// </summary>
        public string SelectedViewId
        {
            get { return _selectedViewId; }
            set
            {
                if (_selectedViewId != value)
                {
                    _selectedViewId = value;
                    OnPropertyChanged("SelectedViewId");
                }
            }
        }

        /// <summary>
        /// 视图名称
        /// </summary>
        public string ViewName
        {
            get { return _viewName; }
            set
            {
                if (_viewName != value)
                {
                    _viewName = value;
                    OnPropertyChanged("ViewName");
                }
            }
        }

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
        /// 梁的最小X坐标
        /// </summary>
        public double XBeamMin
        {
            get { return _xBeamMin; }
            set
            {
                if (_xBeamMin != value)
                {
                    _xBeamMin = value;
                    OnPropertyChanged("XBeamMin");
                }
            }
        }

        /// <summary>
        /// 梁的最小Y坐标
        /// </summary>
        public double YBeamMin
        {
            get { return _yBeamMin; }
            set
            {
                if (_yBeamMin != value)
                {
                    _yBeamMin = value;
                    OnPropertyChanged("YBeamMin");
                }
            }
        }

        /// <summary>
        /// 板的底部Y坐标
        /// </summary>
        public double YSlabBottom
        {
            get { return _ySlabBottom; }
            set
            {
                if (_ySlabBottom != value)
                {
                    _ySlabBottom = value;
                    OnPropertyChanged("YSlabBottom");
                }
            }
        }

        /// <summary>
        /// 板的顶部Y坐标
        /// </summary>
        public double YSlabTop
        {
            get { return _ySlabTop; }
            set
            {
                if (_ySlabTop != value)
                {
                    _ySlabTop = value;
                    OnPropertyChanged("YSlabTop");
                }
            }
        }

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

        /// <summary>
        /// 清除所有坐标数据
        /// </summary>
        public void ClearCoordinates()
        {
            XBeamMin = 0;
            YBeamMin = 0;
            YSlabBottom = 0;
            YSlabTop = 0;
        }
    }
}
