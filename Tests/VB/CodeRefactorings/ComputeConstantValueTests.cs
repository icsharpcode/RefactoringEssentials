using RefactoringEssentials.VB.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.VB.CodeRefactorings
{
    public class ComputeConstantValueTests : VBCodeRefactoringTestBase
    {
        [Fact]
        public void Rational1()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
Class TestClass
	Public Sub F()
		Dim a = 1 $+ 1
	End Sub
End Class", @"
Class TestClass
	Public Sub F()
		Dim a = 2
    End Sub
End Class");
        }

        [Fact]
        public void Rational1WithComment()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
Class TestClass
	Public Sub F()
        ' Some comment
		Dim a = 1 $+ 1
	End Sub
End Class", @"
Class TestClass
	Public Sub F()
        ' Some comment
		Dim a = 2
    End Sub
End Class");
        }

        [Fact]
        public void Rational2()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
Class TestClass
	Public Sub F()
		Dim a = 2 $* 2
	End Sub
End Class", @"
Class TestClass
	Public Sub F()
		Dim a = 4
    End Sub
End Class");
        }
        [Fact]
        public void Rational3()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
Class TestClass
	Public Sub F()
		Dim c = 0.2 $/ 2
	End Sub
End Class", @"
Class TestClass
	Public Sub F()
		Dim c = 0.1
    End Sub
End Class");
        }
        [Fact]
        public void Rational4()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
Class TestClass
	Public Sub F()
		Dim d = 2 $* (-0.2)
	End Sub
End Class", @"
Class TestClass
	Public Sub F()
		Dim d = -0.4
    End Sub
End Class");
        }
        [Fact]
        public void Rational5()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
Class TestClass
	Public Sub F()
		Dim e = 2 $* (1 << 2)
	End Sub
End Class", @"
Class TestClass
	Public Sub F()
		Dim e = 8
    End Sub
End Class");
        }
        [Fact]
        public void Rational6()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
Class TestClass
	Public Sub F()
		Dim f = 1 $+ (Not 4)
	End Sub
End Class", @"
Class TestClass
	Public Sub F()
		Dim f = -4
    End Sub
End Class");
        }

        [Fact]
        public void Bool1()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
Class TestClass
	Public Sub F()
		Dim a = $Not True
	End Sub
End Class", @"
Class TestClass
	Public Sub F()
		Dim a = False
    End Sub
End Class");
        }

        [Fact]
        public void Bool2()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
Class TestClass
	Public Sub F()
		Dim b = $Not Not (Not Not Not (True And False))
	End Sub
End Class", @"
Class TestClass
	Public Sub F()
		Dim b = True
    End Sub
End Class");
        }

        [Fact]
        public void Bool3()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
Class TestClass
	Public Sub F()
		Dim c = 1 $> 0
	End Sub
End Class", @"
Class TestClass
	Public Sub F()
		Dim c = True
    End Sub
End Class");
        }

        [Fact]
        public void String1()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
Class TestClass
	Public Sub F()
		Dim a = ""a"" $+ ""b""
    End Sub
End Class", @"
Class TestClass
	Public Sub F()
		Dim a = ""ab""
    End Sub
End Class");
        }

        [Fact]
        public void UseConstant()
        {
            Test<ComputeConstantValueCodeRefactoringProvider>(@"
Class TestClass
	Const pi As Double = 3.141
	Public Sub F()
		Dim pi2 = 2 $* pi
	End Sub
End Class", @"
Class TestClass
	Const pi As Double = 3.141
	Public Sub F()
		Dim pi2 = 6.282
    End Sub
End Class");
        }


        [Fact]
        public void Invalid()
        {
            TestWrongContext<ComputeConstantValueCodeRefactoringProvider>(@"
Class TestClass
	Public Sub F()
		Dim a = Not (True)
		Dim b = $Not Not(Not Not Not(True And a))
	End Sub
End Class");
        }


        [Fact]
        public void TestWrongHotSpot()
        {
            TestWrongContext<ComputeConstantValueCodeRefactoringProvider>(@"
Class TestClass
	Public Sub F()
		Dim a = 1 +$ 1
	End Sub
End Class");
        }
    }
}
