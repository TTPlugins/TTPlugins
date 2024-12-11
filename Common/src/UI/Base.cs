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

using System.Windows;
using System.Windows.Media;
using TigerTrade.Dx;
using TigerTrade.Dx.Enums;

namespace CustomCommon.UI
{
    public abstract class Base
    {
        protected double width = 0;
        protected double minWidth = 0;
        protected double maxWidth = 0;
        protected Unit widthUnit = Unit.None;
        protected Unit minWidthUnit = Unit.None;
        protected Unit maxWidthUnit = Unit.None;

        protected double height = 0;
        protected double minHeight = 0;
        protected double maxHeight = 0;
        protected Unit heightUnit = Unit.None;
        protected Unit minHeightUnit = Unit.None;
        protected Unit maxHeightUnit = Unit.None;

        protected double marginLeft = 0;
        protected double marginTop = 0;
        protected double marginRight = 0;
        protected double marginBottom = 0;

        protected double paddingLeft = 0;
        protected double paddingTop = 0;
        protected double paddingRight = 0;
        protected double paddingBottom = 0;

        protected XBrush? backgroundBrush = null;

        protected XPen? borderPen = null;
        protected XBrush? borderBrush = null;
        protected int borderThickness = 1;
        protected XDashStyle borderStyle = XDashStyle.Solid;

        protected double cornerRadius = 0;

        public abstract double GetBaseWidth(double parentWidth);
        public abstract double GetBaseHeight(double parentHeight);

        public virtual double GetBaseX(double x)
        {
            return x + marginLeft + paddingLeft;
        }

        public virtual double GetBaseY(double y)
        {
            return y + marginTop + paddingTop;
        }

        public virtual Rect GetBaseRect(double x, double y, double parentWidth, double parentHeight)
        {
            return new Rect(
                GetBaseX(x),
                GetBaseY(y),
                GetBaseWidth(parentWidth),
                GetBaseHeight(parentHeight)
            );
        }

        public virtual double GetInnerX(double x)
        {
            return x + marginLeft;
        }

        public virtual double GetInnerY(double y)
        {
            return y + marginTop;
        }

        public virtual double GetInnerWidth(double parentWidth)
        {
            return paddingLeft + GetBaseWidth(parentWidth) + paddingRight;
        }

        public virtual double GetInnerHeight(double parentHeight)
        {
            return paddingTop + GetBaseHeight(parentHeight) + paddingBottom;
        }

        public virtual Rect GetInnerRect(
            double x,
            double y,
            double parentWidth,
            double parentHeight
        )
        {
            return new Rect(
                GetInnerX(x),
                GetInnerY(y),
                GetInnerWidth(parentWidth),
                GetInnerHeight(parentHeight)
            );
        }

        public virtual double GetOuterWidth(double parentWidth)
        {
            return marginLeft + GetInnerWidth(parentWidth) + marginRight;
        }

        public virtual double GetOuterHeight(double parentHeight)
        {
            return marginTop + GetInnerHeight(parentHeight) + marginBottom;
        }

        public virtual Rect GetOuterRect(
            double x,
            double y,
            double parentWidth,
            double parentHeight
        )
        {
            return new Rect(x, y, GetOuterWidth(parentWidth), GetOuterHeight(parentHeight));
        }

        public abstract void Render(
            DxVisualQueue visual,
            double x,
            double y,
            double parentWidth,
            double parentHeight
        );

        protected virtual void RenderBase(
            DxVisualQueue visual,
            double x,
            double y,
            double parentWidth,
            double parentHeight
        )
        {
            Rect rect = new Rect(
                GetInnerX(x),
                GetInnerY(y),
                GetInnerWidth(parentWidth),
                GetInnerHeight(parentHeight)
            );

            if (cornerRadius == 0)
            {
                if (backgroundBrush != null)
                    visual.FillRectangle(backgroundBrush, rect);

                if (borderPen != null)
                    visual.DrawRectangle(borderPen, rect);
            }
            else
            {
                Point radius = new Point(cornerRadius, cornerRadius);

                if (backgroundBrush != null)
                    visual.FillRoundedRectangle(backgroundBrush, rect, radius);

                if (borderPen != null)
                    visual.DrawRoundedRectangle(borderPen, rect, radius);
            }
        }

