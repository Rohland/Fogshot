$greenshotPath = "C:\Program Files (x86)\Greenshot\"
$pluginPath = "Plugins\GreenshotFogbugzPlugin\"
$pluginAssemblyName = "GreenshotFogbugzPlugin.dll"
$pluginAssemblyNameGSP = "GreenshotFogbugzPlugin.gsp"
$languageFileName = "language_fogbugzplugin-en-US.xml"

$destinationAssemblyFolder = [system.io.path]::combine($greenshotPath, $pluginPath)
$destinationLanguageFolder = [system.io.path]::combine([system.io.path]::combine($greenshotPath, "Languages\"), $pluginPath)

$destinationAssemblyPath = [system.io.path]::combine($destinationAssemblyFolder, $pluginAssemblyNameGSP)
$destinationLanguagePath = [system.io.path]::combine($destinationLanguageFolder, $languageFileName)

# Create the directories required
if ([system.io.directory]::exists($destinationAssemblyFolder) -eq $false){
	[system.io.directory]::createdirectory($destinationAssemblyFolder)
}

if ([system.io.directory]::exists($destinationLanguageFolder) -eq $false){
	[system.io.directory]::createdirectory($destinationLanguageFolder)
}


[system.io.file]::copy((Resolve-Path $pluginAssemblyName).ToString(), $destinationAssemblyPath, $true)
[system.io.file]::copy((Resolve-Path $languageFileName).ToString(), $destinationLanguagePath, $true)