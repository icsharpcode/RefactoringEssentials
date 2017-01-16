# Refactoring Essentials for Visual Studio

[![Join the chat at https://gitter.im/icsharpcode/RefactoringEssentials](https://badges.gitter.im/icsharpcode/RefactoringEssentials.svg)](https://gitter.im/icsharpcode/RefactoringEssentials?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

[@vsrefactoring](https://twitter.com/vsrefactoring) | [vsrefactoringessentials.com](http://vsrefactoringessentials.com/)

Refactorings Essentials sports the following features:

* Refactorings for C# and Visual Basic
* Analyzers and code fixes for C# and Visual Basic
* Code Converters to convert C# to Visual Basic and vice versa (VB to C#)

Refactoring Essentials comes in the following flavors:

* VSIX: built for Visual Studio - supports analyzers, refactorings and code converters (in VS 2015 and 2017)
* Nuget: packaged for build servers / IDEs that support analyzers via Nuget packages
* RefactoringEssentials assembly: usable in any IDE (-like environment), base assembly with all features

The latter means it is x-platform and not tied to Windows / Visual Studio.

History: Refactoring Essentials started out as NR6Pack, and was part of the NRefactory 6 repository. In the course of  
joint development, we made architectural tweaks that resulted in the two projects now being separate - which
makes them easier to maintain and contribute to.

# Project Build Status

Branch | Status 4.* (VS 2015) | Status 5.* (VS 2017)
--- | --- | ---
*master* (Development) | [![Build status](https://ci.appveyor.com/api/projects/status/5463mskexnsa0176/branch/master?svg=true)](https://ci.appveyor.com/project/icsharpcode/refactoringessentials/branch/master) | [![Build status](https://ci.appveyor.com/api/projects/status/r01wr5xdclj24y20/branch/master?svg=true)](https://ci.appveyor.com/project/icsharpcode/refactoringessentials-wgnsw/branch/master) 
*release* (Latest Release) | [![Build status](https://ci.appveyor.com/api/projects/status/5463mskexnsa0176/branch/release?svg=true)](https://ci.appveyor.com/project/icsharpcode/refactoringessentials/branch/release) | [![Build status](https://ci.appveyor.com/api/projects/status/r01wr5xdclj24y20/branch/release?svg=true)](https://ci.appveyor.com/project/icsharpcode/refactoringessentials-wgnsw/branch/release)