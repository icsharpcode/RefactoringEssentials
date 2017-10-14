using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class CreateCustomEventImplementationTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
        public void Test()
        {
            Test<CreateCustomEventImplementationAction>(@"
class TestClass
{
	public event System.EventHandler $TestEvent;
}", @"
class TestClass
{
    public event System.EventHandler TestEvent
    {
        add
        {
            throw new System.NotImplementedException();
        }

        remove
        {
            throw new System.NotImplementedException();
        }
    }
}");
        }

        [Fact]
        public void TestSimplification()
        {
            Test<CreateCustomEventImplementationAction>(@"
using System;
class TestClass
{
	public event EventHandler $TestEvent;
}", @"
using System;
class TestClass
{
    public event EventHandler TestEvent
    {
        add
        {
            throw new NotImplementedException();
        }

        remove
        {
            throw new NotImplementedException();
        }
    }
}");
        }

        [Fact]
        public void TestMultipleEventDeclaration()
        {
            Test<CreateCustomEventImplementationAction>(@"
using System;
class TestClass
{
	event EventHandler $TestEvent, TestEvent2;
}", @"
using System;
class TestClass
{
    event EventHandler TestEvent2;

    event EventHandler TestEvent
    {
        add
        {
            throw new NotImplementedException();
        }

        remove
        {
            throw new NotImplementedException();
        }
    }
}");
        }

        [Fact]
        public void TestInterfaceContext()
        {
            TestWrongContext<CreateCustomEventImplementationAction>(
                @"interface Test { event EventHandler $TestEvent; }"
            );
        }
    }
}
