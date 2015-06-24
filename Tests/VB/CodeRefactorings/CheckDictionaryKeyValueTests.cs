using NUnit.Framework;
using RefactoringEssentials.VB.CodeRefactorings;

namespace RefactoringEssentials.Tests.VB.CodeRefactorings
{
    [TestFixture]
    public class CheckDictionaryKeyValueTests : VBCodeRefactoringTestBase
    {
        [Test]
        public void TestSimpleCase()
        {
            Test<CheckDictionaryKeyValueCodeRefactoringProvider>(@"
Class Test
    Public Shared Sub Main(args As System.Collections.Generic.IDictionary(Of Integer, Integer))
        Console.WriteLine(args($5))
    End Sub
End Class", @"
Class Test
    Public Shared Sub Main(args As System.Collections.Generic.IDictionary(Of Integer, Integer))
        If args.ContainsKey(5) Then
            Console.WriteLine(args(5))
        End If
    End Sub
End Class");
        }

        [Test]
        public void TestNestedCase1()
        {
            Test<CheckDictionaryKeyValueCodeRefactoringProvider>(@"
Class Test
    Public Shared Sub Main(args As System.Collections.Generic.IDictionary(Of Integer, Integer))
        If True Then
            If True Then
                Console.WriteLine(args($5 + 234 - 234))
            End If
        End If
    End Sub
End Class", @"
Class Test
    Public Shared Sub Main(args As System.Collections.Generic.IDictionary(Of Integer, Integer))
        If True Then
            If True Then
                If args.ContainsKey(5 + 234 - 234) Then
                    Console.WriteLine(args(5 + 234 - 234))
                End If
            End If
        End If
    End Sub
End Class");
        }

        [Test]
        public void TestNestedCase2()
        {
            Test<CheckDictionaryKeyValueCodeRefactoringProvider>(@"
Class Test
    Public Shared Sub Main(args As System.Collections.Generic.IDictionary(Of Integer, Integer))
        If args.ContainsKey(5) Then
            Console.WriteLine(args($5))
        End If
    End Sub
End Class", @"
Class Test
    Public Shared Sub Main(args As System.Collections.Generic.IDictionary(Of Integer, Integer))
        If args.ContainsKey(5) Then
            If args.ContainsKey(5) Then
                Console.WriteLine(args(5))
            End If
        End If
    End Sub
End Class");
        }
    }
}