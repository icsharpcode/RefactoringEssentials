using RefactoringEssentials.VB.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.VB.CodeRefactorings
{
    public class CreateOverloadWithoutParameterTests : VBCodeRefactoringTestBase
    {
        [Fact]
        public void Test()
        {
            Test<CreateOverloadWithoutParameterCodeRefactoringProvider>(@"
Public Class Test
    Sub TestMethod ($i As Integer)
    End Sub
End Class", @"
Public Class Test
    Sub TestMethod()
        TestMethod(0)
    End Sub
    Sub TestMethod (i As Integer)
    End Sub
End Class");
        }

        [Fact]
        public void TestWithReturnValue()
        {
            Test<CreateOverloadWithoutParameterCodeRefactoringProvider>(@"
Public Class Test
    Function TestMethod ($i As Integer) As Integer
        Return -1
    End Function
End Class", @"
Public Class Test
    Function TestMethod() As Integer
        Return TestMethod(0)
    End Function
    Function TestMethod (i As Integer) As Integer
        Return -1
    End Function
End Class");
        }

        [Fact]
        public void TestWithXmlDoc()
        {
            Test<CreateOverloadWithoutParameterCodeRefactoringProvider>(@"
Public Class Test
    ''' <summary>
    ''' Some description
    ''' </summary>
    Sub TestMethod ($i As Integer)
    End Sub
End Class", @"
Public Class Test
    Sub TestMethod()
        TestMethod(0)
    End Sub
    ''' <summary>
    ''' Some description
    ''' </summary>
    Sub TestMethod (i As Integer)
    End Sub
End Class");
        }

        [Fact]
        public void TestByRefParameter()
        {
            Test<CreateOverloadWithoutParameterCodeRefactoringProvider>(@"
Public Class Test
    Sub TestMethod (ByRef $i As Integer)
    End Sub
End Class", @"
Public Class Test
    Sub TestMethod()
        Dim i As Integer = 0
        TestMethod(i)
    End Sub
    Sub TestMethod (ByRef i As Integer)
    End Sub
End Class");
        }

        [Fact]
        public void TestDefaultValue()
        {
            TestDefaultValue("Object", null, "Nothing");
            TestDefaultValue("Integer?", null, "Nothing");
            TestDefaultValue("System.Nullable(Of Integer)", null, "Nothing");
            TestDefaultValue("System.Collections.Generic.IEnumerable(Of Integer)", null, "Nothing");
            TestDefaultValue("System.Collections.Generic.IEnumerable", "T", "CType(Nothing, System.Collections.Generic.IEnumerable(Of T))");
            TestDefaultValue("Boolean", null, "False");
            TestDefaultValue("Double", null, "0");
            TestDefaultValue("Char", null, "vbNullChar");
            TestDefaultValue("System.DateTime", null, "New System.DateTime");

            Test<CreateOverloadWithoutParameterCodeRefactoringProvider>(@"
Public Class Test
    Sub TestMethod(Of T)($i As T)
    End Sub
End Class", @"
Public Class Test
    Sub TestMethod(Of T)()
        TestMethod(CType(Nothing, T))
    End Sub
    Sub TestMethod(Of T)(i As T)
    End Sub
End Class");
        }

        void TestDefaultValue(string type, string typeParameter, string expectedValue)
        {
            string generic = string.IsNullOrEmpty(typeParameter) ? "" : ("(Of " + typeParameter + ")");

            Test<CreateOverloadWithoutParameterCodeRefactoringProvider>(@"
Public Class Test
    Sub TestMethod" + generic + " ($i As " + type + generic + @")
    End Sub
End Class", @"
Public Class Test
    Sub TestMethod" + generic + @"()
        TestMethod(" + expectedValue + @")
    End Sub
    Sub TestMethod" + generic + " (i As " + type + generic + @")
    End Sub
End Class");
        }

        [Fact]
        public void TestOptionalParameter()
        {
            TestWrongContext<CreateOverloadWithoutParameterCodeRefactoringProvider>(@"
Public Class Test
    Sub TestMethod($i As Integer = 0)
    End Sub
End Class");
        }

        [Fact]
        public void TestExistingMethod()
        {
            TestWrongContext<CreateOverloadWithoutParameterCodeRefactoringProvider>(@"
Public Class Test
    Sub TestMethod(c As Integer)
    End Sub
    Sub TestMethod(a As Integer, $b As Integer)
    End Sub
End Class");
            TestWrongContext<CreateOverloadWithoutParameterCodeRefactoringProvider>(@"
Public Class Test
    Sub TestMethod(Of T)(c As T)
    End Sub
    Sub TestMethod(Of T)(a As T, $b As T)
    End Sub
End Class");
        }

        [Fact]
        public void TestInterface()
        {
            Test<CreateOverloadWithoutParameterCodeRefactoringProvider>(@"
Public Interface ITest
    Sub Test ($a As Integer, b As Integer)
End Interface", @"
Public Interface ITest
    Sub Test(b As Integer)
    Sub Test (a As Integer, b As Integer)
End Interface");
        }

        [Fact]
        public void TestExplicitImpl()
        {
            TestWrongContext<CreateOverloadWithoutParameterCodeRefactoringProvider>(@"
Public Interface ITest
    Sub Test (a As Integer, b As Integer)
End Interface
Public Class Test
    Implements ITest
    Sub ITest.Test (a As Integer, $b As Integer)
    End Sub
End Class");
        }

        [Fact]
        public void TestGenereatedCall()
        {
            Test<CreateOverloadWithoutParameterCodeRefactoringProvider>(@"
Public Class Test
    Sub TestMethod (ByRef $i As Integer, ByRef j As Integer)
    End Sub
End Class", @"
Public Class Test
    Sub TestMethod(ByRef j As Integer)
        Dim i As Integer = 0
        TestMethod(i, j)
    End Sub
    Sub TestMethod (ByRef i As Integer, ByRef j As Integer)
    End Sub
End Class");
        }
    }
}
