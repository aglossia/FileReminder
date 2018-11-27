$release = "FileReminder.exe"

$version = (Get-ItemProperty $release).VersionInfo.FileVersion

$tardir = "E:\work\tl\FileReminder"

$mkdir = $tardir + "\ver" + $version

$module = $mkdir + "\FileReminder.exe"

if(!(Test-Path $mkdir)){
	New-Item $mkdir -itemType Directory
}else{
	if(Test-Path $module){
		Remove-Item $module
	}
}

Copy-Item $release $mkdir

Copy-Item $release $tardir