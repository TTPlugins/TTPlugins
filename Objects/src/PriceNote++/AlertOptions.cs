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

namespace ObjectsPlusPlus.PriceNote
{
    [ReadOnly(true)]
    [DataContract(Name = "AlertOptions")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal sealed class AlertOptions : INotifyPropertyChanged, DeepCopy<AlertOptions>
    {
        private bool _showBell;

        [DataMember(Name = "ShowBell")]
        [DisplayName("Show Bell")]
        public bool ShowBell
        {
            get => _showBell;
            set
            {
                if (value == _showBell)
                    return;

                _showBell = value;

                OnPropertyChanged(nameof(ShowBell));
            }
        }

        private double _bellOffset;

        [DataMember(Name = "BellOffset")]
        [DisplayName("Bell Offset")]
        public double BellOffset
        {
            get => _bellOffset;
            set
            {
                if (value == _bellOffset)
                    return;

                _bellOffset = value;

                OnPropertyChanged(nameof(BellOffset));
            }
        }

        private int _throttle;

        [DataMember(Name = "Throttle")]
        [DisplayName("Throttle (ms)")]
        public int Throttle
        {
            get => _throttle;
            set
            {
                value = Math.Max(0, value);

                if (value == _throttle)
                    return;

                _throttle = value;

                OnPropertyChanged(nameof(Throttle));
            }
        }

        public AlertOptions()
        {
            ShowBell = true;
            BellOffset = 5;
            Throttle = 3000;
        }

        public AlertOptions DeepCopy()
        {
            return new AlertOptions
            {
                ShowBell = ShowBell,
                BellOffset = BellOffset,
                Throttle = Throttle,
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return "";
        }
    }
}
