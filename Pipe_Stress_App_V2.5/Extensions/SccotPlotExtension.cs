using Microsoft.Win32;
using ScottPlot;
using ScottPlot.Plottable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pipe_Stress_App_V2._5.Extensions
{
    public static class SccotPlotExtension
    {
        /// <summary>
        /// 添加辅助线
        /// </summary>
        /// <param name="wpfPlot"></param>
        /// <param name="VerticalLine"></param>
        /// <param name="HorizontalLine"></param>
        /// <param name="VPosition"></param>
        /// <param name="HPosition"></param>
        /// <param name="DragEnabled"></param>
        /// <param name="PositionLabel"></param>
        public static void AddAxisLine(this WpfPlot wpfPlot,
           bool VerticalLine = true, bool HorizontalLine = true,
           double? VPosition = null, double? HPosition = null,
           bool DragEnabled = true, bool PositionLabel = true)
        {
            if (VerticalLine)
            {
                if (VPosition == null)
                {
                    VPosition = wpfPlot.Plot.GetAxisLimits().XCenter;
                }
                var vLine = wpfPlot.Plot.AddVerticalLine((double)VPosition, style: LineStyle.Dash);
                if (DragEnabled)
                {
                    vLine.DragEnabled = true;
                }
                if (PositionLabel)
                {
                    vLine.PositionLabel = true;
                    vLine.PositionLabelBackground = vLine.Color;
                }
            }

            if (HorizontalLine)
            {
                if (HPosition == null)
                {
                    HPosition = wpfPlot.Plot.GetAxisLimits().YCenter;
                }
                var hLine = wpfPlot.Plot.AddHorizontalLine((double)HPosition, style: LineStyle.Dash);
                if (DragEnabled)
                {
                    hLine.DragEnabled = true;
                }
                if (PositionLabel)
                {
                    hLine.PositionLabel = true;
                    hLine.PositionLabelBackground = hLine.Color;
                }
            }
        }


        public static void ScreenShot(this WpfPlot wpfPlot)
        {
            var sfd = new SaveFileDialog
            {
                FileName = "ScottPlot.png",
                Filter = "PNG Files (*.png)|*.png;*.png" +
                         "|JPG Files (*.jpg, *.jpeg)|*.jpg;*.jpeg" +
                         "|BMP Files (*.bmp)|*.bmp;*.bmp" +
                         "|All files (*.*)|*.*"
            };

            if (sfd.ShowDialog() == true)
                wpfPlot.Plot.SaveFig(sfd.FileName);
        }

        /// <summary>
        /// 移除辅助线
        /// </summary>
        /// <param name="wpfPlot"></param>
        /// <param name="VerticalLine"></param>
        /// <param name="HorizontalLine"></param>
        public static void ReMoveAxisLine(this WpfPlot wpfPlot, bool VerticalLine = true, bool HorizontalLine = true)
        {
            var plt = wpfPlot.Plot.GetPlottables();
            foreach (var item in plt)
            {
                if (item is VLine)
                {
                    if (VerticalLine)
                    {
                        wpfPlot.Plot.Remove(item);
                    }
                }
                if (item is HLine)
                {
                    if (HorizontalLine)
                    {
                        wpfPlot.Plot.Remove(item);
                    }
                }
            }
           
        }


        /// <summary>
        /// 鼠标拖拽缩放功能
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="middle"></param>
        public static void MouseDraggable(this WpfPlot wpfPlot, bool left = true, bool right = true, bool middle = true, bool scrollwheel=true)
        {
            wpfPlot.Configuration.LeftClickDragPan = left;
            wpfPlot.Configuration.RightClickDragZoom = right;
            wpfPlot.Configuration.MiddleClickDragZoom = middle;
            wpfPlot.Configuration.ScrollWheelZoom= scrollwheel;
        }
    }
}
