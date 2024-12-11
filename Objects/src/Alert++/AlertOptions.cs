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
using TigerTrade.Dx;
using TigerTrade.Dx.Fonts;

namespace ObjectsPlusPlus.Alert
{
    [ReadOnly(true)]
    [DataContract(Name = "AlertOptions")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal sealed class AlertOptions : INotifyPropertyChanged, DeepCopy<AlertOptions>
    {
        public static string AlertBellText = char.ConvertFromUtf32((int)FontAwesomeIcon.Bell);
        public static string AlertBellInactiveText = char.ConvertFromUtf32(
            (int)FontAwesomeIcon.BellSlash
        );
        public XFont AlertBellFont;
        public double AlertBellWidth;
        public double AlertBellHalfWidth;
        public double AlertBellInactiveWidth;
        public double AlertBellInactiveHalfWidth;
        public double AlertBellHeight;
        public double AlertBellHalfHeight;

        private double _bellSize;

        [DataMember(Name = "BellSize")]
        [DisplayName("Bell Size")]
        public double BellSize
        {
            get => _bellSize;
            set
            {
                value = Math.Max(0, value);

                if (value == _bellSize)
                    return;

                _bellSize = value;

                AlertBellFont = new XFont("FontAwesome", _bellSize);
                AlertBellWidth = AlertBellFont.GetWidth(AlertBellText);
                AlertBellHalfWidth = AlertBellWidth / 2;
                AlertBellInactiveWidth = AlertBellFont.GetWidth(AlertBellInactiveText);
                AlertBellInactiveHalfWidth = AlertBellInactiveWidth / 2;
                AlertBellHeight = AlertBellFont.GetHeight();
                AlertBellHalfHeight = AlertBellHeight / 2;

                OnPropertyChanged(nameof(BellSize));
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
            BellSize = 14;
            BellOffset = 5;
            Throttle = 3000;
        }

        public AlertOptions DeepCopy()
        {
            return new AlertOptions
            {
                BellSize = BellSize,
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
