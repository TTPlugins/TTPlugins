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

using System.Collections.Generic;

namespace CustomCommon.Helpers
{
    public interface DeepCopy<T>
    {
        public T DeepCopy();
    }

    public class Memory
    {
        public static List<T> DeepCopy<T>(List<T> list)
            where T : DeepCopy<T>
        {
            int count = list.Count;

            List<T> copy = new List<T>(count);

            for (int i = 0; i < count; i++)
            {
                copy.Add(list[i].DeepCopy());
            }

            return copy;
        }
    }
}
