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
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CustomCommon.Helpers;
using TigerTrade.Chart.Alerts;
using TigerTrade.Chart.Base;
using TigerTrade.Chart.Indicators.Common;
using TigerTrade.Chart.Objects.Common;
using TigerTrade.Dx;
using DrawH = CustomCommon.Helpers.Draw;

namespace ObjectsPlusPlus.Alert
{
    /// <summary>
    /// ------ CP0
    /// </summary>
    internal class AlertInfo
    {
        public Point ControlPoint0;

        public Rect Text;
        public Rect Alert;

        public bool TextIntersectsWithCanvas;
        public bool AlertIntersectsWithCanvas;

        public XFont? Font;
    }

    [DataContract(
        Name = "AlertPlusPlusObject",
        Namespace = "http://schemas.datacontract.org/2004/07/TigerTrade.Chart.Objects.Custom"
    )]
    [ChartObject("AlertPlusPlusObject", "Alert++", 1, Type = typeof(Alert))]
    internal sealed class Alert : ObjectBase
    {
        private static double TextMarginX = 3;
        private static long AdjustCoordinatesRefreshRate = 90;

        private bool isObjectInArea;
        private bool areControlPointsVisible;
        private AlertInfo? info;

        private long lastDrawTimestamp = 0;

        private AlertLevel? alertLevel;

        private TimeframeVisibility _visibility;

        [DataMember(Name = "Visibility")]
        [Category("General"), DisplayName("Timeframe Visibility")]
        public TimeframeVisibility Visibility
        {
            get => _visibility;
            set
            {
                if (value == _visibility)
                    return;

                _visibility = value;

                Time.SetTimeframeVisibility(DataProvider, Periods, _visibility);

                OnPropertyChanged(nameof(Visibility));
            }
        }

        [DataMember(Name = "Price")]
        [Category("General"), DisplayName("Price")]
        public double Price
        {
            get => ControlPoints[0].Y;
            set
            {
                if (value == ControlPoints[0].Y)
                    return;

                ControlPoints[0].Y = value;

                OnPropertyChanged(nameof(ControlPoints));
            }
        }

        private XBrush _activeBrush;

        private XColor _activeColor;

        [DataMember(Name = "ActiveColor")]
        [Category("Style"), DisplayName("Active Color")]
        public XColor ActiveColor
        {
            get => _activeColor;
            set
            {
                if (value == _activeColor)
                    return;

                _activeColor = value;

                _activeBrush = new XBrush(_activeColor);

                OnPropertyChanged(nameof(ActiveColor));
            }
        }

        private XBrush _inactiveBrush;

        private XColor _inactiveColor;

        [DataMember(Name = "InactiveColor")]
        [Category("Style"), DisplayName("Inactive Color")]
        public XColor InactiveColor
        {
            get => _inactiveColor;
            set
            {
                if (value == _inactiveColor)
                    return;

                _inactiveColor = value;

                _inactiveBrush = new XBrush(_inactiveColor);

                OnPropertyChanged(nameof(InactiveColor));
            }
        }

        private string _text;

        [DataMember(Name = "Text")]
        [Category("Text"), DisplayName("Text")]
        public string Text
        {
            get => _text;
            set
            {
                if (value == _text)
                    return;

                _text = value;

                OnPropertyChanged(nameof(Text));
            }
        }

        private TextOptions _textOptions;

        [DataMember(Name = "TextOptions")]
        [Category("Text"), DisplayName("Options")]
        public TextOptions TextOptions
        {
            get => _textOptions ?? (_textOptions = new TextOptions());
            set
            {
                if (Equals(value, _textOptions))
                    return;

                _textOptions = value;

                OnPropertyChanged(nameof(TextOptions));
            }
        }

        private bool _showPriceLabel;

        [DataMember(Name = "ShowPriceLabel")]
        [Category("Extra"), DisplayName("Show Price Label")]
        public bool ShowPriceLabel
        {
            get => _showPriceLabel;
            set
            {
                if (value == _showPriceLabel)
                    return;

                _showPriceLabel = value;

                OnPropertyChanged(nameof(ShowPriceLabel));
            }
        }

        private ChartAlertSettings _alert;

        [DataMember(Name = "AlertSettings")]
        [Category("Alert"), DisplayName("Alert")]
        public ChartAlertSettings AlertSettings
        {
            get => _alert ?? (_alert = new ChartAlertSettings());
            set
            {
                if (Equals(value, _alert))
                    return;

                _alert = value;

                OnPropertyChanged(nameof(AlertSettings));
            }
        }

        private AlertFrequency _alertFrequency;

