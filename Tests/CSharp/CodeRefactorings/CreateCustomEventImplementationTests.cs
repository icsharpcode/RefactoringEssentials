using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class CreateCustomEventImplementationTests : CSharpCodeRefactoringTestBase
    {
        [Test]
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

        [Test]
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

        [Test]
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
    }
}
