using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class UnusedParameterTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestUnusedParameter()
        {
            var input = @"
class TestClass {
	void TestMethod (int $i$)
	{
	}
}";
            Analyze<UnusedParameterAnalyzer>(input);
        }

        [Test]
        public void TestUnusedParameterMethodGetsCalled()
        {
            var input = @"
class TestClass {
	void TestMethod (int $i$)
	{
		TestMethod(0);
	}
}";
            Analyze<UnusedParameterAnalyzer>(input);
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
            Analyze<UnusedParameterAnalyzer>(input);
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
            Analyze<UnusedParameterAnalyzer>(input);
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
            Analyze<UnusedParameterAnalyzer>(input);
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
            Analyze<UnusedParameterAnalyzer>(input);
        }

        [Test]
        public void TestAnonymousMethod()
        {
            var input = @"
class TestClass {
	void TestMethod ()
	{
		System.Action<int> a = delegate (int $i$) {
		};
	}
}";
            Analyze<UnusedParameterAnalyzer>(input);
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
            Analyze<UnusedParameterAnalyzer>(input);
        }

        [Test]
        public void TestMethodLooksLikeEventHandlerButNotUsedAsSuch()
        {
            var input = @"using System;
class TestClass {
	void FooBar (object $sender$, EventArgs $e$) {}
}";
            Analyze<UnusedParameterAnalyzer>(input);
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
            Analyze<UnusedParameterAnalyzer>(input);
        }

        [Test]
        public void UnusedParameterInConstructor()
        {
            var input = @"
class TestClass {
	public TestClass(int $i$)
	{
	}
}";
            Analyze<UnusedParameterAnalyzer>(input);
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
            Analyze<UnusedParameterAnalyzer>(input);
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
            Analyze<UnusedParameterAnalyzer>(input);
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
            Analyze<UnusedParameterAnalyzer>(input);
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
using Foundation;

namespace Foundation {
    public class ExportAttribute : Attribute {}
}

class TestClass : ISerializable {
	
	[Export (""run:"")]
	public void Run (NSObject dummy)
	{
	    // something
	}
}";
            Analyze<UnusedParameterAnalyzer>(input);
        }

        [Test]
        [Ignore("Support for indexers disabled")]
        public void TestUnusedParameterInExpressionBodiedIndexer()
        {
            var input = @"
class TestClass {
	public string this[int $i$] => "";
}";
            Analyze<UnusedParameterAnalyzer>(input);
        }

        [Test]
        [Ignore("Support for indexers disabled")]
        public void TestUsedParameterInExpressionBodiedIndexer()
        {
            var input = @"
class TestClass {
	public string this[int i] => i.ToString();
}";
            Analyze<UnusedParameterAnalyzer>(input);
        }

        [Test]
        public void TestUnusedParameterInExpressionBodiedMethod()
        {
            var input = @"
class TestClass {
	public string TestMethod(int $i$) => "";
}";
            Analyze<UnusedParameterAnalyzer>(input);
        }

        [Test]
        public void TestUsedParameterInExpressionBodiedMethod()
        {
            var input = @"
class TestClass {
	public string TestMethod(int i) => i.ToString();
}";
            Analyze<UnusedParameterAnalyzer>(input);
        }
    }
}