        [DataMember(Name = "AlertFrequency")]
        [Category("Alert"), DisplayName("Frequency")]
        public AlertFrequency AlertFrequency
        {
            get => _alertFrequency;
            set
            {
                if (value == _alertFrequency)
                    return;

                _alertFrequency = value;

                AlertSettings.Active = true;
                if (_alertFrequency == AlertFrequency.Once)
                    AlertSettings.Execution = ChartAlertExecution.OnlyOnce;
                else if (_alertFrequency == AlertFrequency.OncePerBar)
                    AlertSettings.Execution = ChartAlertExecution.EveryTime;
                else if (_alertFrequency == AlertFrequency.EveryTime)
                    AlertSettings.Execution = ChartAlertExecution.EveryTime;

                OnPropertyChanged(nameof(AlertFrequency));
            }
        }

        private AlertOptions _alertOptions;

        [DataMember(Name = "AlertOptions")]
        [Category("Alert"), DisplayName("Alert Options")]
        public AlertOptions AlertOptions
        {
            get => _alertOptions ?? (_alertOptions = new AlertOptions());
            set
            {
                if (Equals(value, _alertOptions))
                    return;

                _alertOptions = value;

                OnPropertyChanged(nameof(AlertOptions));
            }
        }

        public Alert()
        {
            Visibility = TimeframeVisibility.Disabled;

            ActiveColor = Colors.Gold;
            InactiveColor = Colors.Gray;

            Text = "";
            TextOptions = new TextOptions();

            ShowPriceLabel = false;

            AlertSettings = new ChartAlertSettings();
            AlertFrequency = AlertFrequency.EveryTime;
            AlertOptions = new AlertOptions();
            AlertSettings.Active = true;
        }

        protected override void Prepare()
        {
            if (InSetup)
            {
                Time.SetTimeframeVisibility(DataProvider, Periods, _visibility);
            }

            Point p0 = new Point(Canvas.Rect.Right, Canvas.GetY(ControlPoints[0].Y));

            Rect alert = new Rect(
                p0.X
                    - (
                        AlertSettings.IsActive
                            ? AlertOptions.AlertBellWidth
                            : AlertOptions.AlertBellInactiveWidth
                    )
                    - AlertOptions.BellOffset,
                p0.Y - AlertOptions.AlertBellHalfHeight,
                AlertOptions.AlertBellWidth,
                AlertOptions.AlertBellHeight
            );

            XFont? font = null;

            Rect text = Rect.Empty;
            if (Text != "")
            {
                font = new XFont(Canvas.ChartFont.Name, TextOptions.FontSize);
                double textWidth = font.GetWidth(Text);
                double textHeight = font.GetHeight();

                alert.X -= textWidth + TextMarginX;

                double x = p0.X - textWidth - AlertOptions.BellOffset;
                double y = p0.Y - (textHeight / 2);

                text = new Rect(x, y, textWidth, textHeight);
            }

            info = new AlertInfo
            {
                ControlPoint0 = p0,

                Text = text,
                Alert = alert,

                TextIntersectsWithCanvas = Canvas.Rect.IntersectsWith(text),
                AlertIntersectsWithCanvas = Canvas.Rect.IntersectsWith(alert),

                Font = font,
            };

            isObjectInArea = info.TextIntersectsWithCanvas || info.AlertIntersectsWithCanvas;
        }

        protected override void Draw(DxVisualQueue visual, ref List<ObjectLabelInfo> labels)
        {
            if (info == null || !isObjectInArea)
                return;

            if (areControlPointsVisible && !Lock)
            {
                if (Keyboard.IsKeyDown(Key.Up))
                {
                    long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    if (now - lastDrawTimestamp >= AdjustCoordinatesRefreshRate)
                    {
                        lastDrawTimestamp = now;

                        ControlPoints[0].Y += DataProvider.Step;

                        ControlPointsChanged();
                        OnPropertyChanged(nameof(ControlPoints));
                    }
                }
                else if (Keyboard.IsKeyDown(Key.Down))
                {
                    long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    if (now - lastDrawTimestamp >= AdjustCoordinatesRefreshRate)
                    {
                        lastDrawTimestamp = now;

                        ControlPoints[0].Y -= DataProvider.Step;

                        ControlPointsChanged();
                        OnPropertyChanged(nameof(ControlPoints));
                    }
                }
            }

            if (!info.Text.IsEmpty && info.TextIntersectsWithCanvas && info.Font != null)
            {
                if (!TextOptions.BackgroundColor.IsTransparent)
                    visual.FillRectangle(TextOptions.BackgroundBrush, info.Text);
                if (!TextOptions.BorderColor.IsTransparent)
                    visual.DrawRectangle(TextOptions.BorderPen, info.Text);
                if (!TextOptions.ForegroundColor.IsTransparent)
                    visual.DrawString(Text, info.Font, TextOptions.ForegroundBrush, info.Text);
            }

            if (!info.Alert.IsEmpty && info.AlertIntersectsWithCanvas)
            {
                if (AlertSettings.IsActive)
                {
                    visual.DrawString(
                        AlertOptions.AlertBellText,
                        AlertOptions.AlertBellFont,
                        _activeBrush,
                        info.Alert.X,
                        info.Alert.Y
                    );
                }
                else
                {
                    visual.DrawString(
                        AlertOptions.AlertBellInactiveText,
                        AlertOptions.AlertBellFont,
                        _inactiveBrush,
                        info.Alert.X,
                        info.Alert.Y
                    );
                }
            }

            if (areControlPointsVisible)
            {
                labels.Add(
                    new ObjectLabelInfo(
                        ControlPoints[0].Y,
                        AlertSettings.IsActive ? _activeColor : _inactiveColor
                    )
                );

                areControlPointsVisible = false;
            }
            else
            {
                if (ShowPriceLabel)
                {
                    labels.Add(
                        new ObjectLabelInfo(
                            ControlPoints[0].Y,
                            AlertSettings.IsActive ? _activeColor : _inactiveColor
                        )
                    );
                }
            }
        }

