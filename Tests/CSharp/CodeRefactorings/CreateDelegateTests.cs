using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class CreateDelegateTests : CSharpCodeRefactoringTestBase
    {

        [TestCase(@"$event MyEventHandler evt;")]
        [TestCase(@"event $MyEventHandler evt;")]
        [TestCase(@"event MyEventHandler $evt;")]
        [Description("Test that an undefined delegate gets created.")]
        public void TestCreateDelegate(string given)
        {
            Test<CreateDelegateAction>(
@"class TestClass
{
	" + given + @"
}
",
@"delegate void MyEventHandler(object sender, System.EventArgs e);

class TestClass
{
	event MyEventHandler evt;
}
");
        }

        [TestCase(@"$public event MyEventHandler evt;")]
        [TestCase(@"public $event MyEventHandler evt;")]
        [TestCase(@"public event $MyEventHandler evt;")]
        [TestCase(@"public event MyEventHandler $evt;")]
        [Description("Test that an undefined delegate gets created and maintains modifiers.")]
        public void TestCreatePublicDelegate(string given)
        {
            Test<CreateDelegateAction>(
@"class TestClass
{
    " + given + @"
}",
@"public delegate void MyEventHandler(object sender, System.EventArgs e);

class TestClass
{
    public event MyEventHandler evt;
}");
        }

        [Test]
        [Description("Test that an undefined delegate gets created and properly uses the static modifier.")]
        public void TestCreatePublicStaticDelegate()
        {
            Test<CreateDelegateAction>(
@"class TestClass
{
    public static event $MyEventHandler evt;
}",
@"public delegate void MyEventHandler(object sender, System.EventArgs e);

class TestClass
{
    public static event MyEventHandler evt;
}");
        }


        [TestCase(@"$event MyEventHandler evt;")]
        [TestCase(@"event $MyEventHandler evt;")]
        [TestCase(@"event MyEventHandler $evt;")]
        [Description("Test that an undefined delegate gets created within namespace.")]
        public void TestCreateDelegateWithNamespace(string given)
        {
            Test<CreateDelegateAction>(
@"namespace foo
{
    class TestClass
    {
        " + given + @"
    }
}",
@"namespace foo
{
    delegate void MyEventHandler(object sender, System.EventArgs e);

    class TestClass
    {
        event MyEventHandler evt;
    }
}");
        }

        [TestCase(@"$event EventHandler evt;")]
        [TestCase(@"event $EventHandler evt;")]
        [TestCase(@"event EventHandler $evt;")]
        [TestCase(@"$event EventHandler<AssemblyLoadEventArgs> evt;")]
        [TestCase(@"event $EventHandler<AssemblyLoadEventArgs> evt;")]
        [TestCase(@"event EventHandler<AssemblyLoadEventArgs> $evt;")]
        [Description("Test that a delegate does not get created for standard EventHandler and EventHandler<> types.")]
        public void TestDoesNotCreateDelegate(string given)
        {
            TestWrongContext<CreateDelegateAction>(
@"using System;

namespace foo
{
    class TestClass
    {
        " + given + @"
    }
}");
        }
    }
}
