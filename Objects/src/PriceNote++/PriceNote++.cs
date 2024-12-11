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
using TigerTrade.Core.UI.Converters;
using TigerTrade.Dx;
using TigerTrade.Dx.Enums;
using DrawH = CustomCommon.Helpers.Draw;
using LineP = CustomCommon.Helpers.Line;

namespace ObjectsPlusPlus.PriceNote
{
    /// <summary>
    /// CP0 ------
    /// </summary>
    internal class PriceNoteInfo
    {
        public Point ControlPoint0;

        public Rect Text;
        public LineP Line;
        public Rect ExtendedText;
        public LineP? ExtendedLine;
        public Rect Alert;

        public bool TextIntersectsWithCanvas;
        public bool LineIntersectsWithCanvas;
        public bool ExtendedTextIntersectsWithCanvas;
        public bool ExtendedLineIntersectsWithCanvas;
        public bool AlertIntersectsWithCanvas;

        public XFont? Font;
        public XFont? ExtendedFont;
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    [DataContract(
        Name = "VerticalAlignment",
        Namespace = "http://schemas.datacontract.org/2004/07/TigerTrade.Chart.Objects.Custom"
    )]
    public enum VerticalAlignment
    {
        [EnumMember(Value = "Top"), Description("Top")]
        Top,

        [EnumMember(Value = "Center"), Description("Center")]
        Center,

        [EnumMember(Value = "Bottom"), Description("Bottom")]
        Bottom,
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    [DataContract(
        Name = "Direction",
        Namespace = "http://schemas.datacontract.org/2004/07/TigerTrade.Chart.Objects.Custom"
    )]
    public enum Direction
    {
        [EnumMember(Value = "Left"), Description("Left")]
        Left,

        [EnumMember(Value = "Right"), Description("Right")]
        Right,
    }

    [DataContract(
        Name = "PriceNotePlusPlusObject",
        Namespace = "http://schemas.datacontract.org/2004/07/TigerTrade.Chart.Objects.Custom"
    )]
    [ChartObject("PriceNotePlusPlusObject", "PriceNote++", 1, Type = typeof(PriceNote))]
    internal sealed class PriceNote : ObjectBase
    {
        private static int MinLineHitbox = 8;
        private static long AdjustCoordinatesRefreshRate = 90;

        private bool isObjectInArea;
        private bool areControlPointsVisible;
        private PriceNoteInfo? info;

        private ObjectPoint[] lastControlPoints;

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

        private int _textFontSize;

        [DataMember(Name = "TextFontSize")]
        [Category("Text"), DisplayName("Font Size")]
        public int TextFontSize
        {
            get => _textFontSize;
            set
            {
                value = Math.Max(1, Math.Min(300, value));

                if (value == _textFontSize)
                    return;

                _textFontSize = value;

                OnPropertyChanged(nameof(TextFontSize));
            }
        }

        private VerticalAlignment _textVerticalAlignment;

        [DataMember(Name = "TextVerticalAlignment")]
        [Category("Text"), DisplayName("Vertical Alignment")]
        public VerticalAlignment TextVerticalAlignment
        {
            get => _textVerticalAlignment;
            set
            {
                if (value == _textVerticalAlignment)
                    return;

                _textVerticalAlignment = value;

                OnPropertyChanged(nameof(TextVerticalAlignment));
            }
        }

        private double _textHorizontalPadding;

        [DataMember(Name = "TextHorizontalPadding")]
        [Category("Text"), DisplayName("Horizontal Padding")]
        public double TextHorizontalPadding
        {
            get => _textHorizontalPadding;
            set
            {
                value = Math.Max(0, Math.Min(300, value));

                if (value == _textHorizontalPadding)
                    return;

                _textHorizontalPadding = value;

                OnPropertyChanged(nameof(TextHorizontalPadding));
            }
        }

        private double _textVerticalPadding;

