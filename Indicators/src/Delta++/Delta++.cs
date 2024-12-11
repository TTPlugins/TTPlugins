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
using CustomCommon.Draw;
using CustomCommon.Helpers;
using TigerTrade.Chart.Alerts;
using TigerTrade.Chart.Data;
using TigerTrade.Chart.Indicators.Common;
using TigerTrade.Chart.Indicators.Enums;
using TigerTrade.Dx;
using TigerTrade.Dx.Enums;

namespace IndicatorsPlusPlus.Delta
{
    [DataContract(
        Name = "DeltaPlusPlusIndicator",
        Namespace = "http://schemas.datacontract.org/2004/07/TigerTrade.Chart.Indicators.Custom"
    )]
    [Indicator("DeltaPlusPlusIndicator", "*Delta++", true, Type = typeof(Delta))]
    internal sealed class Delta : IndicatorBase
    {
        public enum Shape
        {
            [EnumMember(Value = "Circle"), Description("Circle")]
            Circle,

            [EnumMember(Value = "Square"), Description("Square")]
            Square,

            [EnumMember(Value = "Diamond"), Description("Diamond")]
            Diamond,

            [EnumMember(Value = "Triangle"), Description("Triangle")]
            Triangle,

            [EnumMember(Value = "RoundedSquare"), Description("Rounded Square")]
            RoundedSquare,
        }

        private Shape _deltaShape;

        [DataMember(Name = "DeltaShape"), DefaultValue(Shape.Circle)]
        [Category("Delta"), DisplayName("Shape")]
        public Shape DeltaShape
        {
            get => _deltaShape;
            set
            {
                if (value == _deltaShape)
                {
                    return;
                }

                _deltaShape = value;

                OnPropertyChanged();
            }
        }

        private List<DeltaTier> _deltaTiers;

        [DataMember(Name = "DeltaTiers")]
        [Category("Delta"), DisplayName("Tiers")]
        public List<DeltaTier> DeltaTiers
        {
            get => _deltaTiers ?? (_deltaTiers = new List<DeltaTier>());
            set
            {
                value.Sort((a, b) => a.Size.CompareTo(b.Size));

                if (Equals(value, _deltaTiers))
                {
                    return;
                }

                _deltaTiers = value;
                foreach (DeltaTier d in _deltaTiers)
                {
                    d.OnClear = Clear;
                    d.OnRecalculateTiers = RecalculateDeltaTiers;
                }

                Clear();
                OnPropertyChanged();
            }
        }

        private double _deltaMaxSize;

        [DataMember(Name = "DeltaMaxSize")]
        [Category("Delta"), DisplayName("Max Size")]
        public double DeltaMaxSize
        {
            get => _deltaMaxSize;
            set
            {
                value = Math.Max(0, value);

                if (value == _deltaMaxSize)
                {
                    return;
                }

                _deltaMaxSize = value;

                Clear();
                OnPropertyChanged();
            }
        }

        private double _deltaMaxDrawingSize;

        [DataMember(Name = "DeltaMaxDrawingSize")]
        [Category("Delta"), DisplayName("Max Drawing Size")]
        public double DeltaMaxDrawingSize
        {
            get => _deltaMaxDrawingSize;
            set
            {
                value = Math.Max(1, Math.Min(value, 300));

                if (value == _deltaMaxDrawingSize)
                {
                    return;
                }

                _deltaMaxDrawingSize = value;

                RecalculateDeltaTiers();
                OnPropertyChanged();
            }
        }

        private List<Palette> _palettes;

        [DataMember(Name = "Palettes")]
        [Category("Style"), DisplayName("Palettes")]
        public List<Palette> Palettes
        {
            get => _palettes ?? (_palettes = new List<Palette>());
            set
            {
                value.Sort((a, b) => a.Name.CompareTo(b.Name));

                if (Equals(value, _palettes))
                {
                    return;
                }

                _palettes = value;
                foreach (Palette p in _palettes)
                {
                    p.OnRecalculateTiers = RecalculateDeltaTiers;
                }

                RecalculateDeltaTiers();
                OnPropertyChanged();
            }
        }

        private ChartAlertSettings _alert;

