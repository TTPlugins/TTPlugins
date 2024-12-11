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

using System.ComponentModel;
using System.Runtime.Serialization;
using CustomCommon.Helpers;

namespace IndicatorsPlusPlus.Delta
{
    [ReadOnly(true)]
    [DataContract(Name = "GlobalZoomOptions")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal sealed class GlobalZoomOptions : INotifyPropertyChanged, DeepCopy<GlobalZoomOptions>
    {
        private bool _isEnabled;

        [DataMember(Name = "IsEnabled")]
        [DisplayName("Is Enabled")]
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (value == _isEnabled)
                {
                    return;
                }

                _isEnabled = value;

                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        private double _minDrawingWidth;

        [DataMember(Name = "MinDrawingWidth")]
        [DisplayName("Min Drawing Width")]
        public double MinDrawingWidth
        {
            get => _minDrawingWidth;
            set
            {
                if (value == _minDrawingWidth)
                {
                    return;
                }

                _minDrawingWidth = value;

                OnPropertyChanged(nameof(MinDrawingWidth));
            }
        }

        private double _minColumnWidth;

        [DataMember(Name = "MinColumnWidth")]
        [DisplayName("Min Column Width")]
        public double MinColumnWidth
        {
            get => _minColumnWidth;
            set
            {
                if (value == _minColumnWidth)
                {
                    return;
                }

                _minColumnWidth = value;

                OnPropertyChanged(nameof(MinColumnWidth));
            }
        }

        public GlobalZoomOptions()
        {
            IsEnabled = true;
            MinDrawingWidth = 1.3;
            MinColumnWidth = 2.0;
        }

        public GlobalZoomOptions DeepCopy()
        {
            return new GlobalZoomOptions
            {
                IsEnabled = IsEnabled,
                MinDrawingWidth = MinDrawingWidth,
                MinColumnWidth = MinColumnWidth,
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return IsEnabled ? "Enabled" : "Disabled";
        }
    }
}
