using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host.Mef;
using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings.Uncategorized;
using RefactoringEssentials.Tests.Common;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    /// <summary>
    /// Tests the <see cref="InlineMethodAction"/> <see cref="Microsoft.CodeAnalysis.CodeRefactorings.CodeRefactoringProvider"/>.
    /// </summary>
    [TestFixture]
    public class InlineMethodActionTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        [Description("Do not suggest refactoring unless the method signature is marked (excluding arguments).")]
        public void DoesNotRefactorWhenNotOnMethodSignature()
        {
            // Ensure the inline method action is only invoked when the method's signature is marked.

            TestWrongContext<InlineMethodAction>(
@"
class Animal
{
    public void Foo() {
        var x = Bar(5, 1);
    }

    public int Bar(int a, int b) {
        //// Simple math.
        return $(a + b);
    }

}");

            TestWrongContext<InlineMethodAction>(
@"
class Animal
{
    public void Foo() {
        var x = Bar(5, 1);
    }

    public int Bar(int a, int b) {
        //// Simple math.
        $return (a + b);
    }

}");

            TestWrongContext<InlineMethodAction>(
@"
class Animal
{
    public void Foo() {
        var x = Bar(5, 1);
    }

    public int Bar(int a, int $b) {
        //// Simple math.
        return (a + b);
    }

}");
        }

        [Test]
        [Description("Do not inline a method with errors, that would just make more errors.")]
        public void DoesNotRefactorMethodWithErrors()
        {
            // Do no inline a method with errors, that would only propogate the errors.

            TestWrongContext<InlineMethodAction>(
@"
class Animal
{
    public void Foo() {
        var x = Bar(5, 1);
    }

    public int $Bar(int a, int b) {
        //// Simple math.
        return (a + b) This is invalid syntax!;
    }

}");
        }

        [Test]
        [Description("Do not inline an external public method. It may be referenced externally and refactoring may be breaking.")]
        public void DoesNotRefactorPublicExternalMethods()
        {
            // Class Animal is public as is Animal.Bar. Therefore Animal.Bar is an external reference.
            // External references may be in use by software outside the scope of the current workspace.
            // Removing an external reference could break unknown consumers.

            TestWrongContext<InlineMethodAction>(
@"
public class Animal
{
    public void Foo() {
        var x = Bar(5, 1);
    }

    public int $Bar(int a, int b) {
        //// Simple math.
        return (a + b);
    }

}");
        }

        [Test]
        [Description("Do not inline a method without any references, that is not our interest.")]
        public void DoesNotRefactorUnreferencedMethods()
        {
            // Animal.Bar is not referenced by any code and that is not the concern of the inline method action.

            TestWrongContext<InlineMethodAction>(
@"
class Animal
{

    // Method without any references.
    public int $Bar(int a, int b) {
        //// Simple math.
        return (a + b);
    }
}");
        }

        [Test]
        [Description("Do not inline a method with more than 1 line of code. This test may be invalid as the capability of method inlining is expanded.")]
        public void DoesNotRefactorComplexMethod()
        {
            // Animal.Bar is considered complex because it has more than one line of code.
            // At this time we are not refactoring complex methods.

            TestWrongContext<InlineMethodAction>(
@"
class Animal
{
    public void Foo() {
        var x = Bar(1, 2);
    }

    // A complex method, one with multiple lines of code.
    public int $Bar(int a, int b) {
        //// Simple math.
        var result = (a + b);
        return result;
    }
}");
        }

        [Test]
        [Description("Do not inline a method when contains internal member access.")]
        public void DoNotInlineMethodBecauseOfInternalMemberAccess()
        {
            // Do not inline method Animal.AddLeg because its body references internal member Animal.leg.
            // AddLeg references internal member legs.
            // Class Dog cannot refer to Animal.legs.

            TestWrongContext<InlineMethodAction>(
@"class Dog : Animal {
    
    public void DoLegs() {
        AddLeg();
        AddLeg();
        AddLeg();
        AddLeg();
    }
}

class Animal
{
    private int legs = 0;
    
    protected void $AddLeg() {
        //// Simple math.
        legs++;
    }

}");
        }

        [Test]
        [Description("Inline a method with return type. int Bar()")]
        public void InlineReturnMethod()
        {
            Test<InlineMethodAction>(
@"class TestClass
{
    public void Foo() {
        var x = Bar(5, 1);
    }

    public int $Bar(int a, int b) {
        //// Simple math.
        return (a + b);
    }

}",
@"class TestClass
{
    public void Foo() {
        var x = (5 + 1);
    }

}");
        }

        [Test]
        [Description("Inline a method without return type. void Bar()")]
        public void InlineVoidMethod()
        {
            Test<InlineMethodAction>(
@"class TestClass
{
    public void Foo() {
        Bar(5, 1);
    }

    public void $Bar(int a, int b) {
        //// Simple math.
        Console.Writeline(a + b);
    }

}",
@"class TestClass
{
    public void Foo() {
        Console.Writeline(5 + 1);
    }

}");
        }

        [Test]
        [Description("Inline a method and replace a member access invocation. foo.Bar(1, 2)")]
        public void InlineMethodReplaceMemberAccess()
        {
            Test<InlineMethodAction>(
@"class TestClass
{
    public void Foo() {
        var tc = new TestClass();
        tc.Bar(5, 1);
    }

    public void $Bar(int a, int b) {
        //// Simple math.
        Console.Writeline(a + b);
    }

}",
@"class TestClass
{
    public void Foo() {
        var tc = new TestClass();
        Console.Writeline(5 + 1);
    }

}");
        }

        [Test]
        [Description("Inline a method and replace a delegate reference.")]
        public void InlineMethodReplaceDelegate()
        {
            // Method Foo invokes Foo3 with a delegate Bar.
            // The delegate Bar will be replaced with inline method in the form of lambda syntax.

            Test<InlineMethodAction>(
@"class TestClass
{
    public void Foo() {
        Foo3(Bar);
    }

    public void Foo3(Action act)
    {
        var x = act();
    }

    public void $Bar() {
        Console.Writeline(""Blah"");
    }

}",
@"class TestClass
{
    public void Foo() {
        Foo3(() => Console.Writeline(""Blah""));
    }

    public void Foo3(Action act)
    {
        var x = act();
    }

}");
        }

        [Test]
        [Description("Inline a method with a lambda, remove optional value.")]
        public void InlineMethodWithParenthesizedLambdaReference()
        {
            // Method Foo invokes Foo3 with a delegate Bar.
            // The delegate Bar will be replaced with inline method in the form of lambda syntax.
            // Additionally, the lambda method will contain the parameters of the inline method Bar.
            // Note that optional parameter b is not supported by lambda syntax and its default value is removed.
            // See the below stackoverflow answer for more detail.
            // http://stackoverflow.com/a/14249310/2735

            Test<InlineMethodAction>(
@"class TestClass
{
    public void Foo() {
        Foo3(Bar);
    }

    public void Foo3(Func<int, int, int> act)
    {
        var x = act(1, 2);
    }

    public int $Bar(int a, int b = 1) {
        //// Simple math.
        return (a + b);
    }

}",
@"class TestClass
{
    public void Foo() {
        Foo3((int a, int b) => (a + b));
    }

    public void Foo3(Func<int, int, int> act)
    {
        var x = act(1, 2);
    }

}");
        }

        [Test]
        [Description("Inline a method with multiple references.")]
        public void InlineMethodWithMultipleReferences()
        {
            // Test multiple references and syntax changes.
            // Modifying code will rearrange a document's syntax.
            // If 2 locations in the same document are known, changing the code for one location may invalidate the location of the other.

            Test<InlineMethodAction>(
@"class TestClass
{
    public void Foo() {
        var x = Bar(5, 1);
    }

    public int $Bar(int a, int b) {
        return (a + b);
    }

    public void Foo2() {
        var x2 = Bar(10, 2);
    }

}",
@"class TestClass
{
    public void Foo() {
        var x = (5 + 1);
    }

    public void Foo2() {
        var x2 = (10 + 2);
    }

}");
        }

        [Test]
        [Description("Inline a method with optional parameters.")]
        public void InlineMethodWithOptionalParam()
        {
            // Ensure that the optional parameter c in Bar is not lost.

            Test<InlineMethodAction>(
@"class TestClass
{
    public void Foo() {
        var x = Bar(5, 1);
    }

    public int $Bar(int a, int b, int c = 0) {
        //// Simple math.
        return (a + b + c);
    }

}",
@"class TestClass
{
    public void Foo() {
        var x = (5 + 1 + 0);
    }

}");
        }

        [Test]
        [Description("Inline a method with references in another classes.")]
        public void InlineMethodWithReferenceInAnotherClass()
        {
            Test<InlineMethodAction>(
@"class Dog : Animal
{
    public void DoLegs() {
        Bark();
    }
}

class Animal
{
    protected void $Bark() {
        //// Simple math.
        Console.Writeline(""bark"");
    }
}",
@"class Dog : Animal
{
    public void DoLegs() {
        Console.Writeline(""bark"");
    }
}

class Animal
{
}");
        }

        [Test]
        [Description("Test to ensure that references withing multiple documents are properly handled.")]
        public void MultipleDocumentTest()
        {
            // ClassA is in document named ClassA, ClassB is in document named ClassB.
            // Method ClassA.Foo invokes ClassB.Bar, the invocation of ClassB.Bar will be replaced by body of ClassB.Bar.

            var solutionId = SolutionId.CreateNewId();
            var projectId = ProjectId.CreateNewId();
            var projectName = "ProjectA";
            var document1Id = DocumentId.CreateNewId(projectId);
            var document1Name = "ClassA";
            var document2Id = DocumentId.CreateNewId(projectId);
            var document2Name = "ClassB";

            // Create Test Workspace
            var testWorkspace = new AdhocWorkspace(MefHostServices.DefaultHost, "Temp");
            testWorkspace.AddSolution(WorkspaceHelpers.CreateSolution(
                solutionId,
                new[] {
                    WorkspaceHelpers.CreateCSharpProject(projectId, projectName, new []
                    {
                        WorkspaceHelpers.CreateDocument(document1Id, document1Name,
@"class ClassA
{
    public void Foo() {
        var x = ClassB.Bar(5, 1);
    }
}"),
                        WorkspaceHelpers.CreateDocument(document2Id, document2Name,
@"class ClassB
{
    public void Foo() {
        var x = Bar(10, 2);
    }

    public static int $Bar(int a, int b, int c = 0) {
        //// Simple math.
        return (a + b + c);
    }

    public void Foo2() {
        var x = Bar(10, 2);
    }
}")
                    })
                }));

            // Create expected workspace
            var expWorkspace = new AdhocWorkspace();
            expWorkspace.AddSolution(WorkspaceHelpers.CreateSolution(
                solutionId,
                new[] {
                    WorkspaceHelpers.CreateCSharpProject(
                        projectId,
                        projectName,
                        new []
                        {
                            WorkspaceHelpers.CreateDocument(document1Id,document1Name,
@"class ClassA
{
    public void Foo() {
        var x = (5 + 1 + 0);
    }
}"),
                            WorkspaceHelpers.CreateDocument(document2Id,document2Name ,
@"class ClassB
{
    public void Foo() {
        var x = (10 + 2 + 0);
    }

    public void Foo2() {
        var x = (10 + 2 + 0);
    }
}")
                        })
            }));

            // Run action
            var alteredWorkspace = WorkspaceTestUtil.RunRefactoringProvider<InlineMethodAction>(testWorkspace);

            // Assert expected workspace equals altered workspace.
            WorkspaceTestUtil.AssertEqual(expWorkspace, alteredWorkspace);
        }

    }
}