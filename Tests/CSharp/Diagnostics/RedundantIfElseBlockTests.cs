using RefactoringEssentials.CSharp.Diagnostics;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    public class RedundantIfElseBlockTests : CSharpDiagnosticTestBase
    {
        [Fact]
        public void TestReturn()
        {
            var input = @"
class TestClass
{
    int TestMethod (int i)
    {
        if (i > 0)
            return 1;
        $else$
            return 0;
    }
}";
            var output = @"
class TestClass
{
    int TestMethod (int i)
    {
        if (i > 0)
            return 1;
        return 0;
    }
}";
            Analyze<RedundantIfElseBlockAnalyzer>(input, output);
        }

        [Fact]
        public void TestDisable()
        {
            var input = @"
class TestClass
{
    int TestMethod (int i)
    {
        if (i > 0)
            return 1;
#pragma warning disable " + CSharpDiagnosticIDs.RedundantIfElseBlockAnalyzerID + @"
        else
            return 0;
    }
}";
            Analyze<RedundantIfElseBlockAnalyzer>(input);
        }

        [Fact]
        public void TestBreakLoop()
        {
            var input = @"
class TestClass
{
    void TestMethod ()
    {
        int k = 0;
        for (int i = 0; i < 10; i++) {
            if (i > 5)
                break;
            $else$
                k++;
        }
    }
}";
            var output = @"
class TestClass
{
    void TestMethod ()
    {
        int k = 0;
        for (int i = 0; i < 10; i++) {
            if (i > 5)
                break;
            k++;
        }
    }
}";
            Analyze<RedundantIfElseBlockAnalyzer>(input, output);
        }

        [Fact]
        public void TestContinueLoop()
        {
            var input = @"
class TestClass
{
    void TestMethod ()
    {
        int k = 0;
        for (int i = 0; i < 10; i++) {
            if (i > 5)
                continue;
            $else$
                k++;
        }
    }
}";
            var output = @"
class TestClass
{
    void TestMethod ()
    {
        int k = 0;
        for (int i = 0; i < 10; i++) {
            if (i > 5)
                continue;
            k++;
        }
    }
}";
            Analyze<RedundantIfElseBlockAnalyzer>(input, output);
        }

        [Fact]
        public void TestBlockStatement()
        {
            var input = @"
class TestClass
{
    int TestMethod (int i)
    {
        if (i > 0)
        {
            return 1;
        } $else$
        {
            return 0;
        }
    }
}";
            var output = @"
class TestClass
{
    int TestMethod (int i)
    {
        if (i > 0)
        {
            return 1;
        }
        return 0;
    }
}";
            Analyze<RedundantIfElseBlockAnalyzer>(input, output);
        }

        [Fact]
        public void TestEmptyFalseBlock()
        {
            var input = @"
class TestClass
{
    void TestMethod (int i)
    {
        int a;
        if (i > 0)
            a = 1;
        $else$ { }
    }
}";
            var output = @"
class TestClass
{
    void TestMethod (int i)
    {
        int a;
        if (i > 0)
            a = 1;
    }
}";
            Analyze<RedundantIfElseBlockAnalyzer>(input, output);
        }

        [Fact]
        public void TestNecessaryElse()
        {

            var input = @"
class TestClass
{
    void TestMethod (int i)
    {
        int a;
        if (i > 0)
            a = 1;
        else
            a = 0;
    }
}";
            Analyze<RedundantIfElseBlockAnalyzer>(input);
        }

        [Fact]
        public void TestNecessaryElseCase2()
        {

            var input = @"
class TestClass
{
    void TestMethod (int i)
    {
        int a;
        while (true) {
            if (i > 0) {
                a = 1;
            } else if (i < 0) {
                a = 0;
                break;
            } else {
                break;
            }
        }
    }
}";
            Analyze<RedundantIfElseBlockAnalyzer>(input);
        }

        [Fact]

        public void TestNecessaryElseBecauseOfVarDeclaration()
        {

            var input = @"
class TestClass
{
    void TestMethod (int i)
    {
        if (i > 0) {
            int a = 1;
            return;
        } else {
            int a = 2;
            return;
        }
    }
}";
            Analyze<RedundantIfElseBlockAnalyzer>(input);
        }

        [Fact]
        public void TestNecessaryElseBecauseOfVarDeclarationInDifferentStatement()
        {
            var input = @"
class TestClass
{
    void TestMethod (int i)
    {
        {
            int a = 1;
        }
        if (i > 0) {
            return;
        } else {
            int a = 2;
            return;
        }
    }
}";
            Analyze<RedundantIfElseBlockAnalyzer>(input);
        }

        [Fact]
        public void TestReturn2()
        {
            var input = @"
class TestClass
{
    bool? EvaluateCondition (Expression expr)
    {
        ResolveResult rr = EvaluateConstant (expr);
        if (rr != null && rr.IsCompileTimeConstant)
            return rr.ConstantValue as bool?;
        $else$
            return null;
    }
}";
            var output = @"
class TestClass
{
    bool? EvaluateCondition (Expression expr)
    {
        ResolveResult rr = EvaluateConstant (expr);
        if (rr != null && rr.IsCompileTimeConstant)
            return rr.ConstantValue as bool?;
        return null;
    }
}";
            Analyze<RedundantIfElseBlockAnalyzer>(input, output);
        }

    }
}