        public double GetPixelWidth(double parentWidth)
        {
            if (widthUnit == Unit.Percent)
                return parentWidth * width / 100.0;
            else
                return width;
        }

        public double GetPixelMinWidth(double parentWidth)
        {
            if (minWidthUnit == Unit.Percent)
                return parentWidth * minWidth / 100.0;
            else
                return minWidth;
        }

        public double GetPixelMaxWidth(double parentWidth)
        {
            if (maxWidthUnit == Unit.Percent)
                return parentWidth * maxWidth / 100.0;
            else
                return maxWidth;
        }

        public double GetPixelHeight(double parentHeight)
        {
            if (heightUnit == Unit.Percent)
                return parentHeight * height / 100.0;
            else
                return height;
        }

        public double GetPixelMinHeight(double parentHeight)
        {
            if (minHeightUnit == Unit.Percent)
                return parentHeight * minHeight / 100.0;
            else
                return minHeight;
        }

        public double GetPixelMaxHeight(double parentHeight)
        {
            if (maxHeightUnit == Unit.Percent)
                return parentHeight * maxHeight / 100.0;
            else
                return maxHeight;
        }

        public double GetWidth()
        {
            return width;
        }

        public Unit GetWidthUnit()
        {
            return widthUnit;
        }

        public Base Width(double width)
        {
            this.width = width;
            return this;
        }

        public Base Width(Unit unit)
        {
            this.widthUnit = unit;
            return this;
        }

        public Base Width(double width, Unit unit)
        {
            this.width = width;
            this.widthUnit = unit;
            return this;
        }

        public double GetMinWidth()
        {
            return minWidth;
        }

        public Unit GetMinWidthUnit()
        {
            return minWidthUnit;
        }

        public Base MinWidth(double minWidth)
        {
            this.minWidth = minWidth;
            return this;
        }

        public Base MinWidth(Unit unit)
        {
            this.minWidthUnit = unit;
            return this;
        }

        public Base MinWidth(double minWidth, Unit unit)
        {
            this.minWidth = minWidth;
            this.minWidthUnit = unit;
            return this;
        }

        public double GetMaxWidth()
        {
            return maxWidth;
        }

        public Unit GetMaxWidthUnit()
        {
            return maxWidthUnit;
        }

        public Base MaxWidth(double maxWidth)
        {
            this.maxWidth = maxWidth;
            return this;
        }

        public Base MaxWidth(Unit unit)
        {
            this.maxWidthUnit = unit;
            return this;
        }

        public Base MaxWidth(double maxWidth, Unit unit)
        {
            this.maxWidth = maxWidth;
            this.maxWidthUnit = unit;
            return this;
        }

        public double GetHeight()
        {
            return height;
        }

        public Unit GetHeightUnit()
        {
            return heightUnit;
        }

        public Base Height(double height)
        {
            this.height = height;
            return this;
        }

        public Base Height(Unit unit)
        {
            this.heightUnit = unit;
            return this;
        }

        public Base Height(double height, Unit unit)
        {
            this.height = height;
            this.heightUnit = unit;
            return this;
        }

        public double GetMinHeight()
        {
            return minHeight;
        }

        public Unit GetMinHeightUnit()
        {
            return minHeightUnit;
        }

        public Base MinHeight(double minHeight)
        {
            this.minHeight = minHeight;
            return this;
        }

        public Base MinHeight(Unit unit)
        {
            this.minHeightUnit = unit;
            return this;
        }

        public Base MinHeight(double minHeight, Unit unit)
        {
            this.minHeight = minHeight;
            this.minHeightUnit = unit;
            return this;
        }

        public double GetMaxHeight()
        {
            return maxHeight;
        }

        public Unit GetMaxHeightUnit()
        {
            return maxHeightUnit;
        }

        public Base MaxHeight(double maxHeight)
        {
            this.maxHeight = maxHeight;
            return this;
        }