        [DataMember(Name = "TextVerticalPadding")]
        [Category("Text"), DisplayName("Vertical Padding")]
        public double TextVerticalPadding
        {
            get => _textVerticalPadding;
            set
            {
                value = Math.Max(0, Math.Min(300, value));

                if (value == _textVerticalPadding)
                    return;

                _textVerticalPadding = value;

                OnPropertyChanged(nameof(TextVerticalPadding));
            }
        }

        private XBrush _textColorBrush;

        private XColor _textColor;

        [DataMember(Name = "TextColor")]
        [Category("Text"), DisplayName("Color")]
        public XColor TextColor
        {
            get => _textColor;
            set
            {
                if (value == _textColor)
                    return;

                _textColor = value;

                _textColorBrush = new XBrush(_textColor);

                OnPropertyChanged(nameof(TextColor));
            }
        }

        private XBrush _textBackgroundColorBrush;

        private XColor _textBackgroundColor;

        [DataMember(Name = "TextBackgroundColor")]
        [Category("Text"), DisplayName("Background Color")]
        public XColor TextBackgroundColor
        {
            get => _textBackgroundColor;
            set
            {
                if (value == _textBackgroundColor)
                    return;

                _textBackgroundColor = value;

                _textBackgroundColorBrush = new XBrush(_textBackgroundColor);

                OnPropertyChanged(nameof(TextBackgroundColor));
            }
        }

        private Direction _lineDirection;

        [DataMember(Name = "LineDirection")]
        [Category("Line"), DisplayName("Direction")]
        public Direction LineDirection
        {
            get => _lineDirection;
            set
            {
                if (value == _lineDirection)
                    return;

                _lineDirection = value;

                OnPropertyChanged(nameof(LineDirection));
            }
        }

        private XBrush _lineBrush;

        private XPen _linePen;

        private XColor _lineColor;

        [DataMember(Name = "LineColor")]
        [Category("Line"), DisplayName("Color")]
        public XColor LineColor
        {
            get => _lineColor;
            set
            {
                if (value == _lineColor)
                    return;

                _lineColor = value;

                _lineBrush = new XBrush(_lineColor);
                _linePen = new XPen(_lineBrush, LineThickness, LineStyle);

                OnPropertyChanged(nameof(LineColor));
            }
        }

        private int _lineWidth;

        [DataMember(Name = "LineWidth")]
        [Category("Line"), DisplayName("Width")]
        public int LineWidth
        {
            get => _lineWidth;
            set
            {
                value = Math.Max(0, value);

                if (value == _lineWidth)
                    return;

                _lineWidth = value;

                _linePen = new XPen(_lineBrush, LineThickness, LineStyle);

                OnPropertyChanged(nameof(LineWidth));
            }
        }

        private int lineHitbox;
        private int _lineThickness;

        [DataMember(Name = "LineThickness")]
        [Category("Line"), DisplayName("Thickness")]
        public int LineThickness
        {
            get => _lineThickness;
            set
            {
                value = Math.Max(1, Math.Min(100, value));

                if (value == _lineThickness)
                    return;

                _lineThickness = value;

                _linePen = new XPen(_lineBrush, _lineThickness, LineStyle);
                lineHitbox = Math.Max(MinLineHitbox, _lineThickness);

                OnPropertyChanged(nameof(LineThickness));
            }
        }

        private XDashStyle _lineStyle;

        [DataMember(Name = "LineStyle")]
        [Category("Line"), DisplayName("Style")]
        public XDashStyle LineStyle
        {
            get => _lineStyle;
            set
            {
                if (value == _lineStyle)
                    return;

                _lineStyle = value;

                _linePen = new XPen(_lineBrush, LineThickness, _lineStyle);

                OnPropertyChanged(nameof(LineStyle));
            }
        }

        private bool _showExtendedText;

        [DataMember(Name = "ShowExtendedText")]
        [Category("Extended Text"), DisplayName("Show")]
        public bool ShowExtendedText
        {
            get => _showExtendedText;
            set
            {
                if (value == _showExtendedText)
                    return;

                _showExtendedText = value;

                OnPropertyChanged(nameof(ShowExtendedText));
            }
        }

