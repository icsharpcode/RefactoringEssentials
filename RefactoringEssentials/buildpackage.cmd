@echo off

..\..\.nuget\NuGet.exe pack RefactoringEssentials.nuspec -NoPackageAnalysis -BasePath . -OutputDirectory .
