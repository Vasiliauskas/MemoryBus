$root = (split-path -parent $MyInvocation.MyCommand.Definition) + '\..'
$assemblyFile = $root + '\bin\Build\MemoryBus.dll'
Write-Host "$root"
Write-Host "$assemblyFile"
$version = [System.Reflection.Assembly]::LoadFile("$assemblyFile").GetName().Version
$versionStr = "{0}.{1}.{2}" -f ($version.Major, $version.Minor, $version.Build)

Write-Host "Setting .nuspec version tag to $versionStr"

$content = (Get-Content $root\nuget\MemoryBus.nuspec)
$content = $content -replace '\$version\$',$versionStr

$content | Out-File $root\nuget\MemoryBus.compiled.nuspec 
& $root\nuget\nuget.exe pack $root\nuget\MemoryBus.compiled.nuspec