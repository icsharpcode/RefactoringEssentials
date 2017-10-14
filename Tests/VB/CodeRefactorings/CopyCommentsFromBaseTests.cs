using RefactoringEssentials.VB.CodeRefactorings;
using Xunit;

namespace RefactoringEssentials.Tests.VB.CodeRefactorings
{
    public class CopyCommentsFromBaseTest : VBCodeRefactoringTestBase
    {
        [Fact]
        public void TestCopyMethodMultiString()
        {

            Test<CopyCommentsFromBaseCodeRefactoringProvider>(@"
Namespace TestNS
    Class TestClass
        '''<summary>ssss
        '''ssss</summary>
        Public Overridable Sub Test()
            Dim a As Integer
        End Sub
    End Class
    Class DerivedClass
        Inherits TestClass
        Public Overrides Sub $Test()
            Dim str As String = String.Empty
        End Sub
    End Class
End Namespace", @"
Namespace TestNS
    Class TestClass
        '''<summary>ssss
        '''ssss</summary>
        Public Overridable Sub Test()
            Dim a As Integer
        End Sub
    End Class
    Class DerivedClass
        Inherits TestClass
        '''<summary>ssss
        '''ssss</summary>
        Public Overrides Sub Test()
            Dim str As String = String.Empty
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void TestCopyMethodSingleString()
        {

            Test<CopyCommentsFromBaseCodeRefactoringProvider>(@"
Namespace TestNS
    Class TestClass
        '''ssss
        Public Overridable Sub Test()
            Dim a As Integer
        End Sub
    End Class
    Class DerivedClass
        Inherits TestClass
        Public Overrides Sub $Test()
            Dim str As String = String.Empty
        End Sub
    End Class
End Namespace", @"
Namespace TestNS
    Class TestClass
        '''ssss
        Public Overridable Sub Test()
            Dim a As Integer
        End Sub
    End Class
    Class DerivedClass
        Inherits TestClass
        '''ssss
        Public Overrides Sub Test()
            Dim str As String = String.Empty
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void TestCopyMethodAbstractClassString()
        {

            Test<CopyCommentsFromBaseCodeRefactoringProvider>(@"
Namespace TestNS
    MustInherit Class TestClass
        '''ssss
        '''ssss
        Public MustOverride Sub Test()
    End Class
    MustInherit Class DerivedClass
        Inherits TestClass
        Public Overrides Sub $Test()
            Dim str As String = String.Empty
        End Sub
    End Class
End Namespace", @"
Namespace TestNS
    MustInherit Class TestClass
        '''ssss
        '''ssss
        Public MustOverride Sub Test()
    End Class
    MustInherit Class DerivedClass
        Inherits TestClass
        '''ssss
        '''ssss
        Public Overrides Sub Test()
            Dim str As String = String.Empty
        End Sub
    End Class
End Namespace");
        }

        [Fact]
        public void TestCopyProperty()
        {

            Test<CopyCommentsFromBaseCodeRefactoringProvider>(@"
Namespace TestNS
    Class TestClass
        '''<summary>
        '''FooBar
        '''</summary>
        Public Overridable Property Test As Integer
            Get
                Return 0
            End Get
            Set(ByVal value As Integer)
            End Set
        End Property
    End Class
    Class DerivedClass
        Inherits TestClass
        Public Overrides Property $Test As Integer
            Get
                Return 0
            End Get
            Set(ByVal value As Integer)
            End Set
        End Property
    End Class
End Namespace", @"
Namespace TestNS
    Class TestClass
        '''<summary>
        '''FooBar
        '''</summary>
        Public Overridable Property Test As Integer
            Get
                Return 0
            End Get
            Set(ByVal value As Integer)
            End Set
        End Property
    End Class
    Class DerivedClass
        Inherits TestClass
        '''<summary>
        '''FooBar
        '''</summary>
        Public Overrides Property Test As Integer
            Get
                Return 0
            End Get
            Set(ByVal value As Integer)
            End Set
        End Property
    End Class
End Namespace");
        }

        [Fact]
        public void TestCopyType()
        {

            Test<CopyCommentsFromBaseCodeRefactoringProvider>(@"
'''<summary>
'''FooBar
'''</summary>
Class Base 
End Class

Class $TestClass
    Inherits Base
End Class", @"
'''<summary>
'''FooBar
'''</summary>
Class Base 
End Class

'''<summary>
'''FooBar
'''</summary>
Class TestClass
    Inherits Base
End Class");
        }

        [Fact]
        public void TestSkipExisting()
        {
            TestWrongContext<CopyCommentsFromBaseCodeRefactoringProvider>(@"
'''<summary>
'''FooBar
'''</summary>
Class Base 
End Class

'''<summary>
'''FooBar
'''</summary>
Class $TestClass
    Inherits Base
End Class
");
        }

        [Fact]
        public void TestSkipEmpty()
        {
            TestWrongContext<CopyCommentsFromBaseCodeRefactoringProvider>(@"
Class Base 
End Class

Class $TestClass
    Inherits Base
End Class
");
        }

        [Fact]
        public void TestInterfaceSimpleCase()
        {
            Test<CopyCommentsFromBaseCodeRefactoringProvider>(@"
Interface ITest
    '''ssss
    Sub Test()
End Interface
Class DerivedClass
    Implements ITest
    Public Sub $Test() Implements ITest.Test
        Dim str As String = String.Empty
    End Sub
End Class", @"
Interface ITest
    '''ssss
    Sub Test()
End Interface
Class DerivedClass
    Implements ITest
    '''ssss
    Public Sub Test() Implements ITest.Test
        Dim str As String = String.Empty
    End Sub
End Class");
        }

        [Fact]
        public void TestInterfaceMultiCase()
        {
            Test<CopyCommentsFromBaseCodeRefactoringProvider>(@"
Interface ITest
    '''ssss
    '''ssss
    Sub Test()
End Interface
Class DerivedClass
    Implements ITest
    Public Sub $Test() Implements ITest.Test
        Dim str As String = String.Empty
    End Sub
End Class", @"
Interface ITest
    '''ssss
    '''ssss
    Sub Test()
End Interface
Class DerivedClass
    Implements ITest
    '''ssss
    '''ssss
    Public Sub Test() Implements ITest.Test
        Dim str As String = String.Empty
    End Sub
End Class");
        }

        [Fact]
        public void TestInterfaceNoProblem()
        {
            TestWrongContext<CopyCommentsFromBaseCodeRefactoringProvider>(@"
Interface ITest
    Sub Test()
End Interface
Class DerivedClass
    Implements ITest
    Public Sub $Test() Implements ITest.Test
        Dim str As String = String.Empty
    End Sub
End Class");
        }
    }
}