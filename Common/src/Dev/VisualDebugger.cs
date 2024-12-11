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

namespace CustomCommon.Debug
{
    public sealed class VisualDebugger
    {
        private DxVisualQueue Visual;

        public bool IsEnabled;
        public double X;
        public double Y;
        public double PaddingX;
        public double PaddingY;
        public XFont Font;
        public XBrush Foreground;
        public XBrush Background;

        public VisualDebugger(DxVisualQueue visual)
        {
            Visual = visual;

            IsEnabled = true;
            X = 15;
            Y = 30;
            PaddingX = 6;
            PaddingY = 4;
            // Font = new XFont("Inter", 14);
            Font = new XFont("JetBrainsMono Nerd Font", 14);
            Foreground = new XBrush(Colors.CornflowerBlue);
            Background = new XBrush(Colors.Black);
        }

        public void Debug(string message)
        {
            if (!IsEnabled)
                return;

            Size size = Font.GetSize(message);
            Point textPoint1 = new Point(X + PaddingX, Y + PaddingY);
            Point textPoint2 = new Point(textPoint1.X + size.Width, textPoint1.Y + size.Height);
            Point backgroundPoint1 = new Point(X, Y);
            Point backgroundPoint2 = new Point(textPoint2.X + PaddingX, textPoint2.Y + PaddingY);

            Visual.FillRectangle(Background, new Rect(backgroundPoint1, backgroundPoint2));
            Visual.DrawString(message, Font, Foreground, new Rect(textPoint1, textPoint2));
            Y += backgroundPoint2.Y - backgroundPoint1.Y;
        }

        public void Debug(params object[] values)
        {
            if (!IsEnabled)
                return;

            string message = string.Join("", values);

            Size size = Font.GetSize(message);
            Point textPoint1 = new Point(X + PaddingX, Y + PaddingY);
            Point textPoint2 = new Point(textPoint1.X + size.Width, textPoint1.Y + size.Height);
            Point backgroundPoint1 = new Point(X, Y);
            Point backgroundPoint2 = new Point(textPoint2.X + PaddingX, textPoint2.Y + PaddingY);

            Visual.FillRectangle(Background, new Rect(backgroundPoint1, backgroundPoint2));
            Visual.DrawString(message, Font, Foreground, new Rect(textPoint1, textPoint2));
            Y += backgroundPoint2.Y - backgroundPoint1.Y;
        }
    }
}