        [DataMember(Name = "Alert"), Browsable(true)]
        [Category("Alert"), DisplayName("Alert")]
        public ChartAlertSettings Alert
        {
            get => _alert ?? (_alert = new ChartAlertSettings());
            set
            {
                if (Equals(value, _alert))
                {
                    return;
                }

                _alert = value;

                OnPropertyChanged();
            }
        }

        private int _alertTier;

        [DataMember(Name = "AlertTier")]
        [Category("Alert"), DisplayName("Alert Tier (Starts at 0)")]
        public int AlertTier
        {
            get => _alertTier;
            set
            {
                value = Math.Max(0, value);

                if (value == _alertTier)
                {
                    return;
                }

                _alertTier = value;

                OnPropertyChanged();
            }
        }

        private AdvancedOptions _advancedOptions;

        [DataMember(Name = "AdvancedOptions")]
        [Category("Indicator"), DisplayName("Advanced Options")]
        public AdvancedOptions AdvancedOptions
        {
            get => _advancedOptions ?? (_advancedOptions = new AdvancedOptions());
            private set
            {
                if (Equals(value, _advancedOptions))
                {
                    return;
                }

                _advancedOptions = value;

                OnPropertyChanged();
            }
        }

        [Browsable(false)]
        public override bool ShowIndicatorTitle => false;

        [Browsable(false)]
        public override bool ShowIndicatorValues => false;

        [Browsable(false)]
        public override bool ShowIndicatorLabels => false;

        [Browsable(false)]
        public override IndicatorCalculation Calculation => IndicatorCalculation.OnEachTick;

        private int _lastIndex = 0;

        private List<(int index, int tierIndex, double price, double delta)> _deltas;
        private List<(int index, int tierIndex, double price, double delta)> _realtimeDeltas;
        private HashSet<long> _realtimeAlerts;
        private List<(Rect rect, int tierIndex, double price, double delta)> _hitboxes;

        private struct CacheDeltaTier
        {
            public bool Show;
            public bool ShowText;
            public double MinSize;
            public double MaxSize;
            public double MinDrawingSize;
            public double MaxDrawingSize;
            public double TextLineWidth;
            public XFont Font;
            public CacheDeltaTierSide Ask;
            public CacheDeltaTierSide Bid;
            public TierZoomOptions Zoom;
        }

        private struct CacheDeltaTierSide
        {
            public Palette.PaletteTextAlignment TextAlignment;
            public XBrush Background;
            public XPen Border;
            public XPen TextLine;
        }

        private CacheDeltaTier[] _cacheDeltaTiers;

        private void RecalculateDeltaTiers()
        {
            _cacheDeltaTiers = new CacheDeltaTier[DeltaTiers.Count];

            if (DeltaTiers.Count == 0)
                return;

            double absoluteMaxSize = Math.Max(DeltaTiers[0].Size, DeltaMaxSize);
            double absoluteMaxDrawingSize = Math.Max(
                DeltaTiers[0].DrawingSize,
                DeltaMaxDrawingSize
            );
            (double size, int tierIndex)[] values = new (double size, int tierIndex)[
                DeltaTiers.Count
            ];

            for (int i = 0; i < DeltaTiers.Count; i++)
            {
                DeltaTier tier = DeltaTiers[i];

                if (tier.Size > absoluteMaxSize)
                    absoluteMaxSize = tier.Size;
                if (tier.DrawingSize > absoluteMaxDrawingSize)
                    absoluteMaxDrawingSize = tier.DrawingSize;

                values[i] = (tier.Size, i);
            }

            // Sort by size (Descending, Bigger first)
            Array.Sort(values, (a, b) => b.size.CompareTo(a.size));

            for (int i = 0; i < values.Length; i++)
            {
                double maxSize;
                double maxDrawingSize;
                if (i == 0)
                {
                    maxSize = absoluteMaxSize;
                    maxDrawingSize = absoluteMaxDrawingSize;
                }
                else
                {
                    maxSize = _cacheDeltaTiers[i - 1].MinSize;
                    maxDrawingSize = _cacheDeltaTiers[i - 1].MinDrawingSize;
                }

                DeltaTier tier = DeltaTiers[values[i].tierIndex];
                Palette p =
                    Palettes.Count > 0
                        ? Palettes.Find(palette => palette.Name == tier.PaletteName) ?? Palettes[0]
                        : new Palette();

                _cacheDeltaTiers[i] = new CacheDeltaTier
                {
                    Show = tier.Show,
                    ShowText = tier.ShowText,
                    MinSize = tier.Size,
                    MaxSize = Math.Max(maxSize, tier.Size),
                    MinDrawingSize = tier.DrawingSize,
                    MaxDrawingSize = Math.Max(maxDrawingSize, tier.DrawingSize),
                    TextLineWidth = p.TextLineWidth,
                    Font = new XFont(Canvas?.ChartFont?.Name ?? "Inter", p.FontSize),
                    Ask = new CacheDeltaTierSide
                    {
                        TextAlignment = p.AskTextAlignment,
                        Background = p.AskBackgroundBrush,
                        Border = p.AskBorderPen,
                        TextLine = p.AskTextLinePen,
                    },
                    Bid = new CacheDeltaTierSide
                    {
                        TextAlignment = p.BidTextAlignment,
                        Background = p.BidBackgroundBrush,
                        Border = p.BidBorderPen,
                        TextLine = p.BidTextLinePen,
                    },
                    Zoom = tier.Zoom,
                };
            }
        }

