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

Write-Host
Write-Host
Write-Host
Write-Host -ForegroundColor Cyan "TTPlugins Installer"
Write-Host
Write-Host
Write-Host

# Wait for user confirmation
Write-Host "Press any key to install or update the plugins..."
$Host.UI.RawUI.ReadKey("NoEcho, IncludeKeyDown")

Write-Host
Write-Host

# Get the path to the `TigerTrade` folder in the user's documents
$BASE_TARGET_PATH = Join-Path -Path ([Environment]::GetFolderPath("MyDocuments")) -ChildPath "TigerTrade"

# Check if the `TigerTrade` folder exists
if (-Not (Test-Path $BASE_TARGET_PATH)) {
    Write-Host -ForegroundColor DarkGray "Creating missing folder: $BASE_TARGET_PATH"
    New-Item -ItemType Directory -Path $BASE_TARGET_PATH | Out-Null
}

# Get the path to the `Indicators` folder in the `TigerTrade` folder
$INDICATORS_TARGET_PATH = Join-Path -Path $BASE_TARGET_PATH -ChildPath "Indicators"

# Check if the `Indicators` folder exists
if (-Not (Test-Path $INDICATORS_TARGET_PATH)) {
    Write-Host -ForegroundColor DarkGray "Creating missing folder: $INDICATORS_TARGET_PATH"
    New-Item -ItemType Directory -Path $INDICATORS_TARGET_PATH | Out-Null
}

# Get the path to the `Objects` folder in the `TigerTrade` folder
$OBJECTS_TARGET_PATH = Join-Path -Path $BASE_TARGET_PATH -ChildPath "Objects"

# Check if the `Objects` folder exists
if (-Not (Test-Path $OBJECTS_TARGET_PATH)) {
    Write-Host -ForegroundColor DarkGray "Creating missing folder: $OBJECTS_TARGET_PATH"
    New-Item -ItemType Directory -Path $OBJECTS_TARGET_PATH | Out-Null
}

# Check for existing `Indicators++.dll` file
$INDICATORS_DLL_PATH = Join-Path -Path $INDICATORS_TARGET_PATH -ChildPath "Indicators++.dll"
if (Test-Path $INDICATORS_DLL_PATH) {
    Write-Host -ForegroundColor DarkGray "Removing existing Indicators++.dll: $INDICATORS_DLL_PATH"
    Remove-Item -Force -Path $INDICATORS_DLL_PATH
}

# Check for existing `Objects++.dll` file
$OBJECTS_DLL_PATH = Join-Path -Path $OBJECTS_TARGET_PATH -ChildPath "Objects++.dll"
if (Test-Path $OBJECTS_DLL_PATH) {
    Write-Host -ForegroundColor DarkGray "Removing existing Objects++.dll: $OBJECTS_DLL_PATH"
    Remove-Item -Force -Path $OBJECTS_DLL_PATH
}

Write-Host
Write-Host -ForegroundColor Cyan "Downloading and Installing Indicators++.dll"

# Download the Indicators++.dll file from the latest release
$INDICATORS_DLL_URI = "https://github.com/TTPlugins/TTPlugins/releases/latest/download/Indicators++.dll"
Invoke-WebRequest -Uri $INDICATORS_DLL_URI -OutFile $INDICATORS_DLL_PATH

Write-Host -ForegroundColor Green "Indicators++ installed successfully"

Write-Host
Write-Host -ForegroundColor Cyan "Downloading and Installing Objects++.dll"

# Download the Objects++.dll file from the latest release
$OBJECTS_DLL_URI = "https://github.com/TTPlugins/TTPlugins/releases/latest/download/Objects++.dll"
Invoke-WebRequest -Uri $OBJECTS_DLL_URI -OutFile $OBJECTS_DLL_PATH

Write-Host -ForegroundColor Green "Objects++ installed successfully"

Write-Host
Write-Host
Write-Host
Write-Host -ForegroundColor DarkGray "If you like the project consider sharing it with others or leaving a star:"
Write-Host -ForegroundColor DarkGray "https://github.com/TTPlugins/TTPlugins"
Write-Host
Write-Host -ForegroundColor Yellow "Please restart Tiger.com (aka TigerTrade) for the changes to take effect"
Write-Host
Write-Host
Write-Host

# Wait for user confirmation
Write-Host "Press any key to exit the installer..."
$Host.UI.RawUI.ReadKey("NoEcho, IncludeKeyDown")