        public Base MaxHeight(Unit unit)
        {
            this.maxHeightUnit = unit;
            return this;
        }

        public Base MaxHeight(double maxHeight, Unit unit)
        {
            this.maxHeight = maxHeight;
            this.maxHeightUnit = unit;
            return this;
        }

        public double GetMarginLeft()
        {
            return marginLeft;
        }

        public double GetMarginTop()
        {
            return marginTop;
        }

        public double GetMarginRight()
        {
            return marginRight;
        }

        public double GetMarginBottom()
        {
            return marginBottom;
        }

        public Base Margin(double margin)
        {
            marginLeft = margin;
            marginTop = margin;
            marginRight = margin;
            marginBottom = margin;
            return this;
        }

        public Base MarginX(double margin)
        {
            marginLeft = margin;
            marginRight = margin;
            return this;
        }

        public Base MarginY(double margin)
        {
            marginTop = margin;
            marginBottom = margin;
            return this;
        }

        public Base MarginLeft(double margin)
        {
            marginLeft = margin;
            return this;
        }

        public Base MarginTop(double margin)
        {
            marginTop = margin;
            return this;
        }

        public Base MarginRight(double margin)
        {
            marginRight = margin;
            return this;
        }

        public Base MarginBottom(double margin)
        {
            marginBottom = margin;
            return this;
        }

        public double GetPaddingLeft()
        {
            return paddingLeft;
        }

        public double GetPaddingTop()
        {
            return paddingTop;
        }

        public double GetPaddingRight()
        {
            return paddingRight;
        }

        public double GetPaddingBottom()
        {
            return paddingBottom;
        }

        public Base Padding(double padding)
        {
            paddingLeft = padding;
            paddingTop = padding;
            paddingRight = padding;
            paddingBottom = padding;
            return this;
        }

        public Base PaddingX(double padding)
        {
            paddingLeft = padding;
            paddingRight = padding;
            return this;
        }

        public Base PaddingY(double padding)
        {
            paddingTop = padding;
            paddingBottom = padding;
            return this;
        }

        public Base PaddingLeft(double padding)
        {
            paddingLeft = padding;
            return this;
        }

        public Base PaddingTop(double padding)
        {
            paddingTop = padding;
            return this;
        }

        public Base PaddingRight(double padding)
        {
            paddingRight = padding;
            return this;
        }

        public Base PaddingBottom(double padding)
        {
            paddingBottom = padding;
            return this;
        }

        public XBrush? GetBackground()
        {
            return backgroundBrush;
        }

        public Base Background(Color color)
        {
            backgroundBrush = new XBrush(color);
            return this;
        }

        public XPen? GetBorder()
        {
            return borderPen;
        }

        public XBrush? GetBorderBrush()
        {
            return borderBrush;
        }

        public int GetBorderThickness()
        {
            return borderThickness;
        }

        public XDashStyle GetBorderStyle()
        {
            return borderStyle;
        }

        public Base Border(Color color)
        {
            borderBrush = new XBrush(color);
            borderPen = new XPen(borderBrush, borderThickness, borderStyle);
            return this;
        }

        public Base Border(int thickness)
        {
            borderThickness = thickness;
            borderPen = new XPen(borderBrush, borderThickness, borderStyle);
            return this;
        }

        public Base Border(XDashStyle style)
        {
            borderStyle = style;
            borderPen = new XPen(borderBrush, borderThickness, borderStyle);
            return this;
        }

        public Base Border(Color color, int thickness)
        {
            borderBrush = new XBrush(color);
            borderThickness = thickness;
            borderPen = new XPen(borderBrush, borderThickness, borderStyle);
            return this;
        }

        public Base Border(Color color, int thickness, XDashStyle style)
        {
            borderBrush = new XBrush(color);
            borderThickness = thickness;
            borderStyle = style;
            borderPen = new XPen(borderBrush, borderThickness, borderStyle);
            return this;
        }

        public double GetCornerRadius()
        {
            return this.cornerRadius;
        }

        public Base CornerRadius(double radius)
        {
            this.cornerRadius = radius;
            return this;
        }
    }
}