        public override void DrawControlPoints(DxVisualQueue visual)
        {
            if (info == null)
                return;

            areControlPointsVisible = true;

            if (InMove)
            {
                if (Mouse.OverrideCursor != Cursors.Arrow)
                    Mouse.OverrideCursor = Cursors.Arrow;
            }
            else
            {
                if (Mouse.OverrideCursor != null)
                    Mouse.OverrideCursor = null;
            }
        }

        protected override bool IsObjectOnChart()
        {
            if (DataProvider == null)
                return false;

            int datesCount = DataProvider.Dates.Count;
            if (datesCount <= 0)
                return false;

            return true;
        }

        protected override bool IsObjectInArea()
        {
            return isObjectInArea;
        }

        protected override bool InObject(int x, int y)
        {
            if (info == null)
                return false;

            return info.Text.Contains(x, y) || info.Alert.Contains(x, y);
        }

        public override int GetControlPoint(int x, int y)
        {
            if (info == null)
                return -1;

            return DrawH.GetControlPointIndex(x, y, info.ControlPoint0);
        }

        public override int GetExtraPoint(int x, int y)
        {
            return -1;
        }

        protected override int GetMinDist(int x, int y)
        {
            if (info == null)
                return -1;

            return DrawH.GetMinDistance(x, y, info.Text, info.Alert);
        }

        public override void CheckAlert(List<IndicatorBase> indicators)
        {
            if (!AlertSettings.IsActive || DataProvider == null)
            {
                alertLevel = null;
                return;
            }

            int index = DataProvider.Count - 1;
            if (index < 0)
            {
                alertLevel = null;
                return;
            }

            double step = DataProvider.Step;
            double price = DataProvider.GetRawCluster(index).Close * step;

            double priceLevel = ControlPoints[0].Y;
            if (this.alertLevel == null)
                this.alertLevel = new AlertLevel(
                    step,
                    AlertFrequency,
                    priceLevel,
                    AlertDistanceUnit.Tick,
                    0
                );
            else
                this.alertLevel.UpdatePropertiesIfNeeded(
                    AlertFrequency,
                    priceLevel,
                    AlertDistanceUnit.Tick,
                    0
                );

            if (this.alertLevel.Check(price, index))
            {
                if (AlertManager.Check(DataProvider.Symbol.Code, AlertOptions.Throttle))
                {
                    string message = $"[Alert++] Price at {priceLevel}";

                    if (Text != "")
                        message += $", {Text}";

                    AddAlert(AlertSettings, message);
                }
            }
        }

        public override void ApplyTheme(IChartTheme theme)
        {
            base.ApplyTheme(theme);
        }

        public override void CopyTemplate(ObjectBase objectBase, bool style)
        {
            if (objectBase is Alert o)
            {
                Visibility = o.Visibility;

                ActiveColor = o.ActiveColor;
                InactiveColor = o.InactiveColor;

                Text = o.Text;
                TextOptions = o.TextOptions.DeepCopy();

                ShowPriceLabel = o.ShowPriceLabel;

                AlertSettings.Copy(o.AlertSettings, !style);
                AlertFrequency = o.AlertFrequency;
                AlertOptions = o.AlertOptions.DeepCopy();
                AlertSettings.Active = true;
                OnPropertyChanged(nameof(AlertSettings));
            }

            base.CopyTemplate(objectBase, style);
        }
    }
}
