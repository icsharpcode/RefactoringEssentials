using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [Ignore("Needs insertion cursor mode.")]
    [TestFixture]
    public class CreateDelegateTests : ContextActionTestBase
    {
        [Test]
        public void TestCreateDelegate()
        {
            Test<CreateDelegateAction>(
@"
class TestClass
{
	event $MyEventHandler evt;
}
", @"
delegate void MyEventHandler (object sender, System.EventArgs e);
class TestClass
{
	event MyEventHandler evt;
}
");
        }

        [Test]
        public void TestCreatePublicDelegate()
        {
            Test<CreateDelegateAction>(
@"
class TestClass
{
	public event $MyEventHandler evt;
}
", @"
public delegate void MyEventHandler (object sender, System.EventArgs e);
class TestClass
{
	public event MyEventHandler evt;
}
");
        }

    }
}
