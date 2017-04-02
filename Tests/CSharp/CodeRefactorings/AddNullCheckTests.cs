using RefactoringEssentials.CSharp;
using RefactoringEssentials.Tests.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp
{
	/// <summary>
	/// Tests for AddNullCheckCodeRefactoringProvider.
	/// </summary>
    public class AddNullCheckTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestSingleExpression()
        {
            Test<AddNullCheckCodeRefactoringProvider>(@"
using System;

class TestClass
{
    public void TestMethod(string str)
    {
        Console.WriteLine($str);
    }
}", @"
using System;

class TestClass
{
    public void TestMethod(string str)
    {
        if (str != null)
        {
            Console.WriteLine(str);
        }
    }
}");
        }

        [Fact]
        public void TestValueType()
        {
            TestWrongContext<AddNullCheckCodeRefactoringProvider>(@"
using System;

class TestClass
{
    public void TestMethod(DateTime dateTime)
    {
        Console.WriteLine($dateTime);
    }
}");
        }

        [Fact]
        public void TestNullableType()
        {
            Test<AddNullCheckCodeRefactoringProvider>(@"
using System;

class TestClass
{
    public void TestMethod(DateTime? dateTime)
    {
        Console.WriteLine($dateTime);
    }
}", @"
using System;

class TestClass
{
    public void TestMethod(DateTime? dateTime)
    {
        if (dateTime != null)
        {
            Console.WriteLine(dateTime);
        }
    }
}");
        }

        [Fact]
        public void TestMemberAccessExpression1()
        {
            Test<AddNullCheckCodeRefactoringProvider>(@"
using System;

class SomeData
{
    public string Name { get; set; }
}

class TestClass
{
    public void TestMethod()
    {
        SomeData data = new SomeData();
        Console.WriteLine(data.$Name);
    }
}", @"
using System;

class SomeData
{
    public string Name { get; set; }
}

class TestClass
{
    public void TestMethod()
    {
        SomeData data = new SomeData();
        if (data.Name != null)
        {
            Console.WriteLine(data.Name);
        }
    }
}");
        }

        [Fact]
        public void TestMemberAccessExpression2()
        {
            Test<AddNullCheckCodeRefactoringProvider>(@"
using System;

class SomeData
{
    public string Name { get; set; }
}

class TestClass
{
    public void TestMethod()
    {
        SomeData data = new SomeData();
        Console.WriteLine($data.Name);
    }
}", @"
using System;

class SomeData
{
    public string Name { get; set; }
}

class TestClass
{
    public void TestMethod()
    {
        SomeData data = new SomeData();
        if (data != null)
        {
            Console.WriteLine(data.Name);
        }
    }
}");
        }

        [Fact]
        public void TestMemberAccessExpression3()
        {
            Test<AddNullCheckCodeRefactoringProvider>(@"
using System;

class SomeData
{
    public SomeSubData SubData { get; set; }
}

class SomeSubData
{
    public string Name { get; set; }
}

class TestClass
{
    public void TestMethod()
    {
        SomeData data = new SomeData();
        Console.WriteLine(data.$SubData.Name);
    }
}", @"
using System;

class SomeData
{
    public SomeSubData SubData { get; set; }
}

class SomeSubData
{
    public string Name { get; set; }
}

class TestClass
{
    public void TestMethod()
    {
        SomeData data = new SomeData();
        if (data.SubData != null)
        {
            Console.WriteLine(data.SubData.Name);
        }
    }
}");
        }

        [Fact]
        public void TestStaticMemberAccessExpression()
        {
            TestWrongContext<AddNullCheckCodeRefactoringProvider>(@"
using System;

class SomeData
{
    public static string Name { get; set; }
}

class TestClass
{
    public void TestMethod()
    {
        Console.WriteLine($SomeData.Name);
    }
}");
        }

        [Fact]
        public void TestMethodInvocation()
        {
            Test<AddNullCheckCodeRefactoringProvider>(@"
using System;

class SomeData
{
    public string Output()
    {
    }
}

class TestClass
{
    public void TestMethod()
    {
        SomeData data = new SomeData();
        $data.Output();
    }
}", @"
using System;

class SomeData
{
    public string Output()
    {
    }
}

class TestClass
{
    public void TestMethod()
    {
        SomeData data = new SomeData();
        if (data != null)
        {
            data.Output();
        }
    }
}");
        }

        [Fact]
        public void TestStaticMethodInvocation()
        {
            TestWrongContext<AddNullCheckCodeRefactoringProvider>(@"
using System;

class SomeData
{
    public static string Output()
    {
    }
}

class TestClass
{
    public void TestMethod()
    {
        $SomeData.Output();
    }
}");
        }

        [Fact]
        public void TestIndexerAccess()
        {
            Test<AddNullCheckCodeRefactoringProvider>(@"
using System;

class TestClass
{
    public void TestMethod(string str)
    {
        Console.WriteLine($str[0]);
    }
}", @"
using System;

class TestClass
{
    public void TestMethod(string str)
    {
        if (str != null)
        {
            Console.WriteLine(str[0]);
        }
    }
}");
        }

        [Fact]
        public void TestMultipleUsage()
        {
            Test<AddNullCheckCodeRefactoringProvider>(@"
using System;

class TestClass
{
    public void TestMethod(string str)
    {
        Console.WriteLine($str);
        string str2 = str.ToLower();
    }
}", @"
using System;

class TestClass
{
    public void TestMethod(string str)
    {
        if (str != null)
        {
            Console.WriteLine(str);
        }
        string str2 = str.ToLower();
    }
}");
        }

        [Fact]
        public void TestLocalVariable()
        {
            Test<AddNullCheckCodeRefactoringProvider>(@"
using System;

class TestClass
{
    public void TestMethod(string str)
    {
        Console.WriteLine(str);
        string str2 = str.ToLower();
        Console.WriteLine($str2);
    }
}", @"
using System;

class TestClass
{
    public void TestMethod(string str)
    {
        Console.WriteLine(str);
        string str2 = str.ToLower();
        if (str2 != null)
        {
            Console.WriteLine(str2);
        }
    }
}");
        }

        [Fact]
        public void TestLocalVariableDeclaration()
        {
            TestWrongContext<AddNullCheckCodeRefactoringProvider>(@"
using System;

class TestClass
{
    public void TestMethod(string str)
    {
        Console.WriteLine(str);
        string $str2 = str.ToLower();
        Console.WriteLine(str2);
    }
}");
        }

        [Fact]
        public void TestUsageInLocalVariableDeclaration()
        {
            TestWrongContext<AddNullCheckCodeRefactoringProvider>(@"
using System;

class TestClass
{
    public void TestMethod(string str)
    {
        Console.WriteLine(str);
        string str2 = $str.ToLower();
        Console.WriteLine(str2);
    }
}");
        }

        [Fact]
        public void TestUsageInReturnStatement()
        {
            TestWrongContext<AddNullCheckCodeRefactoringProvider>(@"
using System;

class TestClass
{
    public string TestMethod(string str)
    {
        return $str.ToLower();
    }
}");
        }

        [Fact]
        public void TestUsageInIfCondition()
        {
            Test<AddNullCheckCodeRefactoringProvider>(@"
using System;
using System.Collections.Generic;

class TestClass
{
    public void TestMethod(IEnumerable<string> list)
    {
        if ($list.Contains(""Bla""))
        {
            Console.WriteLine(""Contains 'Bla'"");
        }
    }
}", @"
using System;
using System.Collections.Generic;

class TestClass
{
    public void TestMethod(IEnumerable<string> list)
    {
        if ((list != null) && list.Contains(""Bla""))
        {
            Console.WriteLine(""Contains 'Bla'"");
        }
    }
}");
        }

        [Fact]
        public void TestUsageInIfBlock()
        {
            Test<AddNullCheckCodeRefactoringProvider>(@"
using System;
using System.Collections.Generic;

class TestClass
{
    public void TestMethod(int i, string str)
    {
        if (i > 0)
        {
            Console.WriteLine($str);
        }
    }
}", @"
using System;
using System.Collections.Generic;

class TestClass
{
    public void TestMethod(int i, string str)
    {
        if ((str != null) && (i > 0))
        {
            Console.WriteLine(str);
        }
    }
}");
        }

        [Fact]
        public void TestUsageInForLoop()
        {
            Test<AddNullCheckCodeRefactoringProvider>(@"
using System;
using System.Collections.Generic;

class TestClass
{
    public void TestMethod(IEnumerable<string> list)
    {
        foreach (var item in $list)
        {
            Console.WriteLine(item);
        }
    }
}", @"
using System;
using System.Collections.Generic;

class TestClass
{
    public void TestMethod(IEnumerable<string> list)
    {
        if (list != null)
        {
            foreach (var item in list)
            {
                Console.WriteLine(item);
            }
        }
    }
}");
        }

        [Fact]
        public void TestUsageInWhileLoop()
        {
            Test<AddNullCheckCodeRefactoringProvider>(@"
using System;
using System.Collections.Generic;

class TestClass
{
    public void TestMethod(IList<string> list)
    {
        while ($list.Count() > 0)
        {
            Console.WriteLine(item);
            list.RemoveAt(0);
        }
    }
}", @"
using System;
using System.Collections.Generic;

class TestClass
{
    public void TestMethod(IList<string> list)
    {
        if (list != null)
        {
            while (list.Count() > 0)
            {
                Console.WriteLine(item);
                list.RemoveAt(0);
            }
        }
    }
}");
        }

        [Fact]
        public void TestUsageInLambda()
        {
            TestWrongContext<AddNullCheckCodeRefactoringProvider>(@"
using System;
using System.Collections.Generic;
using System.Linq;

class TestClass
{
    public void TestMethod(IList<string> list)
    {
        var lambda = (list) => Console.WriteLine($list.FirstOrDefault());
    }
}");
        }

        [Fact]
        public void TestAlreadyPresentIfNotNullCheck1()
        {
            TestWrongContext<AddNullCheckCodeRefactoringProvider>(@"
using System;

class TestClass
{
    public void TestMethod(string str)
    {
        if (str != null)
        {
            Console.WriteLine($str);
        }
    }
}");
        }

        [Fact]
        public void TestAlreadyPresentIfNotNullCheck2()
        {
            TestWrongContext<AddNullCheckCodeRefactoringProvider>(@"
using System;
using System.Collections.Generic;

class TestClass
{
    public void TestMethod(IEnumerable<string> list)
    {
        if ((list != null) && list.Contains(""Bla""))
        {
            Console.WriteLine($list.First());
        }
    }
}");
        }

        [Fact]
        public void TestAlreadyPresentIfNotNullCheck3()
        {
            TestWrongContext<AddNullCheckCodeRefactoringProvider>(@"
using System;
using System.Collections.Generic;

class TestClass
{
    public void TestMethod(IEnumerable<string> list)
    {
        if (($list != null) && list.Contains(""Bla""))
        {
            Console.WriteLine(list.First());
        }
    }
}");
        }

        [Fact]
        public void TestAlreadyPresentIfNotNullCheck4()
        {
            TestWrongContext<AddNullCheckCodeRefactoringProvider>(@"
using System;
using System.Collections.Generic;

class TestClass
{
    public void TestMethod(IEnumerable<string> list)
    {
        if (($list != null) && list.Contains(""Bla""))
        {
            Console.WriteLine(list.First());
        }
    }
}");
        }

        [Fact]
        public void TestAlreadyPresentNotNullCheckInWhileLoop()
        {
            TestWrongContext<AddNullCheckCodeRefactoringProvider>(@"
using System;

class TestClass
{
    public void TestMethod(string str)
    {
        // Yes, this is an infinite loop
        while (str != null)
        {
            Console.WriteLine($str);
        }
    }
}");
        }

        [Fact]
        public void TestAlreadyPresentNullCheckInForLoop()
        {
            TestWrongContext<AddNullCheckCodeRefactoringProvider>(@"
using System;

class TestClass
{
    public void TestMethod(string str)
    {
        for (int i = 0; (str != null) && (i < 5); i++)
        {
            Console.WriteLine($str[i]);
        }
    }
}");
        }

        [Fact]
        public void TestAlreadyPresentNullCheckInConditionalTernaryExpression1()
        {
            TestWrongContext<AddNullCheckCodeRefactoringProvider>(@"
using System;
using System.Collections.Generic;

class TestClass
{
    public void TestMethod(IEnumerable<string> list)
    {
        Console.WriteLine((list != null ? $list.First() : "");
    }
}");
        }

        [Fact]
        public void TestAlreadyPresentNullCheckInConditionalTernaryExpression2()
        {
            TestWrongContext<AddNullCheckCodeRefactoringProvider>(@"
using System;
using System.Collections.Generic;

class TestClass
{
    public void TestMethod(IEnumerable<string> list)
    {
        Console.WriteLine(($list != null ? list.First() : "");
    }
}");
        }
    }
}

