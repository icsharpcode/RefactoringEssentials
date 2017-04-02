using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace RefactoringEssentials.Tests
{
    public class SymbolChecksTest
    {
        [Fact]
        public void DirectSpecialType()
        {
            var symbol = FindSymbol<IParameterSymbol>(@"
using System;
class Test
{
    void Method(IDisposable a)
    {
        var x = $a;
    }
}");

            Assert.True(
                symbol.GetSymbolType().ImplementsSpecialTypeInterface(SpecialType.System_IDisposable),
                userMessage: "Symbol is implementing IDisposable"
            );
        }

        [Fact]
        public void SpecialTypeAsGenericConstraint()
        {
            var symbol = FindSymbol<IParameterSymbol>(@"
using System;
class Test
{
    void Method<T>(T a)
        where T : IDisposable
    {
        var x = $a;
    }
}");

			Assert.True(
				symbol.GetSymbolType().ImplementsSpecialTypeInterface(SpecialType.System_IDisposable),
                userMessage: "Symbol is implementing IDisposable"
            );
        }

        [Fact]
        public void InterfaceInheritingSpecialTypeAsGenericConstraint()
        {
            var symbol = FindSymbol<IParameterSymbol>(@"
using System;
interface IInheritDisposable : IDisposable
{

}
class Test
{
    void Method<T>(T a)
        where T : IInheritDisposable
    {
        var x = $a;
    }
}");

            Assert.True(
                symbol.GetSymbolType().ImplementsSpecialTypeInterface(SpecialType.System_IDisposable),
				userMessage: "Symbol is implementing IDisposable"
            );
        }

        [Fact]
        public void InterfaceInheritingSpecialTypeAsParameter()
        {
            var symbol = FindSymbol<IParameterSymbol>(@"
using System;
interface IInheritDisposable : IDisposable
{

}
class Test
{
    void Method(IInheritDisposable a)
    {
        var x = $a;
    }
}");
            Assert.True(
                symbol.GetSymbolType().ImplementsSpecialTypeInterface(SpecialType.System_IDisposable),
				userMessage: "Symbol is implementing IDisposable"
            );
        }

        [Fact]
        public void ClassInheritingImplementionOfSpecialTypeAsGenericConstraint()
        {
            var symbol = FindSymbol<IParameterSymbol>(@"
using System;
interface IInheritDisposable : IDisposable
{

}

class BaseClass : IInheritDisposable
{
    public void Dispose(){}
}

class Derived : BaseClass
{}

class Test
{
    void Method<T>(T a)
        where T:Derived
    {
        var x = $a;
    }
}");

            Assert.True(
                symbol.GetSymbolType().ImplementsSpecialTypeInterface(SpecialType.System_IDisposable),
				userMessage: "Symbol is implementing IDisposable"
            );
        }

        [Fact]
        public void ClassInheritingImplementionOfGenericSpecialTypeAsGenericConstraint()
        {
            var symbol = FindSymbol<IParameterSymbol>(@"
using System.Collections.Generic;
interface IInheritEnumerable : IEnumerable<object>
{

}

class BaseClass : IInheritEnumerable
{
    public IEnumerator<object> GetEnumerator()
    {
        return null;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return null;
    }
}

class Derived : BaseClass
{}

class Test
{
    void Method<T>(T a)
        where T:Derived
    {
        var x = $a;
    }
}");

            Assert.True(
                symbol.GetSymbolType().ImplementsSpecialTypeInterface(SpecialType.System_Collections_Generic_IEnumerable_T),
                userMessage: "Symbol is implementing IDisposable"
            );
        }

        [Fact]
        public void LocallyDefinedDisposableNotDetectedAsSpecialType()
        {
            var symbol = FindSymbol<IParameterSymbol>(@"
interface IDisposable
{}
class Test
{
    void Method(IDisposable a)
    {
        var x = $a;
    }
}");

            Assert.False(
                symbol.GetSymbolType().ImplementsSpecialTypeInterface(SpecialType.System_IDisposable),
                userMessage: "Symbol is not implementing IDisposable"
            );
        }

        [Fact]
        public void DoNotHangOnGenericTypes()
        {
            var symbol = FindSymbol<IParameterSymbol>(@"
class Test
{
    void Method(System.IComparable<System.String> a)
    {
        var x = $a;
    }
}");

            Assert.False(
                symbol.GetSymbolType().ImplementsSpecialTypeInterface(SpecialType.System_IDisposable),
                userMessage: "Symbol is not implementing IDisposable"
            );
        }

        static TSymbol FindSymbol<TSymbol>(string code)
            where TSymbol : ISymbol
        {
            var position = code.IndexOf('$');

            var cleanedCode = code.Remove(position, 1);

            var workspace = new DiagnosticTestBase.TestWorkspace();
            var projectId = ProjectId.CreateNewId();
            var documentId = DocumentId.CreateNewId(projectId);

            var docInfo = DocumentInfo.Create(documentId, "Document1.cs", loader: TextLoader.From(TextAndVersion.Create(SourceText.From(cleanedCode), VersionStamp.Create())));

            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

            var projectInfo = ProjectInfo.Create(
                projectId,
                VersionStamp.Create(),
                "Project",
                "Project",
                LanguageNames.CSharp,
                metadataReferences: new[] { mscorlib },
                documents: new[] { docInfo }
                );


            workspace.Open(projectInfo);

            var document = workspace.CurrentSolution.GetDocument(documentId);

			var semanticModel = document.GetSemanticModelAsync().GetAwaiter().GetResult();

            var diag = semanticModel.GetDiagnostics();

            Assert.True(diag.IsEmpty, "No errors reported");

            var symbol = SymbolFinder.FindSymbolAtPositionAsync(semanticModel, position, workspace).GetAwaiter().GetResult();

            Assert.True(symbol != null, "Symbol should be found");
            Assert.True(symbol is TSymbol);

            return (TSymbol)symbol;
        }
    }
}
