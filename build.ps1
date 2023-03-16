[string]$SCRIPT = '.\build.cake'

# set environment variables
[System.Environment]::SetEnvironmentVariable('CI', 'true')

# Install the latest cake.tool
dotnet tool install --global cake.tool

# Start Cake
Write-Host "dotnet cake $SCRIPT $ARGS" -ForegroundColor GREEN

# Execute Cake
dotnet cake $SCRIPT $ARGS