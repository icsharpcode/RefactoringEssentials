# Version settings
$version = New-Object System.Version("4.9.0.0")
$releaseNotesLink = 'https://github.com/icsharpcode/RefactoringEssentials/wiki/Release-4.8'
$copyright = 'Copyright (c) 2014-2017 AlphaSierraPapa and Xamarin Inc.'

# ========================================================================================

# AppVeyor configuration
(cat appveyor.yml) | % { $_ -replace 'version: (\d+\.\d+).\{build\}', ('version: ' + $version.Major + '.' + $version.Minor + '.{build}') } | Out-File appveyor.yml

# AssemblyInfoBase of RefactoringEssentials project
(cat RefactoringEssentials/Properties/AssemblyInfoBase.cs) | % { $_ `
    -replace '\[assembly: AssemblyCopyright\(".+"\)\]', ('[assembly: AssemblyCopyright("' + $copyright + '")]') `
    -replace '\[assembly: AssemblyVersion\(".+"\)\]', ('[assembly: AssemblyVersion("' + $version.ToString() + '")]') `
    -replace '\[assembly: AssemblyFileVersion\(".+"\)\]', ('[assembly: AssemblyFileVersion("' + $version.ToString() + '")]') `
 } | Out-File RefactoringEssentials/Properties/AssemblyInfoBase.cs

 # NuGet defintions of RefactoringEssentials project
(cat RefactoringEssentials/RefactoringEssentials.nuspec) | % { $_ `
    -replace '<copyright>.+</copyright>', ('<copyright>' + $copyright + '</copyright>') `
    -replace '<version>.+</version>', ('<version>{0}.{1}.{2}</version>' -f $version.Major, $version.Minor, $version.Build) `
    -replace 'https://github.com/icsharpcode/RefactoringEssentials/wiki/Release-.+', ('https://github.com/icsharpcode/RefactoringEssentials/wiki/Release-' + '{0}.{1}' -f $version.Major, $version.Minor) `
 } | Out-File RefactoringEssentials/RefactoringEssentials.nuspec
 (cat RefactoringEssentials/RefactoringEssentials.Library.nuspec) | % { $_ `
    -replace '<copyright>.+</copyright>', ('<copyright>' + $copyright + '</copyright>') `
    -replace '<version>.+</version>', ('<version>{0}.{1}.{2}</version>' -f $version.Major, $version.Minor, $version.Build) `
    -replace 'https://github.com/icsharpcode/RefactoringEssentials/wiki/Release-.+', ('https://github.com/icsharpcode/RefactoringEssentials/wiki/Release-' + '{0}.{1}' -f $version.Major, $version.Minor) `
 } | Out-File RefactoringEssentials/RefactoringEssentials.Library.nuspec

# VSIX manifest
(cat Vsix/source.extension.vsixmanifest) | % { $_ `
    -replace 'Version="\d+\.\d+\.\d+\.\d+"', ('Version="' + $version.ToString() + '"') `
    -replace '<GettingStartedGuide>.+</GettingStartedGuide>', ('<GettingStartedGuide>' + $releaseNotesLink + '</GettingStartedGuide>') `
    -replace '<ReleaseNotes>.+</ReleaseNotes>', ('<ReleaseNotes>' + $releaseNotesLink + '</ReleaseNotes>') `
 } | Out-File Vsix/source.extension.vsixmanifest
