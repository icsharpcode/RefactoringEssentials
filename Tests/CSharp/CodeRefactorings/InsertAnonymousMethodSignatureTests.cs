using System;
using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class InsertAnonymousMethodSignatureTests : CSharpCodeRefactoringTestBase
    {
        [Test()]
        public void Test()
        {
            string result = RunContextAction(
                                         new InsertAnonymousMethodSignatureCodeRefactoringProvider(),
                                         "using System;" + Environment.NewLine +
                                         "class TestClass" + Environment.NewLine +
                                         "{" + Environment.NewLine +
                                         "    void Test ()" + Environment.NewLine +
                                         "    {" + Environment.NewLine +
                                         "        EventHandler handler = $delegate {" + Environment.NewLine +
                                         "        };" + Environment.NewLine +
                                         "    }" + Environment.NewLine +
                                         "}"
                                     );

            Assert.AreEqual(
                "using System;" + Environment.NewLine +
                "class TestClass" + Environment.NewLine +
                "{" + Environment.NewLine +
                "    void Test ()" + Environment.NewLine +
                "    {" + Environment.NewLine +
                "        EventHandler handler = delegate (object sender, EventArgs e)" + Environment.NewLine +
                "        {" + Environment.NewLine +
                "        };" + Environment.NewLine +
                "    }" + Environment.NewLine +
                "}", result);
        }
    }
}