        public Delta()
        {
            DeltaShape = Shape.Circle;
            DeltaTiers = new List<DeltaTier>(5)
            {
                new DeltaTier
                {
                    Show = true,
                    ShowText = false,
                    Size = 150,
                    DrawingSize = 5,
                    PaletteName = "Palette1",
                    Zoom = new TierZoomOptions { ReduceRatio = 2.0, MinRatio = 0.4 },
                },
                new DeltaTier
                {
                    Show = true,
                    ShowText = false,
                    Size = 400,
                    DrawingSize = 5,
                    PaletteName = "Palette2",
                    Zoom = new TierZoomOptions { ReduceRatio = 3.0, MinRatio = 0.5 },
                },
                new DeltaTier
                {
                    Show = true,
                    ShowText = false,
                    Size = 800,
                    DrawingSize = 10,
                    PaletteName = "Palette3",
                    Zoom = new TierZoomOptions { ReduceRatio = 4.0, MinRatio = 0.6 },
                },
                new DeltaTier
                {
                    Show = true,
                    ShowText = false,
                    Size = 1500,
                    DrawingSize = 14,
                    PaletteName = "Palette3",
                    Zoom = new TierZoomOptions { ReduceRatio = 5.0, MinRatio = 0.7 },
                },
                new DeltaTier
                {
                    Show = true,
                    ShowText = false,
                    Size = 3000,
                    DrawingSize = 19,
                    PaletteName = "Palette3",
                    Zoom = new TierZoomOptions { ReduceRatio = 6.0, MinRatio = 0.7 },
                },
            };
            DeltaMaxSize = 6000;
            DeltaMaxDrawingSize = 25;

            Palettes = new List<Palette>(3)
            {
                new Palette
                {
                    Name = "Palette1",
                    AskBackgroundColor = Colors.Transparent,
                    AskBorderColor = Colors.SeaGreen,
                    BidBackgroundColor = Colors.Transparent,
                    BidBorderColor = Colors.DarkRed,
                    BorderThickness = 1,
                    BorderStyle = XDashStyle.Solid,
                    AskTextAlignment = Palette.PaletteTextAlignment.Left,
                    BidTextAlignment = Palette.PaletteTextAlignment.Right,
                    FontSize = 14,
                    TextLineWidth = 15,
                    TextLineThickness = 1,
                    TextLineStyle = XDashStyle.Solid,
                },
                new Palette
                {
                    Name = "Palette2",
                    AskBackgroundColor = Colors.SeaGreen,
                    AskBorderColor = Colors.SeaGreen,
                    BidBackgroundColor = Colors.DarkRed,
                    BidBorderColor = Colors.DarkRed,
                    BorderThickness = 1,
                    BorderStyle = XDashStyle.Solid,
                    AskTextAlignment = Palette.PaletteTextAlignment.Left,
                    BidTextAlignment = Palette.PaletteTextAlignment.Right,
                    FontSize = 14,
                    TextLineWidth = 15,
                    TextLineThickness = 1,
                    TextLineStyle = XDashStyle.Solid,
                },
                new Palette
                {
                    Name = "Palette3",
                    AskBackgroundColor = Colors.Gold,
                    AskBorderColor = Colors.Gold,
                    BidBackgroundColor = Colors.DarkMagenta,
                    BidBorderColor = Colors.DarkMagenta,
                    BorderThickness = 1,
                    BorderStyle = XDashStyle.Solid,
                    AskTextAlignment = Palette.PaletteTextAlignment.Left,
                    BidTextAlignment = Palette.PaletteTextAlignment.Right,
                    FontSize = 14,
                    TextLineWidth = 15,
                    TextLineThickness = 1,
                    TextLineStyle = XDashStyle.Solid,
                },
            };

            AlertTier = 2;

            AdvancedOptions = new AdvancedOptions();
        }

