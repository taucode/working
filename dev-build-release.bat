dotnet restore

dotnet build --configuration Debug
dotnet build --configuration Release

dotnet test -c Debug .\tests\TauCode.Working.Tests\TauCode.Working.Tests.csproj
dotnet test -c Release .\tests\TauCode.Working.Tests\TauCode.Working.Tests.csproj

nuget pack nuget\TauCode.Working.nuspec