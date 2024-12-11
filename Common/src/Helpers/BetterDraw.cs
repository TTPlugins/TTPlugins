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
using TigerTrade.Dx;

namespace CustomCommon.Draw
{
    // FIX: Remove in favor of the UI module
    public sealed class BetterDraw
    {
        private DxVisualQueue Visual;

        public BetterDraw(DxVisualQueue visual)
        {
            Visual = visual;
        }

        public void DrawString(string text, XFont font, XBrush foreground, double x, double y)
        {
            Size size = font.GetSize(text);
            Point textPoint1 = new Point(x, y);
            Point textPoint2 = new Point(textPoint1.X + size.Width, textPoint1.Y + size.Height);
            Visual.DrawString(text, font, foreground, new Rect(textPoint1, textPoint2));
        }

        public static void DrawString(
            DxVisualQueue visual,
            string text,
            XFont font,
            XBrush foreground,
            double x,
            double y
        )
        {
            Size size = font.GetSize(text);
            Point textPoint1 = new Point(x, y);
            Point textPoint2 = new Point(textPoint1.X + size.Width, textPoint1.Y + size.Height);
            visual.DrawString(text, font, foreground, new Rect(textPoint1, textPoint2));
        }

        public void DrawString(
            XFont font,
            (string text, XBrush foreground)[] texts,
            double x,
            double y
        )
        {
            double currentX = x;
            foreach (var (text, foreground) in texts)
            {
                DrawString(text, font, foreground, currentX, y);
                currentX += font.GetSize(text).Width;
            }
        }

        public static void DrawString(
            DxVisualQueue visual,
            XFont font,
            (string text, XBrush foreground)[] texts,
            double x,
            double y
        )
        {
            double currentX = x;
            foreach (var (text, foreground) in texts)
            {
                DrawString(visual, text, font, foreground, currentX, y);
                currentX += font.GetSize(text).Width;
            }
        }

        public void DrawString(
            XFont font,
            (string text, XBrush foreground)[] texts,
            double x,
            double y,
            double textSpacing
        )
        {
            double currentX = x;
            foreach (var (text, foreground) in texts)
            {
                DrawString(text, font, foreground, currentX, y);
                currentX += font.GetSize(text).Width + textSpacing;
            }
        }

        public static void DrawString(
            DxVisualQueue visual,
            XFont font,
            (string text, XBrush foreground)[] texts,
            double x,
            double y,
            double textSpacing
        )
        {
            double currentX = x;
            foreach (var (text, foreground) in texts)
            {
                DrawString(visual, text, font, foreground, currentX, y);
                currentX += font.GetSize(text).Width + textSpacing;
            }
        }

        public void DrawString(
            XFont font,
            (string text, XBrush foreground, double marginLeft, double marginRight)[] texts,
            double x,
            double y
        )
        {
            double currentX = x;
            foreach (var (text, foreground, marginLeft, marginRight) in texts)
            {
                currentX += marginLeft;
                DrawString(text, font, foreground, currentX, y);
                currentX += font.GetSize(text).Width + marginLeft + marginRight;
            }
        }

        public static void DrawString(
            DxVisualQueue visual,
            XFont font,
            (string text, XBrush foreground, double marginLeft, double marginRight)[] texts,
            double x,
            double y
        )
        {
            double currentX = x;
            foreach (var (text, foreground, marginLeft, marginRight) in texts)
            {
                currentX += marginLeft;
                DrawString(visual, text, font, foreground, currentX, y);
                currentX += font.GetSize(text).Width + marginLeft + marginRight;
            }
        }
    }
}
