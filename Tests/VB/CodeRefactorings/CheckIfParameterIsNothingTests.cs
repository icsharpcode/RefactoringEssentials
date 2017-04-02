using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis;
using RefactoringEssentials.VB.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.VB.CodeRefactorings
{
    public class CheckIfParameterIsNothingTests : VBCodeRefactoringTestBase
    {
        // always need to check for System.ArgumentNullException in the generated code, because the Simplifier is not working
        // see https://github.com/dotnet/roslyn/issues/473
        [Fact]
        public void Test()
        {
            Test<CheckIfParameterIsNothingCodeRefactoringProvider>(@"
Imports System
Class TestClass
    Sub Test($param As String)
        Console.WriteLine(param)
    End Sub
End Class", @"
Imports System
Class TestClass
    Sub Test(param As String)
        If param Is Nothing Then
            Throw New System.ArgumentNullException(NameOf(param))
        End If

        Console.WriteLine(param)
    End Sub
End Class");
        }

        [Fact]
        public void TestWithComment()
        {
            Test<CheckIfParameterIsNothingCodeRefactoringProvider>(@"
Imports System
Class TestClass
    Sub Test($param As String)
        ' Some comment
        Console.WriteLine(param)
    End Sub
End Class", @"
Imports System
Class TestClass
    Sub Test(param As String)
        If param Is Nothing Then
            Throw New System.ArgumentNullException(NameOf(param))
        End If
        ' Some comment
        Console.WriteLine(param)
    End Sub
End Class");
        }

        [Fact]
        public void TestLambda()
        {
            Test<CheckIfParameterIsNothingCodeRefactoringProvider>(@"
Class TestClass
    Sub Test(param As String)
        Dim lambda = Sub ($sender, e)
                        
                     End Sub
    End Sub
End Class", @"
Class TestClass
    Sub Test(param As String)
        Dim lambda = Sub (sender, e)

                         If sender Is Nothing Then
                             Throw New System.ArgumentNullException(NameOf(sender))
                         End If
                     End Sub
    End Sub
End Class");
        }

        [Fact]
        public void TestNullCheckAlreadyThere_StringName()
        {
            TestWrongContext<CheckIfParameterIsNothingCodeRefactoringProvider>(@"
Class TestClass
    Sub Test(param As String)
        Dim lambda = Sub ($sender, e)
                        If sender Is Nothing Then
                            Throw New System.ArgumentNullException(""sender"")
                        End If
                     End Sub
    End Sub
End Class");
        }

        [Fact]
        public void TestNullCheckAlreadyThere_NameOf()
        {
            TestWrongContext<CheckIfParameterIsNothingCodeRefactoringProvider>(@"
Class TestClass
    Sub Test(param As String)
        Dim lambda = Sub ($sender, e)
                        If sender Is Nothing Then
                            Throw New System.ArgumentNullException(NameOf(sender))
                        End If
                     End Sub
    End Sub
End Class");
        }

        [Fact]
        public void TestNullCheckAlreadyThere_SingleLineIf()
        {
            TestWrongContext<CheckIfParameterIsNothingCodeRefactoringProvider>(@"
Class TestClass
    Sub Test(param As String)
        Dim lambda = Sub ($sender, e)
                        If sender Is Nothing Then Throw New System.ArgumentNullException(NameOf(sender))
                     End Sub
    End Sub
End Class");
        }

        [Fact]
        public void TestPopupOnlyOnName()
        {
            TestWrongContext<CheckIfParameterIsNothingCodeRefactoringProvider>(@"
Class Foo
	Sub Test(param As $String)
	End Sub
End Class");
        }


        [Fact]
        public void Test_OldVB()
        {
            var parseOptions = new VisualBasicParseOptions(
                LanguageVersion.VisualBasic12,
                DocumentationMode.Diagnose | DocumentationMode.Parse,
                SourceCodeKind.Regular
            );

            Test<CheckIfParameterIsNothingCodeRefactoringProvider>(@"
Class Foo
    Sub Test($test As String)
    End Sub
End Class", @"
Class Foo
    Sub Test(test As String)
        If test Is Nothing Then
            Throw New System.ArgumentNullException(""test"")
        End If
    End Sub
End Class", parseOptions: parseOptions);
        }
    }
}
