param (
    [Parameter(Mandatory = $false)][String]$Configuration = "Debug"
)

$SolutionDir = Join-Path -Path $PSCommandPath -ChildPath ../.. -Resolve
Write-Host "Solution directory: $SolutionDir"

[XML]$XmlVersionFile = Get-Content "$SolutionDir/Version.props"
$VersionPrefix = $XmlVersionFile.Project.PropertyGroup.VersionPrefix
$VersionSuffix = "local-test" # $XmlVersionFile.Project.PropertyGroup.VersionSuffix

$PatchVersion = & git rev-list --count master
$VersionPrefix = "$VersionPrefix.$PatchVersion"

$PackageVersion = $VersionPrefix 
if ($VersionSuffix -ne "") {
    $PackageVersion = "$VersionPrefix-$VersionSuffix"
}

Write-Output "Package version: $PackageVersion"

Write-Output "Building"
& dotnet build --configuration $Configuration /p:VersionPrefix=$VersionPrefix /p:VersionSuffix=$VersionSuffix $SolutionDir

Write-Output "Packing"
& dotnet pack --configuration $Configuration /p:VersionPrefix=$VersionPrefix /p:VersionSuffix=$VersionSuffix $SolutionDir

Write-Output "Installing"
& dotnet tool uninstall Basalt.UniversalFileSystem.Cli -g
& dotnet tool install Basalt.UniversalFileSystem.Cli -g --add-source "$SolutionDir\apps\Basalt.UniversalFileSystem.Cli\bin\$Configuration" --version $PackageVersion
