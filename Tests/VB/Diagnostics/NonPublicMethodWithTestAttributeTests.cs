using RefactoringEssentials.VB.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.VB.Diagnostics
{
	public class NonPublicMethodWithTestAttributeTests : VBDiagnosticTestBase
    {
        const string NUnitClasses = @"Imports System
Imports NUnit.Framework

Namespace NUnit.Framework
	Public Class TestFixtureAttribute
        Inherits System.Attribute
    End Class
	Public Class TestAttribute
        Inherits System.Attribute
    End Class
End Namespace";

        [Fact]
        public void TestImplicitPrivate()
        {
            Analyze<NonPublicMethodWithTestAttributeAnalyzer>(NUnitClasses +
                @"
<TestFixture>
Class Tests 
	<Test>
	Sub $NonPublicMethod$()
	End Sub
End Class", NUnitClasses +
                @"
<TestFixture>
Class Tests 
	<Test>
	Public Sub NonPublicMethod()
	End Sub
End Class");
        }

        [Fact]
        public void TestExplicitPrivate()
        {
            Analyze<NonPublicMethodWithTestAttributeAnalyzer>(NUnitClasses +
                @"
<TestFixture>
Class Tests 
	<Test>
	Private Sub $NonPublicMethod$()
	End Sub
End Class", NUnitClasses +
                @"
<TestFixture>
Class Tests 
	<Test>
	Public Sub NonPublicMethod()
	End Sub
End Class");
        }

        [Fact]
        public void TestExplicitProtected()
        {
            Analyze<NonPublicMethodWithTestAttributeAnalyzer>(NUnitClasses +
                @"
<TestFixture>
Class Tests 
	<Test>
	Protected Sub $NonPublicMethod$()
	End Sub
End Class", NUnitClasses +
                @"
<TestFixture>
Class Tests 
	<Test>
	Public Sub NonPublicMethod()
	End Sub
End Class");
        }

        [Fact]
        public void TestExplicitInternal()
        {
            Analyze<NonPublicMethodWithTestAttributeAnalyzer>(NUnitClasses +
                @"
<TestFixture>
Class Tests 
	<Test>
	Friend Sub $NonPublicMethod$()
	End Sub
End Class", NUnitClasses +
                @"
<TestFixture>
Class Tests 
	<Test>
	Public Sub NonPublicMethod()
	End Sub
End Class");
        }

        [Fact]
        public void TestDisable()
        {
            Analyze<NonPublicMethodWithTestAttributeAnalyzer>(NUnitClasses + @"
<TestFixture>
Class Tests 
#Disable Warning " + VBDiagnosticIDs.NonPublicMethodWithTestAttributeAnalyzerID + @"
	<Test>
	Friend Sub NonPublicMethod()
	End Sub
End Class
");
        }
    }
}

