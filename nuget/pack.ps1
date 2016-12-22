$root = (split-path -parent $MyInvocation.MyCommand.Definition) + '\..\..'
$version = [System.Reflection.Assembly]::LoadFile("$root\MemoryBus\bin\Build\MemoryBus.dll").GetName().Version
$versionStr = "{0}.{1}.{2}" -f ($version.Major, $version.Minor, $version.Build)

Write-Host "Setting .nuspec version tag to $versionStr"

$content = (Get-Content $root\nuget\MemoryBus.nuspec)
$content = $content -replace '\$version\$',$versionStr

$content | Out-File $root\nuget\MemoryBus.compiled.nuspec 
& $root\nuget\nuget.exe pack $root\nuget\MemoryBus.compiled.nuspec