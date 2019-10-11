# Refactoring Essentials for Visual Studio

Development of the Visual Studio extension and NuGet package has wound down.

Refactorings Essentials sports the following features:

* Refactorings for C# and Visual Basic
* Analyzers and code fixes for C# and Visual Basic
* Code Converters to convert C# to Visual Basic and vice versa (VB to C#) - now available separately via [https://github.com/icsharpcode/CodeConverter/](https://github.com/icsharpcode/CodeConverter/)

Refactoring Essentials comes in the following flavors:

* VSIX: built for Visual Studio - supports analyzers and refactorings
* Nuget: packaged for build servers / IDEs that support analyzers via Nuget packages
* RefactoringEssentials assembly: usable in any IDE (-like environment), base assembly with all features

The latter means it is x-platform and not tied to Windows / Visual Studio.

History: Refactoring Essentials started out as NR6Pack, and was part of the NRefactory 6 repository. In the course of  
joint development, we made architectural tweaks that resulted in the two projects now being separate - which
makes them easier to maintain and contribute to.
