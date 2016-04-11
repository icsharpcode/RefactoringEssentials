using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
	[TestFixture]
	public class RedundantCheckBeforeAssignmentTests : CSharpDiagnosticTestBase
	{
		[Test]
		public void TestResharperDisableRestore()
		{
			Analyze<RedundantCheckBeforeAssignmentAnalyzer>(@"using System;
public class RedundantCheckBeforeAssignmentTests
{
    public void Method()
    {
        int q = 1;
//Resharper disable RedundantCheckBeforeAssignment
#pragma warning disable " + CSharpDiagnosticIDs.RedundantCheckBeforeAssignmentAnalyzerID + @"
        if (q != 1)
        {
            q = 1;
        }
//Resharper restore RedundantCheckBeforeAssignment
#pragma warning restore " + CSharpDiagnosticIDs.RedundantCheckBeforeAssignmentAnalyzerID + @"

    }
}
");
		}

		[Test]
		public void TestFix()
		{
			Analyze<RedundantCheckBeforeAssignmentAnalyzer>(@"using System;
class RedundantCheckBeforeAssignmentTests
{
    int test;

    public int TestProperty
    {
        get
        {
            return test;
        }
        set
        {
            if ($test != value$)
                test = value;
        }
    }
}
", @"using System;
class RedundantCheckBeforeAssignmentTests
{
    int test;

    public int TestProperty
    {
        get
        {
            return test;
        }
        set
        {
            test = value;
        }
    }
}
");
		}

		[Test]
		public void TestFixWithElse()
		{
			Analyze<RedundantCheckBeforeAssignmentAnalyzer>(@"using System;
class RedundantCheckBeforeAssignmentTests
{
    int test;

    public int TestProperty
    {
        get
        {
            return test;
        }
        set
        {
            if ($test != value$)
                test = value;
            else
                ;
        }
    }
}
", @"using System;
class RedundantCheckBeforeAssignmentTests
{
    int test;

    public int TestProperty
    {
        get
        {
            return test;
        }
        set
        {
            test = value;
        }
    }
}
");
		}

		[Test]
		public void TestQualifiedField()
		{
			Analyze<RedundantCheckBeforeAssignmentAnalyzer>(@"using System;
class RedundantCheckBeforeAssignmentTests
{
    int test;

    public int TestProperty
    {
        get
        {
            return test;
        }
        set
        {
            if ($this.test != value$)
                test = value;
        }
    }
}");
		}

		[Test]
		public void TestFlippedOperands()
		{
			Analyze<RedundantCheckBeforeAssignmentAnalyzer>(@"using System;
class RedundantCheckBeforeAssignmentTests
{
    int test;

    public int TestProperty
    {
        get
        {
            return test;
        }
        set
        {
            if ($value != test$)
                test = value;
        }
    }
}");
		}

		[Test]
		public void TestConstantExpr()
		{
			Analyze<RedundantCheckBeforeAssignmentAnalyzer>(@"using System;
class RedundantCheckBeforeAssignmentTests
{
    public void Test()
    {
        int q = 1;
        if ($q != 1$)
            q = 1;
    }
}");
		}

		[Test]
		public void TestConstantExprFlipped()
		{
			Analyze<RedundantCheckBeforeAssignmentAnalyzer>(@"using System;
class RedundantCheckBeforeAssignmentTests
{
    public void Test()
    {
        int q = 1;
        if ($1 != q$)
            q = 1;
    }
}");
		}

		[Test]
		public void TestBlock()
		{
			Analyze<RedundantCheckBeforeAssignmentAnalyzer>(@"using System;
class RedundantCheckBeforeAssignmentTests
{
    int test;

    public int TestProperty
    {
        get
        {
            return test;
        }
        set
        {
            if ($test != value$)
            {
                test = value;
            }
        }
    }
}");
		}

		[Test]
		public void TestIgnoreMultipleStatementsBlock()
		{
			Analyze<RedundantCheckBeforeAssignmentAnalyzer>(@"using System;
class RedundantCheckBeforeAssignmentTests
{
    int test;

    public int TestProperty
    {
        get
        {
            return test;
        }
        set
        {
            if (test != value)
            {
                test = value;
                RaiseOnPropertyChanged(nameof(TestProperty));
            }
        }
    }

    void RaiseOnPropertyChanged(string name) { /* stub */ }
}");
		}

		[TestCase("==")]
		[TestCase("<")]
		[TestCase(">")]
		[TestCase(">=")]
		[TestCase("<=")]
		public void TestIgnoreOtherComparisonOperators(string op)
		{
			Analyze<RedundantCheckBeforeAssignmentAnalyzer>(@"using System;
public class RE_Issue104
{
    public void Method(int expiryYear)
    {
        if (expiryYear \{op} 100)
        {
            expiryYear = 100;
        }
    }
}
");
		}

		[Test]
		public void TestIgnoreComplexExpression()
		{
			Analyze<RedundantCheckBeforeAssignmentAnalyzer>(@"using System;
public class RedundantCheckBeforeAssignmentTests
{
	int expiryYear;

    public void Method()
    {
        if (expiryYear != 100)
        {
            expiryYear = SomeHellOfAComplexAlgorithm(expiryYear);
        }
    }

	int SomeHellOfAComplexAlgorithm(int arg) { return 0; }
}
");
		}

		[Test]
		public void TestIgnoreREIssue_104()
		{
			Analyze<RedundantCheckBeforeAssignmentAnalyzer>(@"using System;
public class RedundantCheckBeforeAssignmentTests
{
    public void Method(int expiryYear)
    {
        if (expiryYear >= 100)
        {
            expiryYear = expiryYear % 100;
        }
    }
}
");
		}

		[Test]
		public void TestIgnoreWithElseBlock()
		{
			Analyze<RedundantCheckBeforeAssignmentAnalyzer>(@"using System;
class RedundantCheckBeforeAssignmentTests
{
    int test;

    public int TestProperty
    {
        get
        {
            return test;
        }
        set
        {
            if (test != value)
            {
                test = value;
            }
            else
            {
                Debug.Assert(false);
            }
        }
    }
}");
		}


		[Test]
		public void TestIgnoreWithElseIfBlock()
		{
			Analyze<RedundantCheckBeforeAssignmentAnalyzer>(@"using System;
class RedundantCheckBeforeAssignmentTests
{
    int test;

    public int TestProperty
    {
        get
        {
            return test;
        }
        set
        {
            if (test != value)
            {
                test = value;
            }
            else if (test > 2)
            {
                Debug.Assert(false);
            }
        }
    }
}");
		}

		[Test]
		public void TestWithEmptyElseBlock()
		{
			Analyze<RedundantCheckBeforeAssignmentAnalyzer>(@"using System;
class RedundantCheckBeforeAssignmentTests
{
    int test;

    public int TestProperty
    {
        get
        {
            return test;
        }
        set
        {
            if ($test != value$)
            {
                test = value;
            }
            else {}
        }
    }
}");
		}

		[Test]
		public void TestIgnoreWithElseStmt()
		{
			Analyze<RedundantCheckBeforeAssignmentAnalyzer>(@"using System;
class RedundantCheckBeforeAssignmentTests
{
    int test;

    public int TestProperty
    {
        get
        {
            return test;
        }
        set
        {
            if (test != value)
                test = value;
            else
                Debug.Assert(false);
        }
    }
}");
		}

		[Test]
		public void TestWithEmptyStmt()
		{
			Analyze<RedundantCheckBeforeAssignmentAnalyzer>(@"using System;
class RedundantCheckBeforeAssignmentTests
{
    int test;

    public int TestProperty
    {
        get
        {
            return test;
        }
        set
        {
            if ($test != value$)
            {
                test = value;
            }
            else ;
        }
    }
}");
		}
	}
}