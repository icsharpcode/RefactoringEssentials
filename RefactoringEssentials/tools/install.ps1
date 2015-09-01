param($installPath, $toolsPath, $package, $project)

$analyzerPath = join-path (split-Path -path $toolsPath -parent) "analyzers\dotnet"
$analyzerFilePath = join-path $analyzerPath "RefactoringEssentials.dll"

$project.Object.AnalyzerReferences.Add("$analyzerFilePath")