using RefactoringEssentials.VB.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.VB.CodeRefactorings
{
    public class ConvertToCustomEventTests : VBCodeRefactoringTestBase
    {
        [Fact]
        public void TestDelegateBasedEvent()
        {
            Test<ConvertToCustomEventCodeRefactoringProvider>(@"
Imports System

Class Test
    Public Event $PropertyChanged As EventHandler
End Class", @"
Imports System

Class Test
    Public Custom Event PropertyChanged As EventHandler
        AddHandler(ByVal value As EventHandler)
            Throw New System.NotImplementedException()
        End AddHandler
        RemoveHandler(ByVal value As EventHandler)
            Throw New System.NotImplementedException()
        End RemoveHandler
        RaiseEvent(ByVal sender As Object, ByVal e As System.EventArgs)
            Throw New System.NotImplementedException()
        End RaiseEvent
    End Event
End Class");
        }

        [Fact]
        public void TestGenericDelegateBasedEvent()
        {
            Test<ConvertToCustomEventCodeRefactoringProvider>(@"
Imports System

Class Test
    Public Event $PropertyChanged As EventHandler(Of ConsoleCancelEventArgs)
End Class", @"
Imports System

Class Test
    Public Custom Event PropertyChanged As EventHandler(Of ConsoleCancelEventArgs)
        AddHandler(ByVal value As EventHandler(Of ConsoleCancelEventArgs))
            Throw New System.NotImplementedException()
        End AddHandler
        RemoveHandler(ByVal value As EventHandler(Of ConsoleCancelEventArgs))
            Throw New System.NotImplementedException()
        End RemoveHandler
        RaiseEvent(ByVal sender As Object, ByVal e As System.ConsoleCancelEventArgs)
            Throw New System.NotImplementedException()
        End RaiseEvent
    End Event
End Class");
        }

[Fact]
        public void TestEventBasedOnCustomDelegateWithGenericParam()
        {
            Test<ConvertToCustomEventCodeRefactoringProvider>(@"
Imports System

Public Delegate Sub CustomEventDelegate(ByVal enumeration As System.Collections.Generic.IEnumerable(Of String))

Class Test
    Public Event $PropertyChanged As CustomEventDelegate
End Class", @"
Imports System

Public Delegate Sub CustomEventDelegate(ByVal enumeration As System.Collections.Generic.IEnumerable(Of String))

Class Test
    Public Custom Event PropertyChanged As CustomEventDelegate
        AddHandler(ByVal value As CustomEventDelegate)
            Throw New System.NotImplementedException()
        End AddHandler
        RemoveHandler(ByVal value As CustomEventDelegate)
            Throw New System.NotImplementedException()
        End RemoveHandler
        RaiseEvent(ByVal enumeration As System.Collections.Generic.IEnumerable(Of String))
            Throw New System.NotImplementedException()
        End RaiseEvent
    End Event
End Class");
        }

        [Fact]
        public void TestNonDelegateBasedEvent()
        {
            TestWrongContext<ConvertToCustomEventCodeRefactoringProvider>(@"
Class Test
    Public Event $PropertyChanged(ByVal sender As Object, ByVal e As System.EventArgs)
End Class");
        }
    }
}