        private bool? _executedStrictTiers = null;
        private int? _executedDeltaTiersCount = null;
        private int? _executedPalettesCount = null;

        private void Clear()
        {
            _lastIndex = 0;
            _deltas?.Clear();
            _realtimeDeltas?.Clear();
            _hitboxes?.Clear();

            RecalculateDeltaTiers();
        }

        protected override void Execute()
        {
            if (ClearData)
            {
                Clear();
            }

            bool recalculateTiers = false;
            if (AdvancedOptions.StrictTiers != _executedStrictTiers)
            {
                _executedStrictTiers = AdvancedOptions.StrictTiers;
                _lastIndex = 0;
                _deltas?.Clear();
                _realtimeDeltas?.Clear();
                _hitboxes?.Clear();

                recalculateTiers = true;
            }
            if (_deltaTiers.Count != _executedDeltaTiersCount)
            {
                _executedDeltaTiersCount = _deltaTiers.Count;
                foreach (DeltaTier d in _deltaTiers)
                {
                    d.OnClear = Clear;
                    d.OnRecalculateTiers = RecalculateDeltaTiers;
                }
                _lastIndex = 0;
                _deltas?.Clear();
                _realtimeDeltas?.Clear();
                _hitboxes?.Clear();

                recalculateTiers = true;
            }
            if (_palettes.Count != _executedPalettesCount)
            {
                _executedPalettesCount = _palettes.Count;
                foreach (Palette p in _palettes)
                {
                    p.OnRecalculateTiers = RecalculateDeltaTiers;
                }
                recalculateTiers = true;
            }

            double scale = DataProvider.Scale;
            double ticksize = DataProvider.Step / scale;
            double precision = (double)DataProvider.Symbol.GetSize(1);

            int count = DataProvider.Count;

            if (_cacheDeltaTiers == null || recalculateTiers)
                RecalculateDeltaTiers();
            if (_deltas == null)
                _deltas = new List<(int index, int tierIndex, double price, double delta)>(count);
            _realtimeDeltas = new List<(int index, int tierIndex, double price, double delta)>(30);

            double? alertSize = null;
            if (Alert.IsActive)
            {
                int tier = _cacheDeltaTiers.Length - AlertTier - 1;
                if (tier >= 0 && tier < _cacheDeltaTiers.Length)
                    alertSize = _cacheDeltaTiers[tier].MinSize;
            }

            int realtimeIndex = count - 1;
            int deltaTiersLength = _cacheDeltaTiers.Length;
            bool strictTiers = AdvancedOptions.StrictTiers;

            if (_lastIndex != realtimeIndex || _realtimeAlerts == null)
                _realtimeAlerts = new HashSet<long>();

            for (int i = _lastIndex; i < count; i++)
            {
                IRawCluster rawCluster = DataProvider.GetRawCluster(i);
                if (rawCluster == null)
                    continue;

                bool isRealtime = i == realtimeIndex;
                IReadOnlyCollection<IRawClusterItem> items = rawCluster.Items;
                foreach (IRawClusterItem item in items)
                {
                    double delta = (item.Ask - item.Bid) * precision;
                    for (int t = 0; t < deltaTiersLength; t++)
                    {
                        ref CacheDeltaTier c = ref _cacheDeltaTiers[t];
                        if (
                            c.Show
                            && (delta <= -c.MinSize || delta >= c.MinSize)
                            && (
                                strictTiers && t != 0
                                    ? delta > -c.MaxSize && delta < c.MaxSize
                                    : true
                            )
                        )
                        {
                            double price = item.Price * ticksize;

                            if (isRealtime)
                                _realtimeDeltas.Add((i, t, price, delta));
                            else
                                _deltas.Add((i, t, price, delta));
                            break;
                        }
                    }

                    if (i == realtimeIndex && alertSize != null && count > 10)
                    {
                        if (delta <= -alertSize || delta >= alertSize)
                        {
                            if (!_realtimeAlerts.Contains(item.Price))
                            {
                                AddAlert(Alert, $"Delta {delta} → {item.Price * ticksize * scale}");
                                _realtimeAlerts.Add(item.Price);
                            }
                        }
                    }
                }
            }

            _lastIndex = Math.Max(realtimeIndex, 0);

            // NOTE: Just to make sure that the action bindings are set.
            // This is a workaround and these bindings shouldn't be set
            // here for performance reasons. This is a temporary solution.
            //
            // Steps to reproduce the issue:
            // - Comment the two following foreach loops.
            // - Add the Indicator to the chart.
            //   (Bindings work as expected)
            // - Restart the platform.
            // - The First Action works as expected but not afterwards.
            //   (Bindings are cleared after first action)
            foreach (DeltaTier d in _deltaTiers)
            {
                d.OnClear = Clear;
                d.OnRecalculateTiers = RecalculateDeltaTiers;
            }
            foreach (Palette p in _palettes)
            {
                p.OnRecalculateTiers = RecalculateDeltaTiers;
            }
        }

