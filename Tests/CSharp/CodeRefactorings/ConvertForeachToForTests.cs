using System;
using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class ConvertForeachToForTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void TestArray()
        {
            string result = RunContextAction(
                                         new ConvertForeachToForCodeRefactoringProvider(),
                                         "using System;" + Environment.NewLine +
                                         "class TestClass" + Environment.NewLine +
                                         "{" + Environment.NewLine +
                                         "    void Test (string[] args)" + Environment.NewLine +
                                         "    {" + Environment.NewLine +
                                         "        $foreach (var v in args) {" + Environment.NewLine +
                                         "            Console.WriteLine (v);" + Environment.NewLine +
                                         "        }" + Environment.NewLine +
                                         "    }" + Environment.NewLine +
                                         "}"
                                     );

            Assert.Equal(
                "using System;" + Environment.NewLine +
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "    void Test (string[] args)" + Environment.NewLine +
                "    {" + Environment.NewLine +
                "        for (int i = 0; i < args.Length; i++)" + Environment.NewLine +
                "        {" + Environment.NewLine +
                "            var v = args[i];" + Environment.NewLine +
                "            Console.WriteLine(v);" + Environment.NewLine +
                "        }" + Environment.NewLine +
                "    }" + Environment.NewLine +
                "}", result);
        }

        [Fact]
        public void TestListOfT()
        {
            string result = RunContextAction(
                                         new ConvertForeachToForCodeRefactoringProvider(),
                                         "using System;" + Environment.NewLine +
                                         "using System.Collections.Generic;" + Environment.NewLine +
                                         "class TestClass" + Environment.NewLine +
                                         "{" + Environment.NewLine +
                                         "    void Test (List<string> args)" + Environment.NewLine +
                                         "    {" + Environment.NewLine +
                                         "        $foreach (var v in args) {" + Environment.NewLine +
                                         "            Console.WriteLine(v);" + Environment.NewLine +
                                         "        }" + Environment.NewLine +
                                         "    }" + Environment.NewLine +
                                         "}"
                                     );

            Assert.Equal(
                "using System;" + Environment.NewLine +
                "using System.Collections.Generic;" + Environment.NewLine +
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "    void Test (List<string> args)" + Environment.NewLine +
                "    {" + Environment.NewLine +
                "        for (int i = 0; i < args.Count; i++)" + Environment.NewLine +
                "        {" + Environment.NewLine +
                "            var v = args[i];" + Environment.NewLine +
                "            Console.WriteLine(v);" + Environment.NewLine +
                "        }" + Environment.NewLine +
                "    }" + Environment.NewLine +
                "}", result);
        }

        /// <summary>
        /// Bug 9876 - Convert to for loop created invalid code if iteration variable is called i
        /// </summary>
        [Fact]
        public void TestBug9876()
        {
            Test<ConvertForeachToForCodeRefactoringProvider>(@"class TestClass
{
    void TestMethod ()
    {
        $foreach (var i in new[] { 1, 2, 3 }) {
            Console.WriteLine (i);
        }
    }
}", @"class TestClass
{
    void TestMethod ()
    {
        var list = new[] { 1, 2, 3 };
        for (int i = 0; i < list.Length; i++)
        {
            var i = list[i];
            Console.WriteLine(i);
        }
    }
}");
        }

        [Fact]
        public void TestOptimizedForLoop()
        {
            Test<ConvertForeachToForCodeRefactoringProvider>(@"
class Test
{
    void Foo (object[] o)
    {
        $foreach (var p in o) {
            System.Console.WriteLine (p);
        }
    }
}", @"
class Test
{
    void Foo (object[] o)
    {
        for (int i = 0, oLength = o.Length; i < oLength; i++)
        {
            var p = o[i];
            System.Console.WriteLine(p);
        }
    }
}", 1);
        }

        [Fact]
        public void TestOptimizedForLoopWithComment()
        {
            Test<ConvertForeachToForCodeRefactoringProvider>(@"
class Test
{
    void Foo (object[] o)
    {
        // Some comment
        $foreach (var p in o) {
            System.Console.WriteLine (p);
        }
    }
}", @"
class Test
{
    void Foo (object[] o)
    {
        // Some comment
        for (int i = 0, oLength = o.Length; i < oLength; i++)
        {
            var p = o[i];
            System.Console.WriteLine(p);
        }
    }
}", 1);
        }

        [Fact]
        public void TestEnumerableConversion()
        {
            Test<ConvertForeachToForCodeRefactoringProvider>(@"
using System;
using System.Collections.Generic;

class Test
{
    public void Foo (IEnumerable<string> bar)
    {
        $foreach (var b in bar) {
            Console.WriteLine (b);
        }
    }
}", @"
using System;
using System.Collections.Generic;

class Test
{
    public void Foo (IEnumerable<string> bar)
    {
        for (var i = bar.GetEnumerator(); i.MoveNext();)
        {
            var b = i.Current;
            Console.WriteLine(b);
        }
    }
}");
        }

        [Fact]
        public void TestEnumerableConversionWithComment()
        {
            Test<ConvertForeachToForCodeRefactoringProvider>(@"
using System;
using System.Collections.Generic;

class Test
{
    public void Foo (IEnumerable<string> bar)
    {
        // Some comment
        $foreach (var b in bar) {
            Console.WriteLine (b);
        }
    }
}", @"
using System;
using System.Collections.Generic;

class Test
{
    public void Foo (IEnumerable<string> bar)
    {
        // Some comment
        for (var i = bar.GetEnumerator(); i.MoveNext();)
        {
            var b = i.Current;
            Console.WriteLine(b);
        }
    }
}");
        }

        /// <summary>
        /// Bug 30238 - [Roslyn Migration] Smart tag follows cursor while typing comment
        /// </summary>
        [Fact]
        public void TestBug30238()
        {
            TestWrongContext<ConvertForeachToForCodeRefactoringProvider>(@"
class Test
{
    void Foo (object[] o)
    {
        $// Some comment
        foreach (var p in o) {
            System.Console.WriteLine (p);
        }
    }
}");
        }

    }
}