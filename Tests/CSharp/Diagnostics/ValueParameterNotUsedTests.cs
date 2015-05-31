using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class ValueParameterNotUsedTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestPropertySetter()
        {
            Analyze<ValueParameterNotUsedAnalyzer>(@"class A
{
	int Property1
	{
		set {
			int val = value;
		}
	}
	int Property2
	{
		$set$ {
		}
	}
}");
        }


        [Test]
        public void TestDisable()
        {
            Analyze<ValueParameterNotUsedAnalyzer>(@"class A
{
    int Property1
    {
        set
        {
            int val = value;
        }
    }
    int Property2
    {
#pragma warning disable " + CSharpDiagnosticIDs.ValueParameterNotUsedAnalyzerID + @"
        set
        {
        }
    }
}");
        }

        [Test]
        public void TestMatchingIndexerSetter()
        {
            Analyze<ValueParameterNotUsedAnalyzer>(@"class A
{
	A this[int index]
	{
		$set$ {
		}
	}
}");
        }

        [Test]
        public void TestMatchingEventAdder()
        {
            Analyze<ValueParameterNotUsedAnalyzer>(@"class A	
{
	delegate void TestEventHandler ();
	TestEventHandler eventTested;
	event TestEventHandler EventTested
	{
		add {
			eventTested += value;
		}
		$remove$ {
		}
	}
}");
        }

        [Test]
        public void TestNonMatchingIndexerSetter()
        {
            Analyze<ValueParameterNotUsedAnalyzer>(@"class A
{
	A this[int index]
	{
		set {
			A a = value;
		}
	}
}");
        }

        [Test]
        public void IgnoresAutoSetter()
        {
            Analyze<ValueParameterNotUsedAnalyzer>(@"class A
{
	string  Property { set; }
}");
        }

        [Test]
        public void IgnoreReadOnlyProperty()
        {
            Analyze<ValueParameterNotUsedAnalyzer>(@"class A
{
	string  Property { get; }
}");
        }

        [Test]
        public void DoesNotCrashOnNullIndexerAccessorBody()
        {
            Analyze<ValueParameterNotUsedAnalyzer>(@"abstract class A
{
	public abstract string this[int i] { get; set; }
}");
        }

        [Test]
        public void DoesNotWarnOnExceptionThrowingAccessor()
        {
            Analyze<ValueParameterNotUsedAnalyzer>(@"abstract class A
{
	public string Property
	{
		set {
			throw new Exception();
		}
	}
}");
        }

        [Test]
        public void DoesNotWarnOnEmptyCustomEvent()
        {
            // Empty custom events are often used when the event can never be raised
            // by a class (but the event is required e.g. due to an interface).
            Analyze<ValueParameterNotUsedAnalyzer>(@"class A	
{
	delegate void TestEventHandler ();
	event TestEventHandler EventTested
	{
		add { }
		remove { }
	}
}");
        }

        [Test]
        public void DoesNotWarnOnNotImplementedCustomEvent()
        {
            Analyze<ValueParameterNotUsedAnalyzer>(@"class A	
{
	delegate void TestEventHandler ();
	event TestEventHandler EventTested
	{
		add { throw new System.NotImplementedException(); }
		remove { throw new System.NotImplementedException(); }
	}
}");
        }
    }
}

