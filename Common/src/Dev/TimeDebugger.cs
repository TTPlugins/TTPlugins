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

using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using TigerTrade.Chart.Base;
using TigerTrade.Chart.Data;
using TigerTrade.Dx;

namespace CustomCommon.Debug
{
    public sealed class TimeDebugger
    {
        private Dictionary<int, List<string>> Messages;

        public bool IsEnabled;
        public double OffsetX;
        public double OffsetY;
        public double PaddingX;
        public double PaddingY;
        public XFont Font;
        public XBrush Foreground;
        public XBrush Background;

        public TimeDebugger()
        {
            Messages = new Dictionary<int, List<string>>();

            IsEnabled = true;
            OffsetX = 0;
            OffsetY = -30;
            PaddingX = 6;
            PaddingY = 4;
            Font = new XFont("Inter", 14);
            Foreground = new XBrush(Colors.CornflowerBlue);
            Background = new XBrush(Colors.Black);
        }

        public void ClearAll()
        {
            Messages.Clear();
        }

        public bool ClearIndex(int index)
        {
            return Messages.Remove(index);
        }

        public void Debug(int index, string message)
        {
            if (Messages.ContainsKey(index))
            {
                Messages[index].Add(message);
            }
            else
            {
                List<string> m = new List<string>();
                m.Add(message);

                Messages.Add(index, m);
            }
        }

        public void Debug(int index, params object[] values)
        {
            string message = string.Join("", values);

            if (Messages.ContainsKey(index))
            {
                Messages[index].Add(message);
            }
            else
            {
                List<string> m = new List<string>();
                m.Add(message);

                Messages.Add(index, m);
            }
        }

        public void Render(
            DxVisualQueue visual,
            IChartDataProvider dataProvider,
            IChartCanvas canvas
        )
        {
            if (!IsEnabled)
                return;

            double priceMultiplier = dataProvider.Step;

            foreach (KeyValuePair<int, List<string>> pair in Messages)
            {
                int index = pair.Key;
                List<string> messages = pair.Value;

                double x = canvas.GetX(index) + OffsetX;
                double y =
                    canvas.GetY(dataProvider.GetRawCluster(index).High * priceMultiplier) + OffsetY;

                if (!canvas.Rect.Contains(new Point(x, y)))
                    continue;

                int startIndex = messages.Count - 1;
                for (int i = startIndex; i >= 0; i--)
                {
                    string message = messages[i];

                    Size size = Font.GetSize(message);
                    Point textPoint1 = new Point(x + PaddingX, y + PaddingY);
                    Point textPoint2 = new Point(
                        textPoint1.X + size.Width,
                        textPoint1.Y + size.Height
                    );
                    Point backgroundPoint1 = new Point(x, y);
                    Point backgroundPoint2 = new Point(
                        textPoint2.X + PaddingX,
                        textPoint2.Y + PaddingY
                    );

                    visual.FillRectangle(Background, new Rect(backgroundPoint1, backgroundPoint2));
                    visual.DrawString(message, Font, Foreground, new Rect(textPoint1, textPoint2));
                    y -= backgroundPoint2.Y - backgroundPoint1.Y;
                }
            }
        }

        public void RenderIndex(
            int index,
            DxVisualQueue visual,
            IChartDataProvider dataProvider,
            IChartCanvas canvas
        )
        {
            if (!IsEnabled)
                return;
            if (!Messages.ContainsKey(index))
                return;

            double priceMultiplier = dataProvider.Step;

            List<string> messages = Messages[index];

            IRawCluster cluster = dataProvider.GetRawCluster(index);
            if (cluster == null)
                return;

            double x = canvas.GetX(index) + OffsetX;
            double y = canvas.GetY(cluster.High * priceMultiplier) + OffsetY;

            int startIndex = messages.Count - 1;
            for (int i = startIndex; i >= 0; i--)
            {
                string message = messages[i];

                Size size = Font.GetSize(message);
                Point textPoint1 = new Point(x + PaddingX, y + PaddingY);
                Point textPoint2 = new Point(textPoint1.X + size.Width, textPoint1.Y + size.Height);
                Point backgroundPoint1 = new Point(x, y);
                Point backgroundPoint2 = new Point(
                    textPoint2.X + PaddingX,
                    textPoint2.Y + PaddingY
                );

                visual.FillRectangle(Background, new Rect(backgroundPoint1, backgroundPoint2));
                visual.DrawString(message, Font, Foreground, new Rect(textPoint1, textPoint2));
                y -= backgroundPoint2.Y - backgroundPoint1.Y;
            }
        }
    }
}
