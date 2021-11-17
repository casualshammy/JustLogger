Set-Location $PSScriptRoot
dotnet restore
dotnet build -c Release -o _package
$tag = git describe --tags --abbrev=0
dotnet pack -c Release /p:Version=$tag -o _package
dotnet nuget push _package/*.nupkg --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json