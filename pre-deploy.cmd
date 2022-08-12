dotnet restore

dotnet build --configuration Debug
dotnet build --configuration Release

dotnet test -c Debug .\test\TauCode.Working.Tests\TauCode.Working.Tests.csproj
dotnet test -c Release .\test\TauCode.Working.Tests\TauCode.Working.Tests.csproj

nuget pack nuget\TauCode.Working.nuspec