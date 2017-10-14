using RefactoringEssentials.VB.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.VB.CodeRefactorings
{
    public class ConvertHexToDecTests : VBCodeRefactoringTestBase
    {
        [Fact]
        public void Test()
        {
            Test<ConvertHexToDecCodeRefactoringProvider>(@"
Class TestClass
	Private Sub Test()
		Dim i As Integer = $&H10
	End Sub
End Class", @"
Class TestClass
	Private Sub Test()
		Dim i As Integer = 16
	End Sub
End Class");
        }
    }
}
