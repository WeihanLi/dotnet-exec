[string]$SCRIPT = '.\build.cake'

# set environment variables
[System.Environment]::SetEnvironmentVariable('CI', 'true')

# Install the latest cake.tool
dotnet tool update --global cake.tool
# Install the lastest dotnet-execute tool
dotnet tool update --global dotnet-execute

# Execute CSharp script
Write-Host "dotnet-exec ./build.cs --args $ARGS" -ForegroundColor GREEN
dotnet-exec ./build.cs --args $ARGS

# Execute Cake
Write-Host "dotnet-cake $SCRIPT $ARGS" -ForegroundColor GREEN
dotnet cake $SCRIPT $ARGS