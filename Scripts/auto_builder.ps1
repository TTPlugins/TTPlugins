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

# Initialize variables
$PROGRAM_PATH = "C:\Program Files (x86)\TigerTrade\TigerTrade.exe"
$PROGRAM_NAME = "TigerTrade"

# Check if the .env file exists
$envFilePath = Join-Path -Path $pwd -ChildPath ".env"
if (Test-Path $envFilePath) {
    # Read each line from the .env file
    Get-Content $envFilePath | ForEach-Object {
        # Split the line into key-value pairs
        $line = $_ -split '=', 2
        if ($line.Length -eq 2) {
            $key = $line[0].Trim()
            $value = $line[1].Trim()

            if ($key -ieq "PROGRAM_PATH") {
                $PROGRAM_PATH = $value
            } elseif ($key -ieq "PROGRAM_NAME") {
                $PROGRAM_NAME = $value
            }
        }
    }
}

# Terminate the specified program if it is running
Stop-Process -Name $PROGRAM_NAME -Force

while ($true) {
    clear

    & ".\Scripts\build.ps1"

    # Check if the last command was successful
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed. Press Enter to continue."
        Read-Host
        continue
    }

    # Start the specified program and wait for it to exit
    Start-Process -FilePath $PROGRAM_PATH -Wait
}
