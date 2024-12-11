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
using System.ComponentModel;
using System.Runtime.Serialization;
using CustomCommon.Helpers;

namespace IndicatorsPlusPlus.Delta
{
    [ReadOnly(true)]
    [DataContract(Name = "DeltaTier")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal sealed class DeltaTier : INotifyPropertyChanged, DeepCopy<DeltaTier>
    {
        private bool _show;

        [DataMember(Name = "Show")]
        [DisplayName("Show")]
        public bool Show
        {
            get => _show;
            set
            {
                if (value == _show)
                {
                    return;
                }

                _show = value;

                Clear();
                OnPropertyChanged(nameof(Show));
            }
        }

        private double _size;

        [DataMember(Name = "Size")]
        [DisplayName("Size")]
        public double Size
        {
            get => _size;
            set
            {
                value = Math.Max(0, value);

                if (value == _size)
                {
                    return;
                }

                _size = value;

                Clear();
                OnPropertyChanged(nameof(Size));
            }
        }

        private double _drawingSize;

        [DataMember(Name = "DrawingSize")]
        [DisplayName("Drawing Size")]
        public double DrawingSize
        {
            get => _drawingSize;
            set
            {
                value = Math.Max(1, Math.Min(value, 300));

                if (value == _drawingSize)
                {
                    return;
                }

                _drawingSize = value;

                RecalculateTiers();
                OnPropertyChanged(nameof(DrawingSize));
            }
        }

        private string _paletteName;

        [DataMember(Name = "PaletteName")]
        [DisplayName("Palette Name")]
        public string PaletteName
        {
            get => _paletteName;
            set
            {
                if (value == _paletteName)
                {
                    return;
                }

                _paletteName = value;

                RecalculateTiers();
                OnPropertyChanged(nameof(PaletteName));
            }
        }

        private bool _showText;

        [DataMember(Name = "ShowText")]
        [DisplayName("Show Text")]
        public bool ShowText
        {
            get => _showText;
            set
            {
                if (value == _showText)
                {
                    return;
                }

                _showText = value;

                RecalculateTiers();
                OnPropertyChanged(nameof(ShowText));
            }
        }

        private TierZoomOptions _zoom;

        [DataMember(Name = "Zoom")]
        [DisplayName("Tier Zoom Options")]
        public TierZoomOptions Zoom
        {
            get => _zoom;
            set
            {
                if (value == _zoom)
                {
                    return;
                }

                _zoom = value;

                RecalculateTiers();
                OnPropertyChanged(nameof(Zoom));
            }
        }

        public DeltaTier()
        {
            Show = true;
            Size = 1500;
            DrawingSize = 10;
            PaletteName = "Default";
            ShowText = false;
            Zoom = new TierZoomOptions();
        }

        public DeltaTier DeepCopy()
        {
            return new DeltaTier
            {
                Show = Show,
                Size = Size,
                DrawingSize = DrawingSize,
                PaletteName = PaletteName,
                ShowText = ShowText,
                Zoom = Zoom.DeepCopy(),
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Action OnClear;

        private void Clear()
        {
            OnClear?.Invoke();
        }

        public Action OnRecalculateTiers;

        private void RecalculateTiers()
        {
            OnRecalculateTiers?.Invoke();
        }

        public override string ToString()
        {
            return $"{Size}+";
        }
    }
}
