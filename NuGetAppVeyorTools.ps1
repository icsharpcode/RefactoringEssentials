[cmdletbinding()]
param()

function NuGet-GenerateAndPushArtifacts {
    [cmdletbinding()]
    param()
    process {
		$nuPkgPath = "./bin/*.nupkg"
		
		./bin/Release/buildpackage.cmd
		./bin/Release/buildlibrarypackage.cmd
        foreach($filePath in $nuPkgPath) {
            $fileName = (Get-ChildItem $filePath -Recurse)[0]
			Push-AppveyorArtifact ($fileName.FullName) -FileName $fileName.Name -DeploymentName "Latest release"
        }
    }
}