        private int _extendedTextFontSize;

        [DataMember(Name = "ExtendedTextFontSize")]
        [Category("Extended Text"), DisplayName("Font Size")]
        public int ExtendedTextFontSize
        {
            get => _extendedTextFontSize;
            set
            {
                value = Math.Max(1, Math.Min(300, value));

                if (value == _extendedTextFontSize)
                    return;

                _extendedTextFontSize = value;

                OnPropertyChanged(nameof(ExtendedTextFontSize));
            }
        }

        private VerticalAlignment _extendedTextVerticalAlignment;

        [DataMember(Name = "ExtendedTextVerticalAlignment")]
        [Category("Extended Text"), DisplayName("Vertical Alignment")]
        public VerticalAlignment ExtendedTextVerticalAlignment
        {
            get => _extendedTextVerticalAlignment;
            set
            {
                if (value == _extendedTextVerticalAlignment)
                    return;

                _extendedTextVerticalAlignment = value;

                OnPropertyChanged(nameof(ExtendedTextVerticalAlignment));
            }
        }

        private double _extendedTextOffset;

        [DataMember(Name = "ExtendedTextOffset")]
        [Category("Extended Text"), DisplayName("Offset")]
        public double ExtendedTextOffset
        {
            get => _extendedTextOffset;
            set
            {
                value = Math.Max(0, value);

                if (value == _extendedTextOffset)
                    return;

                _extendedTextOffset = value;

                OnPropertyChanged(nameof(ExtendedTextOffset));
            }
        }

        private double _extendedTextHorizontalPadding;

        [DataMember(Name = "ExtendedTextHorizontalPadding")]
        [Category("Extended Text"), DisplayName("Horizontal Padding")]
        public double ExtendedTextHorizontalPadding
        {
            get => _extendedTextHorizontalPadding;
            set
            {
                value = Math.Max(0, Math.Min(300, value));

                if (value == _extendedTextHorizontalPadding)
                    return;

                _extendedTextHorizontalPadding = value;

                OnPropertyChanged(nameof(ExtendedTextHorizontalPadding));
            }
        }

        private double _extendedTextVerticalPadding;

        [DataMember(Name = "ExtendedTextVerticalPadding")]
        [Category("Extended Text"), DisplayName("Vertical Padding")]
        public double ExtendedTextVerticalPadding
        {
            get => _extendedTextVerticalPadding;
            set
            {
                value = Math.Max(0, Math.Min(300, value));

                if (value == _extendedTextVerticalPadding)
                    return;

                _extendedTextVerticalPadding = value;

                OnPropertyChanged(nameof(ExtendedTextVerticalPadding));
            }
        }

        private XBrush _extendedTextColorBrush;

        private XColor _extendedTextColor;

        [DataMember(Name = "ExtendedTextColor")]
        [Category("Extended Text"), DisplayName("Color")]
        public XColor ExtendedTextColor
        {
            get => _extendedTextColor;
            set
            {
                if (value == _extendedTextColor)
                    return;

                _extendedTextColor = value;

                _extendedTextColorBrush = new XBrush(_extendedTextColor);

                OnPropertyChanged(nameof(ExtendedTextColor));
            }
        }

        private XBrush _extendedTextBackgroundColorBrush;

        private XColor _extendedTextBackgroundColor;

        [DataMember(Name = "ExtendedTextBackgroundColor")]
        [Category("Extended Text"), DisplayName("Background Color")]
        public XColor ExtendedTextBackgroundColor
        {
            get => _extendedTextBackgroundColor;
            set
            {
                if (value == _extendedTextBackgroundColor)
                    return;

                _extendedTextBackgroundColor = value;

                _extendedTextBackgroundColorBrush = new XBrush(_extendedTextBackgroundColor);

                OnPropertyChanged(nameof(ExtendedTextBackgroundColor));
            }
        }