        public override void Render(DxVisualQueue visual)
        {
            base.Render(visual);

            // Check for Clusters Data Type
            if (DataProvider?.GetRawCluster(0)?.Items?.Count == 0)
            {
                if (AdvancedOptions.HideMissingDataBanner)
                    return;

                XFont font = Canvas.ChartFont;
                string text = "Delta++ Indicator | Missing Data → Change Data Type to Clusters.";
                Size textSize = font.GetSize(text);

                double x = (Canvas.Rect.Width - textSize.Width) / 2;
                double y = 30;
                double horizontalPadding = 0;
                double verticalPadding = 20;
                Point radius = new Point(5, 5);
                Rect rect = new Rect(
                    x,
                    y,
                    textSize.Width + horizontalPadding,
                    textSize.Height + verticalPadding
                );

                // Draw Box
                visual.FillRoundedRectangle(new XBrush(new XColor(255, 20, 20, 20)), rect, radius);
                visual.DrawRoundedRectangle(
                    new XPen(new XBrush(new XColor(255, 40, 40, 40)), 1),
                    rect,
                    radius
                );

                // Draw Text
                visual.DrawString(text, font, new XBrush(Colors.Red), rect, XTextAlignment.Center);
                return;
            }

            // Reset Hitboxes
            _hitboxes = new List<(Rect rect, int tierIndex, double price, double delta)>();

            // Draw Views
            DrawRealtimeDeltaView(visual);
            DrawDeltaView(visual);
        }

        private double GetDecrementRatio(double minColumnWidth, double reduceRatio, double minRatio)
        {
            if (Canvas.ColumnWidth < minColumnWidth)
            {
                return Math.Max(
                    1.0 - ((minColumnWidth - Canvas.ColumnWidth) / reduceRatio),
                    minRatio
                );
            }
            else
            {
                return 1.0;
            }
        }

