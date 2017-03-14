# Version settings
$version = New-Object System.Version("5.1.0.0")
$releaseNotesLink = 'https://github.com/icsharpcode/RefactoringEssentials/wiki/Release-5.2'
$copyright = 'Copyright (c) 2014-2017 AlphaSierraPapa and Xamarin Inc.'

# ========================================================================================

# AppVeyor configuration
(cat appveyor.2017.yml) | % { $_ -replace 'version: (\d+\.\d+).\{build\}', ('version: ' + $version.Major + '.' + $version.Minor + '.{build}') } | Out-File -Encoding "utf8" appveyor.2017.yml

# Version infos of RefactoringEssentials project
(cat RefactoringEssentials.2017/RefactoringEssentials.csproj) | % { $_ `
    -replace '<Copyright>.+</Copyright>', ('<Copyright>' + $copyright + '</Copyright>') `
    -replace '<AssemblyVersion>.+</AssemblyVersion>', ('<AssemblyVersion>' + $version.ToString() + '</AssemblyVersion>') `
    -replace '<FileVersion>.+</FileVersion>', ('<FileVersion>' + $version.ToString() + '</FileVersion>') `
    -replace '<Version>.+</Version>', ('<Version>{0}.{1}.{2}</Version>' -f $version.Major, $version.Minor, $version.Build) `
 } | Out-File -Encoding "utf8" RefactoringEssentials.2017/RefactoringEssentials.csproj

  # NuGet defintions of RefactoringEssentials project
(cat RefactoringEssentials.2017/RefactoringEssentials.nuspec) | % { $_ `
    -replace '<copyright>.+</copyright>', ('<copyright>' + $copyright + '</copyright>') `
    -replace '<version>.+</version>', ('<version>{0}.{1}.{2}</version>' -f $version.Major, $version.Minor, $version.Build) `
    -replace '<releaseNotes>Please see https://github.com/icsharpcode/RefactoringEssentials/wiki/Release-.+ for more information.</releaseNotes>', ('<releaseNotes>Please see https://github.com/icsharpcode/RefactoringEssentials/wiki/Release-{0}.{1} for more information.</releaseNotes>' -f $version.Major, $version.Minor) `
 } | Out-File -Encoding "utf8" RefactoringEssentials.2017/RefactoringEssentials.nuspec
 (cat RefactoringEssentials.2017/RefactoringEssentials.Library.nuspec) | % { $_ `
    -replace '<copyright>.+</copyright>', ('<copyright>' + $copyright + '</copyright>') `
    -replace '<version>.+</version>', ('<version>{0}.{1}.{2}</version>' -f $version.Major, $version.Minor, $version.Build) `
    -replace '<releaseNotes>Please see https://github.com/icsharpcode/RefactoringEssentials/wiki/Release-.+ for more information.</releaseNotes>', ('<releaseNotes>Please see https://github.com/icsharpcode/RefactoringEssentials/wiki/Release-{0}.{1} for more information.</releaseNotes>' -f $version.Major, $version.Minor) `
 } | Out-File -Encoding "utf8" RefactoringEssentials.2017/RefactoringEssentials.Library.nuspec

 # Version infos of Tests project
#(cat Tests.2017/Tests.csproj) | % { $_ `
#    -replace '<Copyright>.+</Copyright>', ('<Copyright>' + $copyright + '</Copyright>') `
#    -replace '<AssemblyVersion>.+</AssemblyVersion>', ('<AssemblyVersion>' + $version.ToString() + '</AssemblyVersion>') `
#    -replace '<FileVersion>.+</FileVersion>', ('<FileVersion>' + $version.ToString() + '</FileVersion>') `
#    -replace '<Version>.+</Version>', ('<Version>{0}.{1}.{2}</Version>' -f $version.Major, $version.Minor, $version.Build) `
# } | Out-File Tests.2017/Tests.csproj
(cat Tests.2017/Properties/AssemblyInfo.cs) | % { $_ `
    -replace '\[assembly: AssemblyCopyright\(".+"\)\]', ('[assembly: AssemblyCopyright("' + $copyright + '")]') `
    -replace '\[assembly: AssemblyVersion\(".+"\)\]', ('[assembly: AssemblyVersion("' + $version.ToString() + '")]') `
    -replace '\[assembly: AssemblyFileVersion\(".+"\)\]', ('[assembly: AssemblyFileVersion("' + $version.ToString() + '")]') `
 } | Out-File -Encoding "utf8" Tests.2017/Properties/AssemblyInfo.cs

# VSIX manifest
(cat Vsix.2017/source.extension.vsixmanifest) | % { $_ `
    -replace 'Version="\d+\.\d+\.\d+\.\d+"', ('Version="' + $version.ToString() + '"') `
    -replace '<GettingStartedGuide>.+</GettingStartedGuide>', ('<GettingStartedGuide>' + $releaseNotesLink + '</GettingStartedGuide>') `
    -replace '<ReleaseNotes>.+</ReleaseNotes>', ('<ReleaseNotes>' + $releaseNotesLink + '</ReleaseNotes>') `
 } | Out-File -Encoding "utf8" Vsix.2017/source.extension.vsixmanifest
