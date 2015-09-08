using NUnit.Framework;
using RefactoringEssentials.VB.CodeRefactorings;

namespace RefactoringEssentials.Tests.VB.CodeRefactorings
{
    [TestFixture]
    public class ReplaceSafeCastWithDirectCastTests : VBCodeRefactoringTestBase
    {
        [Test]
        public void Test()
        {
            Test<ReplaceSafeCastWithDirectCastCodeRefactoringProvider>(@"
Class TestClass
	Private Sub Test(a As Object)
		Dim b = $TryCast(a, Exception)
	End Sub
End Class", @"
Class TestClass
	Private Sub Test(a As Object)
		Dim b = DirectCast(a, Exception)
	End Sub
End Class");
        }

        [Test]
        public void TestWithComment1()
        {
            Test<ReplaceSafeCastWithDirectCastCodeRefactoringProvider>(@"
Class TestClass
	Private Sub Test(a As Object)
		' Some comment
		Dim b = $TryCast(a, Exception)
	End Sub
End Class", @"
Class TestClass
	Private Sub Test(a As Object)
		' Some comment
		Dim b = DirectCast(a, Exception)
	End Sub
End Class");
        }

        [Test]
        public void TestWithComment2()
        {
            Test<ReplaceSafeCastWithDirectCastCodeRefactoringProvider>(@"
Class TestClass
	Private Sub Test(a As Object)
		Dim b = $TryCast(a, Exception) ' Some comment
	End Sub
End Class", @"
Class TestClass
	Private Sub Test(a As Object)
		Dim b = DirectCast(a, Exception) ' Some comment
	End Sub
End Class");
        }
    }
}
