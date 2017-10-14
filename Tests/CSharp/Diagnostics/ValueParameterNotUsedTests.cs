using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class ValueParameterNotUsedTests : CSharpDiagnosticTestBase
    {
        [Fact]
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


        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
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

        [Fact]
        public void IgnoresAutoSetter()
        {
            Analyze<ValueParameterNotUsedAnalyzer>(@"class A
{
	string  Property { set; }
}");
        }

        [Fact]
        public void IgnoreReadOnlyProperty()
        {
            Analyze<ValueParameterNotUsedAnalyzer>(@"class A
{
	string  Property { get; }
}");
        }

        [Fact]
        public void DoesNotCrashOnNullIndexerAccessorBody()
        {
            Analyze<ValueParameterNotUsedAnalyzer>(@"abstract class A
{
	public abstract string this[int i] { get; set; }
}");
        }

        [Fact]
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

        [Fact]
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

        [Fact]
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