        private bool _showExtendedLine;

        [DataMember(Name = "ShowExtendedLine")]
        [Category("Extended Line"), DisplayName("Show")]
        public bool ShowExtendedLine
        {
            get => _showExtendedLine;
            set
            {
                if (value == _showExtendedLine)
                    return;

                _showExtendedLine = value;

                OnPropertyChanged(nameof(ShowExtendedLine));
            }
        }

        private XBrush _extendedLineBrush;

        private XPen _extendedLinePen;

        private XColor _extendedLineColor;

        [DataMember(Name = "ExtendedLineColor")]
        [Category("Extended Line"), DisplayName("Color")]
        public XColor ExtendedLineColor
        {
            get => _extendedLineColor;
            set
            {
                if (value == _extendedLineColor)
                    return;

                _extendedLineColor = value;

                _extendedLineBrush = new XBrush(_extendedLineColor);
                _extendedLinePen = new XPen(
                    _extendedLineBrush,
                    ExtendedLineThickness,
                    ExtendedLineStyle
                );

                OnPropertyChanged(nameof(ExtendedLineColor));
            }
        }

        private int extendedLineHitbox;
        private int _extendedLineThickness;

        [DataMember(Name = "ExtendedLineThickness")]
        [Category("Extended Line"), DisplayName("Thickness")]
        public int ExtendedLineThickness
        {
            get => _extendedLineThickness;
            set
            {
                value = Math.Max(1, Math.Min(100, value));

                if (value == _extendedLineThickness)
                    return;

                _extendedLineThickness = value;

                _extendedLinePen = new XPen(
                    _extendedLineBrush,
                    _extendedLineThickness,
                    ExtendedLineStyle
                );
                extendedLineHitbox = Math.Max(MinLineHitbox, _extendedLineThickness);

                OnPropertyChanged(nameof(ExtendedLineThickness));
            }
        }

        private XDashStyle _extendedLineStyle;

        [DataMember(Name = "ExtendedLineStyle")]
        [Category("Extended Line"), DisplayName("Style")]
        public XDashStyle ExtendedLineStyle
        {
            get => _extendedLineStyle;
            set
            {
                if (value == _extendedLineStyle)
                    return;

                _extendedLineStyle = value;

                _extendedLinePen = new XPen(
                    _extendedLineBrush,
                    ExtendedLineThickness,
                    _extendedLineStyle
                );

                OnPropertyChanged(nameof(ExtendedLineStyle));
            }
        }

        private bool _showOnPriceScale;

        [DataMember(Name = "ShowOnPriceScale")]
        [Category("Extra"), DisplayName("Show on Price Scale")]
        public bool ShowOnPriceScale
        {
            get => _showOnPriceScale;
            set
            {
                if (value == _showOnPriceScale)
                    return;

                _showOnPriceScale = value;

                OnPropertyChanged(nameof(ShowOnPriceScale));
            }
        }

        private XColor _priceLabelColor;

        [DataMember(Name = "PriceLabelColor")]
        [Category("Extra"), DisplayName("Label Color")]
        public XColor PriceLabelColor
        {
            get => _priceLabelColor;
            set
            {
                if (value == _priceLabelColor)
                    return;

                _priceLabelColor = value;

                OnPropertyChanged(nameof(PriceLabelColor));
            }
        }

        private ChartAlertSettings _alert;

        [DataMember(Name = "Alert")]
        [Category("Alert"), DisplayName("Alert")]
        public ChartAlertSettings Alert
        {
            get => _alert ?? (_alert = new ChartAlertSettings());
            set
            {
                if (Equals(value, _alert))
                    return;

                _alert = value;

                OnPropertyChanged(nameof(Alert));
            }
        }

        private double _alertDistance;

        [DataMember(Name = "AlertDistance")]
        [Category("Alert"), DisplayName("Distance")]
        public double AlertDistance
        {
            get => _alertDistance;
            set
            {
                value = Math.Max(0, value);

                if (value == _alertDistance)
                    return;

                _alertDistance = value;

                OnPropertyChanged(nameof(AlertDistance));
            }
        }

