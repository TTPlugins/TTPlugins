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
    [DataContract(Name = "TierZoomOptions")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal sealed class TierZoomOptions : INotifyPropertyChanged, DeepCopy<TierZoomOptions>
    {
        private double _reduceRatio;

        [DataMember(Name = "ReduceRatio")]
        [DisplayName("Reduce Ratio (Higher for Big Tiers)")]
        public double ReduceRatio
        {
            get => _reduceRatio;
            set
            {
                if (value == _reduceRatio)
                {
                    return;
                }

                _reduceRatio = value;

                OnPropertyChanged(nameof(ReduceRatio));
            }
        }

        private double _minRatio;

        [DataMember(Name = "MinRatio")]
        [DisplayName("Min Ratio (Higher for Big Tiers)")]
        public double MinRatio
        {
            get => _minRatio;
            set
            {
                if (value == _minRatio)
                {
                    return;
                }

                _minRatio = value;

                OnPropertyChanged(nameof(MinRatio));
            }
        }

        public TierZoomOptions()
        {
            ReduceRatio = 6.0;
            MinRatio = 0.7;
        }

        public TierZoomOptions DeepCopy()
        {
            return new TierZoomOptions { ReduceRatio = ReduceRatio, MinRatio = MinRatio };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"Reduce: {ReduceRatio} - Min: {MinRatio}";
        }
    }
}
