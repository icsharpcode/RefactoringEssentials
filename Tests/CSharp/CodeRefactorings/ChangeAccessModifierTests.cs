using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class ChangeAccessModifierTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestNoEnumMember()
        {
            TestWrongContext<ChangeAccessModifierAction>(@"
enum Test
{
	$Foo
}");
        }

        [Test]
        public void TestNoInterfaceMember()
        {
            TestWrongContext<ChangeAccessModifierAction>(@"
interface Test
{
	void $Foo ();
}");
        }

        [Test]
        public void TestNoExplicitInterfaceImplementationMember()
        {
            TestWrongContext<ChangeAccessModifierAction>(@"
interface Test
{
	void Foo ();
}
class TestClass : Test
{
	void $Test.Foo () {}
}");
        }

        [Test]
        public void TestNoOverrideMember()
        {
            TestWrongContext<ChangeAccessModifierAction>(@"
class TestClass : Test
{
	public override string $ToString()
	{
		return ""Test"";
	}
		
}");
        }

        [Test, Ignore("Not implemented!")]
        public void TestType()
        {
            Test<ChangeAccessModifierAction>(@"
class $Foo
{
}", @"
public class Foo
{
}");
        }

        [Test, Ignore("Not implemented!")]
        public void TestMethodToProtected()
        {
            Test<ChangeAccessModifierAction>(@"
class Foo
{
	void $Bar ()
	{
	}
}", @"
class Foo
{
	protected void Bar ()
	{
	}
}");
        }

        [Test, Ignore("Not implemented!")]
        public void TestPrivateMethodToProtected()
        {
            Test<ChangeAccessModifierAction>(@"
class Foo
{
	$private void Bar ()
	{
	}
}", @"
class Foo
{
	protected void Bar ()
	{
	}
}");
        }

        [Test, Ignore("Not implemented!")]
        public void TestMethodToProtectedInternal()
        {
            Test<ChangeAccessModifierAction>(@"
class Foo
{
	void $Bar ()
	{
	}
}", @"
class Foo
{
	protected internal void Bar ()
	{
	}
}", 1);
        }

        [Test, Ignore("Not implemented!")]
        public void TestAccessor()
        {
            Test<ChangeAccessModifierAction>(@"
class Foo
{
	public int Bar
	{
		get; $set;
	}
}", @"
class Foo
{
	public int Bar
	{
		get; private set;
	}
}");
        }

        [Test]
        public void TestStrictAccessor()
        {
            TestWrongContext<ChangeAccessModifierAction>(@"
class Foo
{
	private int Bar
	{
		get; $set;
	}
}");
        }

        [Test, Ignore("Not implemented!")]
        public void TestChangeAccessor()
        {
            Test<ChangeAccessModifierAction>(@"
class Foo
{
	public int Bar
	{
		get; private $set;
	}
}", @"
class Foo
{
	public int Bar
	{
		get; protected set;
	}
}");
        }

        [Test]
        public void TestReturnTypeWrongContext()
        {
            TestWrongContext<ChangeAccessModifierAction>(@"
class Test
{
	public $void Foo () {}
}");
        }

        [Test]
        public void TestWrongModiferContext()
        {
            TestWrongContext<ChangeAccessModifierAction>(@"
class Test
{
	public $virtual void Foo () {}
}");
        }

        [Test]
        public void TestMethodImplementingInterface()
        {
            TestWrongContext<ChangeAccessModifierAction>(@"using System;

class BaseClass : IDisposable
{
	public void $Dispose()
	{
	}
}");
        }


    }
}