        private AlertDistanceUnit _alertUnit;

        [DataMember(Name = "AlertUnit")]
        [Category("Alert"), DisplayName("Distance Unit")]
        public AlertDistanceUnit AlertUnit
        {
            get => _alertUnit;
            set
            {
                if (value == _alertUnit)
                    return;

                _alertUnit = value;

                OnPropertyChanged(nameof(AlertUnit));
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

                Alert.Active = true;
                if (_alertFrequency == AlertFrequency.Once)
                    Alert.Execution = ChartAlertExecution.OnlyOnce;
                else if (_alertFrequency == AlertFrequency.OncePerBar)
                    Alert.Execution = ChartAlertExecution.EveryTime;
                else if (_alertFrequency == AlertFrequency.EveryTime)
                    Alert.Execution = ChartAlertExecution.EveryTime;

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

        protected override int PenWidth => LineThickness;

        public PriceNote()
        {
            Visibility = TimeframeVisibility.Disabled;

            Text = "Text";
            TextFontSize = 12;
            TextVerticalAlignment = VerticalAlignment.Center;
            TextHorizontalPadding = 3;
            TextVerticalPadding = 1;
            TextColor = Colors.Red;
            TextBackgroundColor = Colors.Black;

            LineDirection = Direction.Left;
            LineColor = Colors.Red;
            LineWidth = 30;
            LineThickness = 1;
            LineStyle = XDashStyle.Solid;

            ShowExtendedText = false;
            ExtendedTextFontSize = 12;
            ExtendedTextVerticalAlignment = VerticalAlignment.Center;
            ExtendedTextOffset = 0;
            ExtendedTextHorizontalPadding = 3;
            ExtendedTextVerticalPadding = 1;
            ExtendedTextColor = Colors.Red;
            ExtendedTextBackgroundColor = Colors.Black;

            ShowExtendedLine = false;
            ExtendedLineColor = Colors.White;
            ExtendedLineThickness = 1;
            ExtendedLineStyle = XDashStyle.Solid;

            ShowOnPriceScale = false;
            PriceLabelColor = Colors.Red;

            Alert = new ChartAlertSettings();
            AlertDistance = 0;
            AlertUnit = AlertDistanceUnit.Tick;
            AlertFrequency = AlertFrequency.EveryTime;
            AlertOptions = new AlertOptions();
        }

        protected override void Prepare()
        {
            if (InSetup)
            {
                Time.SetTimeframeVisibility(DataProvider, Periods, _visibility);
            }

            Point p0 = ToPoint(ControlPoints[0]);

            XFont font = new XFont(Canvas.ChartFont.Name, TextFontSize);

            Rect text = Rect.Empty;
            if (Text != "")
            {
                double textWidth = font.GetWidth(Text) + (TextHorizontalPadding * 2.0);
                double textHeight = font.GetHeight() + (TextVerticalPadding * 2.0);

                double textX;
                if (LineDirection == Direction.Left)
                    textX = p0.X - LineWidth - textWidth;
                else
                    textX = p0.X + LineWidth;

                double textY;
                if (TextVerticalAlignment == VerticalAlignment.Top)
                    textY = p0.Y - textHeight - (LineThickness / 2.0);
                else if (TextVerticalAlignment == VerticalAlignment.Center)
                    textY = p0.Y - textHeight / 2.0;
                else
                    textY = p0.Y + (LineThickness / 2.0) + 1.0;

                text = new Rect(textX, textY, textWidth, textHeight);
            }

            LineP line = new LineP(p0, p0, lineHitbox);
            if (LineDirection == Direction.Left)
                line.p0.X = text.Left;
            else
                line.p1.X = text.Right;

            XFont? extendedFont = null;
            Rect extendedText = Rect.Empty;
            if (ShowExtendedText && Text != "")
            {
                extendedFont = new XFont(Canvas.ChartFont.Name, ExtendedTextFontSize);
                double w = extendedFont.GetWidth(Text) + (ExtendedTextHorizontalPadding * 2.0);
                double h = extendedFont.GetHeight() + (ExtendedTextVerticalPadding * 2.0);

                double textX = Canvas.Rect.Right - w - ExtendedTextOffset;

                double textY;
                if (ExtendedTextVerticalAlignment == VerticalAlignment.Top)
                    textY = p0.Y - h - (LineThickness / 2.0);
                else if (ExtendedTextVerticalAlignment == VerticalAlignment.Center)
                    textY = p0.Y - h / 2.0;
                else
                    textY = p0.Y + (LineThickness / 2.0) + 1.0;

                extendedText = new Rect(textX, textY, w, h);
            }

            LineP? extendedLine = null;
            if (ShowExtendedLine)
            {
                extendedLine = new LineP(
                    new Point(p0.X, p0.Y),
                    new Point(Canvas.Rect.Right - ExtendedTextOffset, p0.Y),
                    extendedLineHitbox
                );
            }

            Rect alert = Rect.Empty;
            if (Alert.IsActive && AlertOptions.ShowBell)
            {
                double priceLevel = ControlPoints[0].Y;

                if (this.alertLevel == null)
                    this.alertLevel = new AlertLevel(
                        DataProvider.Step,
                        AlertFrequency,
                        priceLevel,
                        AlertUnit,
                        AlertDistance
                    );
                else
                    this.alertLevel.UpdatePropertiesIfNeeded(
                        AlertFrequency,
                        priceLevel,
                        AlertUnit,
                        AlertDistance
                    );

                double currentPrice = Chart.GetRealtimePriceSafe(DataProvider) ?? 0;
                double deltaMinLevel = Math.Abs(currentPrice - alertLevel.minPrice);
                double deltaMaxLevel = Math.Abs(currentPrice - alertLevel.maxPrice);

                double price;
                if (deltaMinLevel <= deltaMaxLevel)
                    price = alertLevel.minPrice;
                else
                    price = alertLevel.maxPrice;

                alert = new Rect(
                    Canvas.Rect.Right - DrawH.AlertBellWidth - AlertOptions.BellOffset,
                    Canvas.GetY(price) - DrawH.AlertBellHalfHeight,
                    DrawH.AlertBellWidth,
                    DrawH.AlertBellHeight
                );

                if (extendedText != Rect.Empty)
                    extendedText.X -= alert.Width + AlertOptions.BellOffset;
            }

            if (extendedText.X < text.X)
            {
                extendedText = Rect.Empty;
                extendedLine = null;
            }
            if (alert != Rect.Empty && alert.X < text.X)
                alert = Rect.Empty;

            info = new PriceNoteInfo
            {
                ControlPoint0 = p0,

                Text = text,
                Line = line,
                ExtendedText = extendedText,
                ExtendedLine = extendedLine,
                Alert = alert,

                TextIntersectsWithCanvas = Canvas.Rect.IntersectsWith(text),
                LineIntersectsWithCanvas = line.IntersectsWith(Canvas.Rect),
                ExtendedTextIntersectsWithCanvas = Canvas.Rect.IntersectsWith(extendedText),
                ExtendedLineIntersectsWithCanvas =
                    extendedLine?.IntersectsWith(Canvas.Rect) ?? false,
                AlertIntersectsWithCanvas = Canvas.Rect.IntersectsWith(alert),

                Font = font,
                ExtendedFont = extendedFont,
            };

            isObjectInArea =
                info.TextIntersectsWithCanvas
                || info.LineIntersectsWithCanvas
                || info.ExtendedTextIntersectsWithCanvas
                || info.ExtendedLineIntersectsWithCanvas
                || info.AlertIntersectsWithCanvas;
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

            if (info.LineIntersectsWithCanvas && !_lineColor.IsTransparent)
                visual.DrawLine(_linePen, info.Line.p0, info.Line.p1);

            if (
                info.ExtendedLine != null
                && info.ExtendedLineIntersectsWithCanvas
                && !_extendedLineColor.IsTransparent
            )
                visual.DrawLine(
                    _extendedLinePen,
                    info.ExtendedLine.Value.p0,
                    info.ExtendedLine.Value.p1
                );

            if (!info.Text.IsEmpty && info.TextIntersectsWithCanvas && info.Font != null)
            {
                if (!_textBackgroundColor.IsTransparent)
                    visual.FillRectangle(_textBackgroundColorBrush, info.Text);
                if (!_textColor.IsTransparent)
                    visual.DrawString(
                        Text,
                        info.Font,
                        _textColorBrush,
                        info.Text,
                        XTextAlignment.Center
                    );
            }

            if (
                !info.ExtendedText.IsEmpty
                && info.ExtendedTextIntersectsWithCanvas
                && info.ExtendedFont != null
            )
            {
                if (!_extendedTextBackgroundColor.IsTransparent)
                    visual.FillRectangle(_extendedTextBackgroundColorBrush, info.ExtendedText);
                if (!_extendedTextColor.IsTransparent)
                    visual.DrawString(
                        Text,
                        info.ExtendedFont,
                        _extendedTextColorBrush,
                        info.ExtendedText,
                        XTextAlignment.Center
                    );
            }

            if (!info.Alert.IsEmpty && info.AlertIntersectsWithCanvas)
                DrawH.AlertBell(visual, info.Alert.X, info.Alert.Y);

            if (areControlPointsVisible)
            {
                labels.Add(new ObjectLabelInfo(ControlPoints[0].Y, PriceLabelColor));
                areControlPointsVisible = false;
            }
            else if (ShowOnPriceScale)
                labels.Add(new ObjectLabelInfo(ControlPoints[0].Y, PriceLabelColor));
        }

        public override void DrawControlPoints(DxVisualQueue visual)
        {
            if (info == null)
                return;

            areControlPointsVisible = true;

            if (Lock)
                DrawH.ControlPointLockedCorner(visual, info.ControlPoint0);
            else if (InMove)
            {
                if (Mouse.OverrideCursor != Cursors.Arrow)
                    Mouse.OverrideCursor = Cursors.Arrow;
            }
            else
            {
                DrawH.ControlPointCorner(visual, info.ControlPoint0);

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

            if (ShowExtendedLine || ShowExtendedText || Alert.IsActive)
                return true;

            return ControlPoints[0].X >= DataProvider.Dates[0];
        }

        protected override bool IsObjectInArea()
        {
            return isObjectInArea;
        }

        protected override bool InObject(int x, int y)
        {
            if (info == null)
                return false;

            return info.Text.Contains(x, y)
                || info.Line.IntersectsWith(x, y)
                || info.ExtendedText.Contains(x, y)
                || (info.ExtendedLine?.IntersectsWith(x, y) ?? false)
                || info.Alert.Contains(x, y);
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

            {
                double distance = info.Line.DistanceFrom(x, y);
                if (info.Line.Intersects(distance))
                    return (int)distance;
            }

            if (info.ExtendedLine != null)
            {
                double distance = info.ExtendedLine.Value.DistanceFrom(x, y);
                if (info.ExtendedLine.Value.Intersects(distance))
                    return (int)distance;
            }

            return DrawH.GetMinDistance(x, y, info.Text, info.ExtendedText, info.Alert);
        }

        public override void BeginDrag()
        {
            base.BeginDrag();

            lastControlPoints = new ObjectPoint[1]
            {
                new ObjectPoint { X = ControlPoints[0].X, Y = ControlPoints[0].Y },
            };
        }

        public override void DragObject(double dx, double dy)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                if (Math.Abs(dx) >= Math.Abs(dy))
                    dy = 0;
                else
                    dx = 0;
            }

            base.DragObject(dx, dy);

            if (dx == 0)
                ControlPoints[0].X = lastControlPoints[0].X;
            if (dy == 0)
                ControlPoints[0].Y = lastControlPoints[0].Y;
        }

        public override void ControlPointEditing(int index)
        {
            base.ControlPointEditing(index);

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                Point lp = ToPoint(lastControlPoints[index]);
                Point cp = ToPoint(ControlPoints[index]);

                if (Math.Abs(lp.X - cp.X) >= Math.Abs(lp.Y - cp.Y))
                    ControlPoints[index].Y = lastControlPoints[index].Y;
                else
                    ControlPoints[index].X = lastControlPoints[index].X;
            }
        }

