Set-Location $PSScriptRoot
$tag = git describe --tags --abbrev=0
dotnet pack -c Release /p:Version=$tag -o _package
dotnet nuget push _package\*.nupkg --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json
