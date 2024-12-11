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
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using TigerTrade.Dx;

namespace CustomCommon.Debug
{
    public sealed class VisualLogger
    {
        public enum LogType
        {
            Raw,
            Info,
            Notice,
            Warn,
            Error,
        }

        private int capacity;
        private Queue<(LogType type, string message)> logs;

        public double OffsetX;
        public double OffsetY;
        public double Gap;
        public double PaddingX;
        public double PaddingY;
        public XFont Font;
        public XBrush ForegroundRaw;
        public XBrush ForegroundInfo;
        public XBrush ForegroundNotice;
        public XBrush ForegroundWarn;
        public XBrush ForegroundError;
        public XBrush Background;

        public VisualLogger()
        {
            this.capacity = 86;
            this.logs = new Queue<(LogType type, string message)>(this.capacity);

            OffsetX = 10;
            OffsetY = 28;
            Gap = 1;
            PaddingX = 0;
            PaddingY = 0;
            Font = new XFont("JetBrainsMono Nerd Font", 11);
            ForegroundRaw = new XBrush(Colors.White);
            ForegroundInfo = new XBrush(new XColor(255, 90, 90, 90));
            ForegroundNotice = new XBrush(Colors.DodgerBlue);
            ForegroundWarn = new XBrush(Colors.Goldenrod);
            ForegroundError = new XBrush(Colors.Red);
            Background = new XBrush(Colors.Transparent);
        }

        public VisualLogger(int capacity)
        {
            this.capacity = capacity;
            this.logs = new Queue<(LogType type, string message)>(this.capacity);

            OffsetX = 10;
            OffsetY = 28;
            Gap = 1;
            PaddingX = 0;
            PaddingY = 0;
            Font = new XFont("JetBrainsMono Nerd Font", 11);
            ForegroundRaw = new XBrush(Colors.White);
            ForegroundInfo = new XBrush(new XColor(255, 90, 90, 90));
            ForegroundNotice = new XBrush(Colors.DodgerBlue);
            ForegroundWarn = new XBrush(Colors.Goldenrod);
            ForegroundError = new XBrush(Colors.Red);
            Background = new XBrush(Colors.Transparent);
        }

        public int GetCapacity()
        {
            return this.capacity;
        }

        public IEnumerable<(LogType type, string message)> GetLogs()
        {
            return this.logs;
        }

        /// <summary>
        /// Add a new log of type `Raw`
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Raw(string message)
        {
            this.Log(LogType.Raw, message);
        }

        /// <summary>
        /// Add a new log of type `Raw`
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Raw(params object[] values)
        {
            this.Log(LogType.Raw, string.Join("", values));
        }

        /// <summary>
        /// Add a new log of type `Info`
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Info(string message)
        {
            this.Log(LogType.Info, message);
        }

        /// <summary>
        /// Add a new log of type `Info`
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Info(params object[] values)
        {
            this.Log(LogType.Info, string.Join("", values));
        }

        /// <summary>
        /// Add a new log of type `Notice`
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Notice(string message)
        {
            this.Log(LogType.Notice, message);
        }

        /// <summary>
        /// Add a new log of type `Notice`
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Notice(params object[] values)
        {
            this.Log(LogType.Notice, string.Join("", values));
        }

        /// <summary>
        /// Add a new log of type `Warn`
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Warn(string message)
        {
            this.Log(LogType.Warn, message);
        }

        /// <summary>
        /// Add a new log of type `Warn`
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Warn(params object[] values)
        {
            this.Log(LogType.Warn, string.Join("", values));
        }

        /// <summary>
        /// Add a new log of type `Error`
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Error(string message)
        {
            this.Log(LogType.Error, message);
        }

        /// <summary>
        /// Add a new log of type `Error`
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Error(params object[] values)
        {
            this.Log(LogType.Error, string.Join("", values));
        }

        /// <summary>
        /// Add new logs of type `Raw` from an array of raw pointers.
        ///
        /// The given ptr must point to an array of raw pointers.
        /// Each raw pointer should point to a `C` compatible string.
        /// </summary>
        public unsafe void ExtendFromRawPointers(IntPtr* ptr, int length)
        {
            if (length <= 0)
                return;

            unsafe
            {
                for (int i = 0; i < length; i++)
                {
                    IntPtr p = ptr[i];
                    if (p == IntPtr.Zero)
                        continue;

                    this.Log(LogType.Raw, Marshal.PtrToStringAnsi(p));
                }
            }
        }

        /// <summary>
        /// Render the logs
        /// </summary>
        public void Render(DxVisualQueue visual)
        {
            double fontHeight = Font.GetHeight();
            double stepY = fontHeight + Gap;

            double x = OffsetX;
            double y = OffsetY;

            if (!Background.Color.IsTransparent)
            {
                double? maxWidth = null;
                foreach ((_, string message) in this.logs)
                {
                    double width = Font.GetWidth(message);

                    if (maxWidth == null || width > maxWidth)
                        maxWidth = width;
                }

                if (maxWidth != null)
                {
                    int count = this.logs.Count;

                    visual.FillRectangle(
                        Background,
                        new Rect(
                            x,
                            y,
                            maxWidth.Value + PaddingX,
                            (fontHeight * count) + (Gap * (count - 1)) + PaddingY
                        )
                    );
                }
            }

            if (PaddingX != 0)
                x += (PaddingX / 2);
            if (PaddingY != 0)
                y += (PaddingY / 2);

            foreach ((LogType type, string message) in this.logs)
            {
                XBrush foreground;
                if (type == LogType.Info)
                    foreground = ForegroundInfo;
                else if (type == LogType.Notice)
                    foreground = ForegroundNotice;
                else if (type == LogType.Warn)
                    foreground = ForegroundWarn;
                else if (type == LogType.Error)
                    foreground = ForegroundError;
                else
                    foreground = ForegroundRaw;

                visual.DrawString(
                    message,
                    Font,
                    foreground,
                    new Rect(x, y, Font.GetWidth(message), fontHeight)
                );
                y += stepY;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Log(LogType type, string message)
        {
            if (this.logs.Count == this.capacity)
                this.logs.Dequeue();

            this.logs.Enqueue((type, message));
        }
    }
}
