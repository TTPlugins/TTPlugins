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

using System.Windows.Input;

namespace CustomCommon.Helpers
{
    public sealed class Shortcut
    {
        private bool isPressed;
        private ModifierKeys? modifier;
        private Key? key;

        public Shortcut(ModifierKeys modifier)
        {
            this.modifier = modifier;
            this.key = null;
        }

        public Shortcut(Key key)
        {
            this.modifier = null;
            this.key = key;
        }

        public Shortcut(ModifierKeys modifier, Key key)
        {
            this.modifier = modifier;
            this.key = key;
        }

        public bool Check()
        {
            bool isModifierDown =
                this.modifier == null
                    ? true
                    : (Keyboard.Modifiers & this.modifier) == this.modifier;
            bool isKeyDown = this.key == null ? true : Keyboard.IsKeyDown(this.key.Value);

            if (isModifierDown && isKeyDown)
            {
                if (this.isPressed)
                    return false;

                this.isPressed = true;
            }
            else
            {
                this.isPressed = false;
            }

            return this.isPressed;
        }
    }
}
