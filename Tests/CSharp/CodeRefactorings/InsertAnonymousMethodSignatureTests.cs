using System;
using RefactoringEssentials.CSharp.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    public class InsertAnonymousMethodSignatureTests : CSharpCodeRefactoringTestBase
    {
        [Fact]
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

            Assert.Equal(
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
