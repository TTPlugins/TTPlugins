# TTPlugins
# Copyright (C) 2024  TTPlugins
#
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU Affero General Public License as published
# by the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
# GNU Affero General Public License for more details.
#
# You should have received a copy of the GNU Affero General Public License
# along with this program.  If not, see <https://www.gnu.org/licenses/>.

# This script copies a DLL file to a target directory specified in a .env file.
# The .env file should be located in the solution directory.
# The .env file should contain lines in the format VARIABLE=VALUE.
#
# Example .env file:
# INDICATORS_TARGET_PATH=C:\Users\<username>\Documents\TigerTrade\Indicators\
# OBJECTS_TARGET_PATH=C:\Users\<username>\Documents\TigerTrade\Objects\

param (
    [string]$BasePath,
    [string]$SourcePath,
    [string]$TargetPathName
)


# Initialize TARGET_PATH to an empty string
$BASE_TARGET_PATH = Join-Path -Path ([Environment]::GetFolderPath("MyDocuments")) -ChildPath "TigerTrade"

$TARGET_PATH = $BASE_TARGET_PATH
if ("INDICATORS_TARGET_PATH" -ieq $TargetPathName) {
    $TARGET_PATH = Join-Path -Path $BASE_TARGET_PATH -ChildPath "Indicators"
} elseif ("OBJECTS_TARGET_PATH" -ieq $TargetPathName) {
    $TARGET_PATH = Join-Path -Path $BASE_TARGET_PATH -ChildPath "Objects"
}

# Check if the .env file exists
$envFilePath = Join-Path -Path $BasePath -ChildPath ".env"
if (Test-Path $envFilePath) {
    # Read each line from the .env file
    Get-Content $envFilePath | ForEach-Object {
        # Split the line into key-value pairs
        $line = $_ -split '=', 2
        if ($line.Length -eq 2) {
            $key = $line[0].Trim()
            $value = $line[1].Trim()

            # Check if the variable name matches the target path name
            if ($key -ieq $TargetPathName) {
                # Set TARGET_PATH to the value of the matching environment variable
                $TARGET_PATH = $value
            }
        }
    }
}

# Check if the target directory exists, and create it if it doesn't
if (-Not (Test-Path $TARGET_PATH)) {
    New-Item -ItemType Directory -Path $TARGET_PATH | Out-Null
}

# Copy the DLL file to the target directory
Copy-Item -Path $SourcePath -Destination $TARGET_PATH -Force -Recurse
