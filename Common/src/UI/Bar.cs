// TTPlugins
// Copyright (C) 2024  TTPlugins
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows;
using System.Windows.Media;
using TigerTrade.Dx;
using TigerTrade.Dx.Enums;

namespace CustomCommon.UI
{
    public class Bar : Base
    {
        private double complete;
        private double total;

        private Side side = UI.Side.Left;

        protected XBrush? fillBackgroundBrush = null;

        protected XPen? fillBorderPen = null;
        protected XBrush? fillBorderBrush = null;
        protected int fillBorderThickness = 1;
        protected XDashStyle fillBorderStyle = XDashStyle.Solid;

        protected double fillCornerRadius = 0;

        public Bar()
        {
            this.complete = 0;
            this.total = 100;
        }

        public Bar(double complete)
        {
            this.complete = complete;
            this.total = 100;
        }

        public Bar(double complete, double total)
        {
            this.complete = complete;
            this.total = total;
        }

        public override double GetBaseWidth(double parentWidth)
        {
            double baseWidth = GetPixelWidth(parentWidth);

            if (minWidthUnit != Unit.None)
                baseWidth = Math.Max(GetPixelMinWidth(parentWidth), baseWidth);
            if (maxWidthUnit != Unit.None)
                baseWidth = Math.Min(GetPixelMaxWidth(parentWidth), baseWidth);

            return baseWidth;
        }

        public override double GetBaseHeight(double parentHeight)
        {
            double baseHeight = GetPixelHeight(parentHeight);

            if (minHeightUnit != Unit.None)
                baseHeight = Math.Max(GetPixelMinHeight(parentHeight), baseHeight);
            if (maxHeightUnit != Unit.None)
                baseHeight = Math.Min(GetPixelMaxHeight(parentHeight), baseHeight);

            return baseHeight;
        }

        public override void Render(
            DxVisualQueue visual,
            double x,
            double y,
            double parentWidth,
            double parentHeight
        )
        {
            RenderBase(visual, x, y, parentWidth, parentHeight);

            Rect rect = GetInnerRect(x, y, parentWidth, parentHeight);

            if (side == UI.Side.Left)
            {
                rect.Width *= (complete / total);
            }
            else if (side == UI.Side.Top)
            {
                rect.Height *= (complete / total);
            }
            else if (side == UI.Side.Right)
            {
                rect.X += rect.Width * ((total - complete) / total);
                rect.Width *= (complete / total);
            }
            else if (side == UI.Side.Bottom)
            {
                rect.Y += rect.Height * ((total - complete) / total);
                rect.Height *= (complete / total);
            }

            if (fillCornerRadius == 0)
            {
                if (fillBackgroundBrush != null)
                    visual.FillRectangle(fillBackgroundBrush, rect);

                if (fillBorderPen != null)
                    visual.DrawRectangle(fillBorderPen, rect);
            }
            else
            {
                Point fillRadius = new Point(fillCornerRadius, fillCornerRadius);

                if (fillBackgroundBrush != null)
                    visual.FillRoundedRectangle(fillBackgroundBrush, rect, fillRadius);

                if (fillBorderPen != null)
                    visual.DrawRoundedRectangle(fillBorderPen, rect, fillRadius);
            }
        }

        public double GetComplete()
        {
            return this.complete;
        }

        public Bar SetComplete(double complete)
        {
            this.complete = complete;
            return this;
        }

        public double GetTotal()
        {
            return this.total;
        }

        public Bar SetTotal(double total)
        {
            this.total = total;
            return this;
        }

        public Bar Clear()
        {
            this.complete = 0;
            return this;
        }

        public Bar Add(double value)
        {
            this.complete += value;
            return this;
        }

        public Bar Remove(double value)
        {
            this.complete -= value;
            return this;
        }

        public Side GetSide()
        {
            return this.side;
        }

        public Bar Side(Side side)
        {
            this.side = side;
            return this;
        }

        public XBrush? GetFillBackground()
        {
            return fillBackgroundBrush;
        }

        public Bar FillBackground(Color color)
        {
            fillBackgroundBrush = new XBrush(color);
            return this;
        }

        public XPen? GetFillBorder()
        {
            return fillBorderPen;
        }

        public XBrush? GetFillBorderBrush()
        {
            return fillBorderBrush;
        }

        public int GetFillBorderThickness()
        {
            return fillBorderThickness;
        }

        public XDashStyle GetFillBorderStyle()
        {
            return fillBorderStyle;
        }

        public Bar FillBorder(Color color)
        {
            fillBorderBrush = new XBrush(color);
            fillBorderPen = new XPen(fillBorderBrush, fillBorderThickness, fillBorderStyle);
            return this;
        }

        public Bar FillBorder(int thickness)
        {
            fillBorderThickness = thickness;
            fillBorderPen = new XPen(fillBorderBrush, fillBorderThickness, fillBorderStyle);
            return this;
        }

        public Bar FillBorder(XDashStyle style)
        {
            fillBorderStyle = style;
            fillBorderPen = new XPen(fillBorderBrush, fillBorderThickness, fillBorderStyle);
            return this;
        }

        public Bar FillBorder(Color color, int thickness)
        {
            fillBorderBrush = new XBrush(color);
            fillBorderThickness = thickness;
            fillBorderPen = new XPen(fillBorderBrush, fillBorderThickness, fillBorderStyle);
            return this;
        }

        public Bar FillBorder(Color color, int thickness, XDashStyle style)
        {
            fillBorderBrush = new XBrush(color);
            fillBorderThickness = thickness;
            fillBorderStyle = style;
            fillBorderPen = new XPen(fillBorderBrush, fillBorderThickness, fillBorderStyle);
            return this;
        }

        public double GetFillCornerRadius()
        {
            return this.fillCornerRadius;
        }

        public Bar FillCornerRadius(double radius)
        {
            this.fillCornerRadius = radius;
            return this;
        }
    }
}
