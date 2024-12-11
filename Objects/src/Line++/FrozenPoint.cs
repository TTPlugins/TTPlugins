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

namespace ObjectsPlusPlus.Line
{
    [ReadOnly(true)]
    [DataContract(Name = "FrozenPoint")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    internal sealed class FrozenPoint : INotifyPropertyChanged
    {
        public double x;
        public double y;

        [DataMember(Name = "X")]
        [DisplayName("X")]
        public double X
        {
            get => x;
            set
            {
                if (value == x)
                    return;

                x = value;

                OnPropertyChanged(nameof(X));
            }
        }

        [DataMember(Name = "Y")]
        [DisplayName("Y")]
        public double Y
        {
            get => y;
            set
            {
                if (value == y)
                    return;

                y = value;

                OnPropertyChanged(nameof(Y));
            }
        }

        public FrozenPoint()
        {
            X = 100;
            Y = 100;
        }

        public void FrozenPointChanged()
        {
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
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
