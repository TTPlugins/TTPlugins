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
    public class Text : Base
    {
        private double baseWidth = 0;
        private double baseHeight = 0;

        private string text;
        private XFont font;
        private XBrush foregroundBrush = new XBrush(Colors.White);
        private XTextAlignment align = XTextAlignment.Left;

        public Text()
        {
            this.text = "";
            this.font = new XFont("Inter", 14);
        }

        public Text(string text)
        {
            this.text = text;
            this.font = new XFont("Inter", 14);

            RecalculateSize();
        }

        public Text(string text, XFont font)
        {
            this.text = text;
            this.font = font;

            RecalculateSize();
        }

        public override double GetBaseWidth(double parentWidth)
        {
            if (widthUnit == Unit.Pixel)
                return width;
            else if (widthUnit == Unit.Percent)
                return GetPixelWidth(parentWidth);

            double current = baseWidth;

            if (minWidthUnit != Unit.None)
                current = Math.Max(GetPixelMinWidth(parentWidth), current);
            if (maxWidthUnit != Unit.None)
                current = Math.Min(GetPixelMaxWidth(parentWidth), current);

            return current;
        }

        public override double GetBaseHeight(double parentHeight)
        {
            if (heightUnit == Unit.Pixel)
                return height;
            else if (heightUnit == Unit.Percent)
                return GetPixelHeight(height);

            double current = baseHeight;

            if (minHeightUnit != Unit.None)
                current = Math.Max(GetPixelMinHeight(parentHeight), current);
            if (maxHeightUnit != Unit.None)
                current = Math.Min(GetPixelMaxHeight(parentHeight), current);

            return current;
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

            if (text == "")
                return;

            visual.DrawString(
                text,
                font,
                foregroundBrush,
                GetInnerRect(x, y, parentWidth, parentHeight),
                align
            );
        }

        private void RecalculateSize()
        {
            Size size = font.GetSize(text);
            baseWidth = size.Width; // - Math.Floor((double)text.Length / 2);
            baseHeight = size.Height;
        }

        public string Get()
        {
            return this.text;
        }

        public Text Set(string text)
        {
            this.text = text;

            RecalculateSize();
            return this;
        }

        public Text Font(XFont font)
        {
            this.font = font;

            RecalculateSize();
            return this;
        }

        public Text FontName(string name)
        {
            this.font = new XFont(name, this.font.Size, this.font.Bold);

            RecalculateSize();
            return this;
        }

        public Text FontSize(int size)
        {
            this.font = new XFont(this.font.Name, size, this.font.Bold);

            RecalculateSize();
            return this;
        }

        public Text Bold()
        {
            this.font = new XFont(this.font.Name, this.font.Size, true);

            RecalculateSize();
            return this;
        }

        public Text Bold(bool bold)
        {
            this.font = new XFont(this.font.Name, this.font.Size, bold);

            RecalculateSize();
            return this;
        }

        public Text Foreground(Color color)
        {
            this.foregroundBrush = new XBrush(color);
            return this;
        }

        public Text Foreground(XColor color)
        {
            this.foregroundBrush = new XBrush(color);
            return this;
        }

        public Text Align(XTextAlignment align)
        {
            this.align = align;
            return this;
        }
    }
}
