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
    [DataContract(Name = "AdvancedOptions")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal sealed class AdvancedOptions : INotifyPropertyChanged, DeepCopy<AdvancedOptions>
    {
        private bool _strictTiers;

        [DataMember(Name = "StrictTiers")]
        [DisplayName("Strict Tiers")]
        public bool StrictTiers
        {
            get => _strictTiers;
            set
            {
                if (value == _strictTiers)
                {
                    return;
                }

                _strictTiers = value;

                OnPropertyChanged(nameof(StrictTiers));
            }
        }

        private bool _hideMissingDataBanner;

        [DataMember(Name = "HideMissingDataBanner")]
        [DisplayName("Hide Missing Data Banner")]
        public bool HideMissingDataBanner
        {
            get => _hideMissingDataBanner;
            set
            {
                if (value == _hideMissingDataBanner)
                {
                    return;
                }

                _hideMissingDataBanner = value;

                OnPropertyChanged(nameof(HideMissingDataBanner));
            }
        }

        private GlobalZoomOptions _zoom;

        [DataMember(Name = "Zoom")]
        [DisplayName("Global Zoom Options for Tiers")]
        public GlobalZoomOptions Zoom
        {
            get => _zoom ?? (_zoom = new GlobalZoomOptions());
            private set
            {
                if (Equals(value, _zoom))
                {
                    return;
                }

                _zoom = value;

                OnPropertyChanged(nameof(Zoom));
            }
        }

        public AdvancedOptions()
        {
            StrictTiers = true;
            HideMissingDataBanner = false;
            Zoom = new GlobalZoomOptions();
        }

        public AdvancedOptions DeepCopy()
        {
            return new AdvancedOptions
            {
                StrictTiers = StrictTiers,
                HideMissingDataBanner = HideMissingDataBanner,
                Zoom = Zoom.DeepCopy(),
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return "";
        }
    }
}
