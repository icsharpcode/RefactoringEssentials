using RefactoringEssentials.VB.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.VB.CodeRefactorings
{
    public class CreateChangedEventTests : VBCodeRefactoringTestBase
    {
        [Fact]
        public void TestSimpleCase()
        {
            Test<CreateChangedEventCodeRefactoringProvider>(@"
Class TestClass
    Dim _test As String

    Public Property $Test As String
        Get
            Return _test
        End Get
        Set(ByVal value As String)
            _test = value
        End Set
    End Property
End Class", @"
Class TestClass
    Dim _test As String

    Public Property Test As String
        Get
            Return _test
        End Get
        Set(ByVal value As String)
            _test = value
            OnTestChanged(System.EventArgs.Empty)
        End Set
    End Property

    Protected Overridable Sub OnTestChanged(e As System.EventArgs)
        RaiseEvent TestChanged(Me, e)
    End Sub

    Public Event TestChanged As System.EventHandler
End Class");
        }

        [Fact]
        public void TestSimplify()
        {
            Test<CreateChangedEventCodeRefactoringProvider>(@"Imports System
Class TestClass
    Dim _test As String

    Public Property $Test As String
        Get
            Return _test
        End Get
        Set(ByVal value As String)
            _test = value
        End Set
    End Property
End Class", @"Imports System
Class TestClass
    Dim _test As String

    Public Property Test As String
        Get
            Return _test
        End Get
        Set(ByVal value As String)
            _test = value
            OnTestChanged(EventArgs.Empty)
        End Set
    End Property

    Protected Overridable Sub OnTestChanged(e As EventArgs)
        RaiseEvent TestChanged(Me, e)
    End Sub

    Public Event TestChanged As EventHandler
End Class");
        }

        [Fact]
        public void TestSharedPropertyCase()
        {
            Test<CreateChangedEventCodeRefactoringProvider>(@"
NotInheritable Class TestClass
    Shared _test As String

    Public Shared Property $Test As String
        Get
            Return _test
        End Get
        Set(ByVal value As String)
            _test = value
        End Set
    End Property
End Class", @"
NotInheritable Class TestClass
    Shared _test As String

    Public Shared Property Test As String
        Get
            Return _test
        End Get
        Set(ByVal value As String)
            _test = value
            OnTestChanged(System.EventArgs.Empty)
        End Set
    End Property

    Protected Shared Sub OnTestChanged(e As System.EventArgs)
        RaiseEvent TestChanged(Nothing, e)
    End Sub

    Public Shared Event TestChanged As System.EventHandler
End Class");
        }

        [Fact]
        public void TestNonInheritableClassCase()
        {
            Test<CreateChangedEventCodeRefactoringProvider>(@"
NotInheritable Class TestClass
    Dim _test As String

    Public Property $Test As String
        Get
            Return _test
        End Get
        Set(ByVal value As String)
            _test = value
        End Set
    End Property
End Class", @"
NotInheritable Class TestClass
    Dim _test As String

    Public Property Test As String
        Get
            Return _test
        End Get
        Set(ByVal value As String)
            _test = value
            OnTestChanged(System.EventArgs.Empty)
        End Set
    End Property

    Sub OnTestChanged(e As System.EventArgs)
        RaiseEvent TestChanged(Me, e)
    End Sub

    Public Event TestChanged As System.EventHandler
End Class");
        }

        [Fact]
        public void TestReadOnlyProperty()
        {
            TestWrongContext<CreateChangedEventCodeRefactoringProvider>(@"
Class TestClass
    Dim _test As String

    Public ReadOnly Property Test As $String
        Get
            Return _test
        End Get
    End Property
End Class");
        }

        [Fact]
        public void TestWriteOnlyProperty()
        {
            TestWrongContext<CreateChangedEventCodeRefactoringProvider>(@"
Class TestClass
    Dim _test As String

    Public WriteOnly Property $Test As String
        Set(ByVal value As String)
            _test = value
        End Set
    End Property
End Class");
        }

        [Fact]
        public void TestWrongLocation()
        {
            TestWrongContext<CreateChangedEventCodeRefactoringProvider>(@"
Class TestClass
    Dim _test As String

    Public Property Test As $String
        Get
            Return _test
        End Get
        Set(ByVal value As String)
            _test = value
        End Set
    End Property
End Class");

            TestWrongContext<CreateChangedEventCodeRefactoringProvider>(@"
Class TestClass
    Dim _test As String

    Public Property $FooBar.Test As $String
        Get
            Return _test
        End Get
        Set(ByVal value As String)
            _test = value
        End Set
    End Property
End Class");
        }
    }
}

