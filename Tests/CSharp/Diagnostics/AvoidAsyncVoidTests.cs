using NUnit.Framework;
using RefactoringEssentials.CSharp.Diagnostics;
using RefactoringEssentials.CSharp.Diagnostics.Custom;

namespace RefactoringEssentials.Tests.CSharp.Diagnostics
{
    [TestFixture]
    public class AvoidAsyncVoidTests : CSharpDiagnosticTestBase
    {
        [Test]
        public void TestAsyncMethod()
        {
            var input = @"
class TestClass
{
    async void $TestMethodAsync$()
    {
    }
}";

            Analyze<AvoidAsyncVoidAnalyzer>(input );
        }

        [Test]
        public void TestAsyncEventHandlerMethod()
        {
            var input = @"
class TestClass
{
    async void TestMethodAsync(object sender, EventArgs e)
    {
    }
}";

            Analyze<AvoidAsyncVoidAnalyzer>(input);
        }

        [Test]
        public void TestDisable()
        {
            var input = @"
class TestClass
{
#pragma warning disable " + CSharpDiagnosticIDs.AvoidAsyncVoidAnalyzerID + @"
    async void TestMethodAsync()
    {
    }
}";

            Analyze<AvoidAsyncVoidAnalyzer>(input);
        }

        [Test]
        public void TestLambda()
        {
            var input = @"
using System;
class TestClass
{
    static event Action<object> SingleArgumentEvent;
    public ClassName()
    {
        AppDomain.CurrentDomain.DomainUnload += $async$ (sender, e) => { };
        AppDomain.CurrentDomain.DomainUnload += $async$ delegate (object sender, EventArgs e) { };
        SingleArgumentEvent += $async$ arg => { };
    }
}
";

            Analyze<AvoidAsyncVoidAnalyzer>(input);
        }
    }
}
