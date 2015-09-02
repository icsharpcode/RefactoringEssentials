using System;
using NUnit.Framework;
using RefactoringEssentials.VB.CodeRefactorings;

namespace RefactoringEssentials.Tests.VB.CodeRefactorings
{
    [TestFixture]
    public class ConvertHexToDecTests : VBCodeRefactoringTestBase
    {
        [Test()]
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