        private void DrawDeltaView(DxVisualQueue visual)
        {
            double scale = DataProvider.Scale;

            int deltasCount = _deltas.Count;
            for (int i = 0; i < deltasCount; i++)
            {
                (int index, int tierIndex, double price, double delta) = _deltas[i];
                double x = GetX(index);
                double y = GetY(price * scale);
                if (!Canvas.Rect.Contains(new Point(x, y)))
                    continue;

                bool isAsk = delta >= 0;
                ref CacheDeltaTier c = ref _cacheDeltaTiers[tierIndex];
                ref CacheDeltaTierSide cs = ref (isAsk ? ref c.Ask : ref c.Bid);

                double multiplier = Math.Max(
                    0,
                    Math.Min((Math.Abs(delta) - c.MinSize) / (c.MaxSize - c.MinSize), 1)
                );
                double width =
                    (c.MinDrawingSize + ((c.MaxDrawingSize - c.MinDrawingSize) * multiplier)) / 2.0;

                // Apply Zoom Options
                if (AdvancedOptions.Zoom.IsEnabled)
                {
                    width *= GetDecrementRatio(
                        AdvancedOptions.Zoom.MinColumnWidth,
                        c.Zoom.ReduceRatio,
                        c.Zoom.MinRatio
                    );
                    if (width < AdvancedOptions.Zoom.MinDrawingWidth)
                        continue;
                }

                // Add Hitbox
                _hitboxes.Add(
                    (
                        rect: new Rect(
                            new Point(x - width, y - width),
                            new Point(x + width, y + width)
                        ),
                        tierIndex,
                        price,
                        delta
                    )
                );

                // Draw Line
                bool isTextAlignmentLeft = cs.TextAlignment == Palette.PaletteTextAlignment.Left;
                if (c.ShowText)
                {
                    visual.DrawLine(
                        cs.TextLine,
                        new Point(x, y),
                        new Point(
                            x
                                + (
                                    isTextAlignmentLeft
                                        ? -width - c.TextLineWidth
                                        : width + c.TextLineWidth
                                ),
                            y
                        )
                    );
                }

                Shape shape = DeltaShape;

                // Draw Shape
                if (shape == Shape.Circle)
                {
                    visual.FillEllipse(cs.Background, new Point(x, y), width, width);
                    visual.DrawEllipse(cs.Border, new Point(x, y), width, width);
                }
                else if (shape == Shape.Square)
                {
                    Rect rect = new Rect(x - width, y - width, width * 2, width * 2);
                    visual.FillRectangle(cs.Background, rect);
                    visual.DrawRectangle(cs.Border, rect);
                }
                else if (shape == Shape.Diamond)
                {
                    Point[] points = new Point[4]
                    {
                        new Point(x - width, y),
                        new Point(x, y - width),
                        new Point(x + width, y),
                        new Point(x, y + width),
                    };
                    visual.FillPolygon(cs.Background, points);
                    visual.DrawPolygon(cs.Border, points);
                }
                else if (shape == Shape.Triangle)
                {
                    Point[] points = new Point[3]
                    {
                        new Point(x, y + (isAsk ? -width : width)),
                        new Point(x + width, y + (isAsk ? width : -width)),
                        new Point(x - width, y + (isAsk ? width : -width)),
                    };
                    visual.FillPolygon(cs.Background, points);
                    visual.DrawPolygon(cs.Border, points);
                }
                else if (shape == Shape.RoundedSquare)
                {
                    double radius = width / 2;
                    Rect rect = new Rect(x - width, y - width, width * 2, width * 2);
                    visual.FillRoundedRectangle(cs.Background, rect, new Point(radius, radius));
                    visual.DrawRoundedRectangle(cs.Border, rect, new Point(radius, radius));
                }

                // Draw Text
                if (c.ShowText)
                {
                    string text = Format.CompactNumberNoPostfix(Math.Abs(delta), 1);
                    Size size = c.Font.GetSize(text);
                    double textX =
                        x
                        + (
                            isTextAlignmentLeft
                                ? -width - c.TextLineWidth - size.Width
                                : width + c.TextLineWidth
                        );
                    double textY = y - (size.Height / 2);

                    visual.FillRectangle(
                        Canvas.Theme.ChartBackBrush,
                        new Rect(textX, textY, size.Width, size.Height)
                    );
                    visual.DrawString(
                        text,
                        c.Font,
                        cs.TextLine.Brush,
                        new Rect(
                            new Point(textX, textY),
                            new Point(textX + size.Width, textY + size.Height)
                        )
                    );
                }
            }
        }

