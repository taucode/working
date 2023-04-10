dotnet restore

dotnet build TauCode.Working.sln -c Debug
dotnet build TauCode.Working.sln -c Release

dotnet test TauCode.Working.sln -c Debug
dotnet test TauCode.Working.sln -c Release

nuget pack nuget\TauCode.Working.nuspec