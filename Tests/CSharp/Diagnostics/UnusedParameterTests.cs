using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    [Ignore("TODO: Issue not ported yet")]
    public class UnusedParameterTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestUnusedParameter()
        {
            var input = @"
class TestClass {
	void TestMethod (int i)
	{
	}
}";
            Test<UnusedParameterAnalyzer>(input, 1);
        }

        [Test]
        public void TestUnusedParameterMethodGetsCalled()
        {
            var input = @"
class TestClass {
	void TestMethod (int i)
	{
		TestMethod(0);
	}
}";
            Test<UnusedParameterAnalyzer>(input, 1);
        }

        [Test]
        public void TestInterfaceImplementation()
        {
            var input = @"
interface ITestClass {
	void TestMethod (int i);
}
class TestClass : ITestClass {
	public void TestMethod (int i)
	{
	}
}";
            Test<UnusedParameterAnalyzer>(input, 0);
        }

        [Test]
        public void TestAbstractMethodImplementation()
        {
            var input = @"
abstract class TestBase {
	public abstract void TestMethod (int i);
}
class TestClass : TestBase {
	public override void TestMethod (int i)
	{
	}
}";
            Test<UnusedParameterAnalyzer>(input, 0);
        }

        [Test]
        public void TestUsedParameter()
        {
            var input = @"
class TestClass {
	void TestMethod (int i)
	{
		i = 1;
	}
}";
            Test<UnusedParameterAnalyzer>(input, 0);
        }

        [Test]
        public void TestLambda()
        {
            var input = @"
class TestClass {
	void TestMethod ()
	{
		System.Action<int> a = i => {
		};
	}
}";
            Test<UnusedParameterAnalyzer>(input, 0);
        }

        [Test]
        public void TestAnonymousMethod()
        {
            var input = @"
class TestClass {
	void TestMethod ()
	{
		System.Action<int> a = delegate (int i) {
		};
	}
}";
            Test<UnusedParameterAnalyzer>(input, 0);
        }


        [Test]
        public void TestMethodUsedAsDelegateMethod()
        {
            var input = @"using System;
class TestClass {
	public event EventHandler FooEvt;
	void TestMethod ()
	{
		FooEvt += FooBar;
	}
	void FooBar (object sender, EventArgs e) {}
}";
            Test<UnusedParameterAnalyzer>(input, 0);
        }

        [Test]
        public void TestMethodLooksLikeEventHandlerButNotUsedAsSuch()
        {
            var input = @"using System;
class TestClass {
	void FooBar (object sender, EventArgs e) {}
}";
            Test<UnusedParameterAnalyzer>(input, 2);
        }

        [Test]
        public void TestMethodUsedAsDelegateInOtherPart()
        {
            // This test doesn't add the second part;
            // but the issue doesn't look at other files after all;
            // we just rely on heuristics if the class is partial
            var input = @"using System;
partial class TestClass {
	void FooBar (object sender, EventArgs e) {}
}";
            Test<UnusedParameterAnalyzer>(input, 0);
        }

        [Test]
        public void UnusedParameterInConstructor()
        {
            var input = @"
class TestClass {
	public TestClass(int i)
	{
	}
}";
            Test<UnusedParameterAnalyzer>(input, 1);
        }

        [Test]
        public void TestUnusedParameterInVirtualMethod()
        {
            var input = @"
class TestClass {
	public virtual void TestMethod (int i)
	{
	}
}";
            Test<UnusedParameterAnalyzer>(input, 0);
        }

        [Test]
        public void TestUnusedParameterInShadowedMethod()
        {
            var input = @"
class TestClass {
	public new void TestMethod (int i)
	{
	}
}";
            Test<UnusedParameterAnalyzer>(input, 0);
        }

        [Test]
        public void TestUnusedParameterInPartialMethod()
        {
            var input = @"
partial class TestClass {
	partial void TestMethod (int i)
	{
	}
}";
            Test<UnusedParameterAnalyzer>(input, 0);
        }

        [Test]
        public void SerializationConstructor()
        {
            var input = @"using System;
using System.Runtime.Serialization;
class TestClass : ISerializable {
	string text;
	protected TestClass(SerializationInfo info, StreamingContext context)
	{
		this.text = info.GetString(""Text"");
	}
}";
            Analyze<UnusedParameterAnalyzer>(input);
        }

        [Test]
        public void TestBug_29572()
        {
            // https://bugzilla.xamarin.com/show_bug.cgi?id=29572
            var input = @"using System;
using System.Runtime.Serialization;
class TestClass : ISerializable {
	
	[Export (""run:"")]
	public void Run (NSObject dummy)
	{
	    // something
	}
}";
            Analyze<UnusedParameterAnalyzer>(input);
        }
    }
}