        private void DrawRealtimeDeltaView(DxVisualQueue visual)
        {
            double scale = DataProvider.Scale;

            int deltasCount = _realtimeDeltas.Count;
            for (int i = 0; i < deltasCount; i++)
            {
                (int index, int tierIndex, double price, double delta) = _realtimeDeltas[i];
                double x = GetX(index);
                double y = GetY(price * scale);
                if (!Canvas.Rect.Contains(new Point(x, y)))
                    continue;

                bool isAsk = delta >= 0;
                ref CacheDeltaTier c = ref _cacheDeltaTiers[tierIndex];
                ref CacheDeltaTierSide cs = ref (isAsk ? ref c.Ask : ref c.Bid);

                double multiplier = Math.Max(
                    0,
                    Math.Min((Math.Abs(delta) - c.MinSize) / (c.MaxSize - c.MinSize), 1)
                );
                double width =
                    (c.MinDrawingSize + ((c.MaxDrawingSize - c.MinDrawingSize) * multiplier)) / 2.0;

                // Apply Zoom Options
                if (AdvancedOptions.Zoom.IsEnabled)
                {
                    width *= GetDecrementRatio(
                        AdvancedOptions.Zoom.MinColumnWidth,
                        c.Zoom.ReduceRatio,
                        c.Zoom.MinRatio
                    );
                    if (width < AdvancedOptions.Zoom.MinDrawingWidth)
                        continue;
                }

                // Add Hitbox
                _hitboxes.Add(
                    (
                        rect: new Rect(
                            new Point(x - width, y - width),
                            new Point(x + width, y + width)
                        ),
                        tierIndex,
                        price,
                        delta
                    )
                );

                // Draw Line
                bool isTextAlignmentLeft = cs.TextAlignment == Palette.PaletteTextAlignment.Left;
                if (c.ShowText)
                {
                    visual.DrawLine(
                        cs.TextLine,
                        new Point(x, y),
                        new Point(
                            x
                                + (
                                    isTextAlignmentLeft
                                        ? -width - c.TextLineWidth
                                        : width + c.TextLineWidth
                                ),
                            y
                        )
                    );
                }

                Shape shape = DeltaShape;

                // Draw Shape
                if (shape == Shape.Circle)
                {
                    visual.FillEllipse(cs.Background, new Point(x, y), width, width);
                    visual.DrawEllipse(cs.Border, new Point(x, y), width, width);
                }
                else if (shape == Shape.Square)
                {
                    Rect rect = new Rect(x - width, y - width, width * 2, width * 2);
                    visual.FillRectangle(cs.Background, rect);
                    visual.DrawRectangle(cs.Border, rect);
                }
                else if (shape == Shape.Diamond)
                {
                    Point[] points = new Point[4]
                    {
                        new Point(x - width, y),
                        new Point(x, y - width),
                        new Point(x + width, y),
                        new Point(x, y + width),
                    };
                    visual.FillPolygon(cs.Background, points);
                    visual.DrawPolygon(cs.Border, points);
                }
                else if (shape == Shape.Triangle)
                {
                    Point[] points = new Point[3]
                    {
                        new Point(x, y + (isAsk ? -width : width)),
                        new Point(x + width, y + (isAsk ? width : -width)),
                        new Point(x - width, y + (isAsk ? width : -width)),
                    };
                    visual.FillPolygon(cs.Background, points);
                    visual.DrawPolygon(cs.Border, points);
                }
                else if (shape == Shape.RoundedSquare)
                {
                    double radius = width / 2;
                    Rect rect = new Rect(x - width, y - width, width * 2, width * 2);
                    visual.FillRoundedRectangle(cs.Background, rect, new Point(radius, radius));
                    visual.DrawRoundedRectangle(cs.Border, rect, new Point(radius, radius));
                }

                // Draw Text
                if (c.ShowText)
                {
                    string text = Format.CompactNumberNoPostfix(Math.Abs(delta), 1);
                    Size size = c.Font.GetSize(text);
                    double textX =
                        x
                        + (
                            isTextAlignmentLeft
                                ? -width - c.TextLineWidth - size.Width
                                : width + c.TextLineWidth
                        );
                    double textY = y - (size.Height / 2);

                    visual.FillRectangle(
                        Canvas.Theme.ChartBackBrush,
                        new Rect(textX, textY, size.Width, size.Height)
                    );
                    visual.DrawString(
                        text,
                        c.Font,
                        cs.TextLine.Brush,
                        new Rect(
                            new Point(textX, textY),
                            new Point(textX + size.Width, textY + size.Height)
                        )
                    );
                }
            }
        }

