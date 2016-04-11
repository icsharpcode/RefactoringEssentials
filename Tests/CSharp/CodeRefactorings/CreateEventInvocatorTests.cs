using NUnit.Framework;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class CreateEventInvocatorTests : CSharpCodeRefactoringTestBase
    {
        [Test]
        public void TestSimpleCase()
        {
            Test<CreateEventInvocatorCodeRefactoringProvider>(@"using System;
class TestClass
{
    public event EventHandler $Tested;
}", @"using System;
class TestClass
{
    protected virtual void OnTested(EventArgs e)
    {
        Tested?.Invoke(this, e);
    }

    public event EventHandler Tested;
}");
        }

        [Test]
        public void Test_CSharp5_SimpleCase()
        {
            var parseOptions = new CSharpParseOptions(
                                            LanguageVersion.CSharp5,
                                            DocumentationMode.Diagnose | DocumentationMode.Parse,
                                            SourceCodeKind.Regular,
                                            ImmutableArray.Create("DEBUG", "TEST")
                                        );
            Test<CreateEventInvocatorCodeRefactoringProvider>(@"using System;
class TestClass
{
    public event EventHandler $Tested;
}", @"using System;
class TestClass
{
    protected virtual void OnTested(EventArgs e)
    {
        var handler = Tested;
        if (handler != null)
            handler(this, e);
    }

    public event EventHandler Tested;
}", parseOptions: parseOptions);
        }

        [Test]
        public void Test_CSharp5_NameClash()
        {
            var parseOptions = new CSharpParseOptions(
                                            LanguageVersion.CSharp5,
                                            DocumentationMode.Diagnose | DocumentationMode.Parse,
                                            SourceCodeKind.Regular,
                                            ImmutableArray.Create("DEBUG", "TEST")
                                        );
            Test<CreateEventInvocatorCodeRefactoringProvider>(@"using System;
class TestClass
{
    public event EventHandler $e;
}", @"using System;
class TestClass
{
    protected virtual void OnE(EventArgs e)
    {
        var handler = this.e;
        if (handler != null)
            handler(this, e);
    }

    public event EventHandler e;
}", parseOptions: parseOptions);
        }

        [Test]
        public void TestNameClash()
        {
            Test<CreateEventInvocatorCodeRefactoringProvider>(@"using System;
class TestClass
{
    public event EventHandler $e;
}", @"using System;
class TestClass
{
    protected virtual void OnE(EventArgs e)
    {
        this.e?.Invoke(this, e);
    }

    public event EventHandler e;
}");
        }

        [Test]
        public void TestStaticEvent()
        {
            Test<CreateEventInvocatorCodeRefactoringProvider>(@"using System;
class TestClass
{
    public static event EventHandler $Tested;
}", @"using System;
class TestClass
{
    static void OnTested(EventArgs e)
    {
        Tested?.Invoke(null, e);
    }

    public static event EventHandler Tested;
}");
        }


        [Test]
        public void TestStaticNameClash()
        {
            Test<CreateEventInvocatorCodeRefactoringProvider>(@"using System;
class TestClass
{
    public static event EventHandler $e;
}", @"using System;
class TestClass
{
    static void OnE(EventArgs e)
    {
        TestClass.e?.Invoke(null, e);
    }

    public static event EventHandler e;
}");
        }

        [Test]
        public void TestUnusualEventHandler()
        {
            Test<CreateEventInvocatorCodeRefactoringProvider>(@"using System;
public delegate void UnusualEventHandler(EventArgs e);
class TestClass
{
    public event UnusualEventHandler $Tested;
}", @"using System;
public delegate void UnusualEventHandler(EventArgs e);
class TestClass
{
    protected virtual void OnTested(EventArgs e)
    {
        Tested?.Invoke(e);
    }

    public event UnusualEventHandler Tested;
}");
        }
    }
}