        public override void CheckAlert(List<IndicatorBase> indicators)
        {
            if (!Alert.IsActive || DataProvider == null)
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
                    AlertUnit,
                    AlertDistance
                );
            else
                this.alertLevel.UpdatePropertiesIfNeeded(
                    AlertFrequency,
                    priceLevel,
                    AlertUnit,
                    AlertDistance
                );

            if (this.alertLevel.Check(price, index))
            {
                if (AlertManager.Check(DataProvider.Symbol.Code, AlertOptions.Throttle))
                {
                    string message = $"[PriceNote++] Price at {priceLevel}";

                    if (Text != "")
                        message += $", {Text}";

                    AddAlert(Alert, message);
                }
            }
        }

        public override void ApplyTheme(IChartTheme theme)
        {
            base.ApplyTheme(theme);

            TextColor = theme.ChartFontColor;
            TextBackgroundColor = theme.ChartBackColor;

            ExtendedTextColor = theme.ChartFontColor;
            ExtendedTextBackgroundColor = theme.ChartBackColor;

            ExtendedLineColor = theme.ChartObjectLineColor;

            PriceLabelColor = theme.ChartObjectLineColor;
        }

        public override void CopyTemplate(ObjectBase objectBase, bool style)
        {
            if (objectBase is PriceNote o)
            {
                Visibility = o.Visibility;

                Text = o.Text;
                TextFontSize = o.TextFontSize;
                TextVerticalAlignment = o.TextVerticalAlignment;
                TextHorizontalPadding = o.TextHorizontalPadding;
                TextVerticalPadding = o.TextVerticalPadding;
                TextColor = o.TextColor;
                TextBackgroundColor = o.TextBackgroundColor;

                LineDirection = o.LineDirection;
                LineColor = o.LineColor;
                LineWidth = o.LineWidth;
                LineThickness = o.LineThickness;
                LineStyle = o.LineStyle;

                ShowExtendedText = o.ShowExtendedText;
                ExtendedTextFontSize = o.ExtendedTextFontSize;
                ExtendedTextVerticalAlignment = o.ExtendedTextVerticalAlignment;
                ExtendedTextOffset = o.ExtendedTextOffset;
                ExtendedTextHorizontalPadding = o.ExtendedTextHorizontalPadding;
                ExtendedTextVerticalPadding = o.ExtendedTextVerticalPadding;
                ExtendedTextColor = o.ExtendedTextColor;
                ExtendedTextBackgroundColor = o.ExtendedTextBackgroundColor;

                ShowExtendedLine = o.ShowExtendedLine;
                ExtendedLineColor = o.ExtendedLineColor;
                ExtendedLineThickness = o.ExtendedLineThickness;
                ExtendedLineStyle = o.ExtendedLineStyle;

                ShowOnPriceScale = o.ShowOnPriceScale;
                PriceLabelColor = o.PriceLabelColor;

                Alert.Copy(o.Alert, !style);
                AlertDistance = o.AlertDistance;
                AlertUnit = o.AlertUnit;
                AlertFrequency = o.AlertFrequency;
                AlertOptions = o.AlertOptions.DeepCopy();
                OnPropertyChanged(nameof(Alert));
            }

            base.CopyTemplate(objectBase, style);
        }
    }
}
