Param
(
    [String]$CakeVersion = "0.30.0",
    [String]$ToolsDir = "$PSScriptRoot/tools",
    [String]$ToolsProj = "$ToolsDir/build.csproj",
    [String]$Script = "$PSScriptRoot/build.cake",
    [String]$Target = 'Default',
    [String]$Verbosity = 'normal'
)

$CAKE_DIR = "$ToolsDir/Cake.CoreCLR.$CakeVersion"
$CAKE_DLL = "$CAKE_DIR/cake.coreclr/$CakeVersion/Cake.dll"

If (!(Test-Path $ToolsDir))
{
    New-Item -ItemType Directory -Force -Path $ToolsDir
}

if (!(Test-Path $ToolsProj))
{
    $projectFileContents = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>netcoreapp2.1</TargetFramework></PropertyGroup></Project>'
    Out-File -InputObject $projectFileContents -FilePath $ToolsProj
}

dotnet add "$ToolsProj" package cake.coreclr -v "$CakeVersion" --package-directory "$CAKE_DIR"
 
if (!(Test-Path $CAKE_DLL))
{
    Write-Error "Could not find Cake assembly '$CAKE_DLL'"
}
else
{
    dotnet "$CAKE_DLL" "$Script" --target="$Target" --verbosity="$Verbosity"
}

exit $LASTEXITCODE