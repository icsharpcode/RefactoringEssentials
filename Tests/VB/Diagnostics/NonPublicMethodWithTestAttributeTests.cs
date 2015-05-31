using NUnit.Framework;
using RefactoringEssentials.VB.Diagnostics;

namespace RefactoringEssentials.Tests.VB.Diagnostics
{
    [TestFixture]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

