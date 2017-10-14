using RefactoringEssentials.VB.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.VB.CodeRefactorings
{
    public class ReplaceDirectCastWithSafeCastTests : VBCodeRefactoringTestBase
    {
        void TestType(string type)
        {
            string input = @"
Imports System
Class TestClass
	Sub Test(ByVal a As Object)
		Dim b = $CType(a, " + type + @")
	End Sub
End Class";
            string output = @"
Imports System
Class TestClass
	Sub Test(ByVal a As Object)
		Dim b = TryCast(a, " + type + @")
	End Sub
End Class";
            Test<ReplaceDirectCastWithSafeCastCodeRefactoringProvider>(input, output);
        }

        [Fact]
        public void Test()
        {
            TestType("Exception");
        }

        [Fact]
        public void TestNullable()
        {
            TestType("Integer?");
        }

        [Fact]
        public void TestWithComment()
        {
            string input = @"
Imports System
Class TestClass
	Sub Test(ByVal a As Object)
		' Some comment
		Dim b = $CType(a, Exception)
	End Sub
End Class";
            string output = @"
Imports System
Class TestClass
	Sub Test(ByVal a As Object)
		' Some comment
		Dim b = TryCast(a, Exception)
	End Sub
End Class";

            Test<ReplaceDirectCastWithSafeCastCodeRefactoringProvider>(input, output);
        }

        [Fact]
        public void TestNonReferenceType()
        {
            TestWrongContext<ReplaceDirectCastWithSafeCastCodeRefactoringProvider>(@"
Imports System
Class TestClass
	Sub Test(ByVal a As Object)
		Dim b = $CType(a, Integer)
	End Sub
End Class");
        }
    }
}
