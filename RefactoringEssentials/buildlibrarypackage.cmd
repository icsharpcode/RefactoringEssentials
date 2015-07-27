@echo off

..\..\.nuget\NuGet.exe pack RefactoringEssentials.Library.nuspec -NoPackageAnalysis -BasePath . -OutputDirectory .