        public override void RenderCursor(
            DxVisualQueue visual,
            int cursorPos,
            Point cursorCenter,
            ref int topPos
        )
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == 0 || _hitboxes.Count == 0)
                return;

            List<(double absDelta, (string text, XBrush foreground)[] texts)> textLabels =
                new List<(double absDelta, (string text, XBrush foreground)[] texts)>();

            double scale = DataProvider.Scale;
            string priceFormat = Format.Price(DataProvider);

            XFont font = Canvas.ChartFont;
            double maxWidth = 0.0;

            string separator = " → ";
            double separatorWidth = font.GetSize(separator).Width;

            XBrush dimGrayBrush = new XBrush(Colors.DimGray);
            XBrush cornflowerBlueBrush = new XBrush(Colors.CornflowerBlue);

            foreach ((Rect rect, int tierIndex, double price, double delta) in _hitboxes)
            {
                if (!rect.Contains(cursorCenter))
                    continue;

                string deltaText = Format.CompactNumber(delta, 2);
                string priceText = (price * scale).ToString(priceFormat);
                maxWidth = Math.Max(
                    maxWidth,
                    font.GetSize(deltaText).Width + separatorWidth + font.GetSize(priceText).Width
                );

                ref CacheDeltaTier c = ref _cacheDeltaTiers[tierIndex];
                ref CacheDeltaTierSide cs = ref (delta >= 0 ? ref c.Ask : ref c.Bid);
                XBrush foreground =
                    cs.Background.Color.Alpha != 0 ? cs.Background : cs.Border.Brush;

                textLabels.Add(
                    (
                        absDelta: Math.Abs(delta),
                        texts: new (string text, XBrush foreground)[]
                        {
                            (deltaText, foreground),
                            (separator, dimGrayBrush),
                            (priceText, cornflowerBlueBrush),
                        }
                    )
                );
            }

            if (textLabels.Count == 0)
                return;

            // Sort by absDelta (Descending, Bigger first)
            textLabels.Sort((a, b) => b.absDelta.CompareTo(a.absDelta));

            double marginX = 15.0;
            double marginY = 15.0;
            double paddingX = 6.0;
            double paddingY = 6.0;
            double gap = 4.0;
            double fontHeight = font.GetHeight();
            double width = maxWidth + paddingX;
            double height =
                (textLabels.Count * fontHeight) + (gap * (textLabels.Count - 1)) + (paddingY * 2);

            double x = cursorCenter.X + marginX;
            double y = cursorCenter.Y + marginY + topPos;
            if (x + width >= Canvas.Rect.Right)
            {
                x -= width + marginX * 2;
            }
            if (y + height >= Canvas.Rect.Bottom)
            {
                y -= height + marginY * 2;
            }

            Rect box = new Rect(x, y, width, height);
            visual.FillRectangle(Canvas.Theme.ChartBackBrush, box);
            visual.DrawRectangle(new XPen(new XBrush(new XColor(50, 255, 255, 255)), 1), box);

            x += paddingX;
            y += paddingY;
            foreach ((double absDelta, (string text, XBrush foreground)[] texts) in textLabels)
            {
                BetterDraw.DrawString(visual, font, texts, x, y);
                y += font.GetHeight() + gap;
            }
        }

        public override IndicatorTitleInfo GetTitle()
        {
            return new IndicatorTitleInfo(Title, new XBrush(new XColor(255, 255, 255, 255)));
        }

        public override void CopyTemplate(IndicatorBase indicator, bool style)
        {
            Delta i = (Delta)indicator;

            Alert.Copy(i.Alert, !style);
            OnPropertyChanged(nameof(Alert));

            DeltaShape = i.DeltaShape;
            DeltaTiers = Memory.DeepCopy(i.DeltaTiers);
            DeltaMaxSize = i.DeltaMaxSize;
            DeltaMaxDrawingSize = i.DeltaMaxDrawingSize;

            Palettes = Memory.DeepCopy(i.Palettes);

            AlertTier = i.AlertTier;

            AdvancedOptions = i.AdvancedOptions.DeepCopy();

            base.CopyTemplate(indicator, style);
        }

        public override string ToString()
        {
            return $"*Delta++";
        }
    }
}
