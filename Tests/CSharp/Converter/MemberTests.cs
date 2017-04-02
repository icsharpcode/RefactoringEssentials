﻿using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Converter
{
    public class MemberTests : ConverterTestBase
    {
        [Fact]
        public void TestField()
        {
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Const answer As Integer = 42
    Private value As Integer = 10
    ReadOnly v As Integer = 15
End Class", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

class TestClass
{
    const int answer = 42;
    private int value = 10;
    readonly int v = 15;
}");
        }

        [Fact]
        public void TestMethod()
        {
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Public Sub TestMethod(Of T As {Class, New}, T2 As Structure, T3)(<Out> ByRef argument As T, ByRef argument2 As T2, ByVal argument3 As T3)
        argument = Nothing
        argument2 = Nothing
        argument3 = Nothing
    End Sub
End Class", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

class TestClass
{
    public void TestMethod<T, T2, T3>(out T argument, ref T2 argument2, T3 argument3)
        where T : class, new()
        where T2 : struct
    {
        argument = null;
        argument2 = default(T2);
        argument3 = default(T3);
    }
}");
        }

        [Fact]
        public void TestMethodWithReturnType()
        {
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Public Function TestMethod(Of T As {Class, New}, T2 As Structure, T3)(<Out> ByRef argument As T, ByRef argument2 As T2, ByVal argument3 As T3) As Integer
        Return 0
    End Function
End Class", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

class TestClass
{
    public int TestMethod<T, T2, T3>(out T argument, ref T2 argument2, T3 argument3)
        where T : class, new()
        where T2 : struct
    {
        return 0;
    }
}");
        }

        [Fact]
        public void TestStaticMethod()
        {
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Public Shared Sub TestMethod(Of T As {Class, New}, T2 As Structure, T3)(<Out> ByRef argument As T, ByRef argument2 As T2, ByVal argument3 As T3)
        argument = Nothing
        argument2 = Nothing
        argument3 = Nothing
    End Sub
End Class", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

class TestClass
{
    public static void TestMethod<T, T2, T3>(out T argument, ref T2 argument2, T3 argument3)
        where T : class, new()
        where T2 : struct
    {
        argument = null;
        argument2 = default(T2);
        argument3 = default(T3);
    }
}");
        }

        [Fact]
        public void TestAbstractMethod()
        {
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Public MustOverride Sub TestMethod()
End Class", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

class TestClass
{
    public abstract void TestMethod();
}");
        }

        [Fact]
        public void TestSealedMethod()
        {
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Public NotOverridable Sub TestMethod(Of T As {Class, New}, T2 As Structure, T3)(<Out> ByRef argument As T, ByRef argument2 As T2, ByVal argument3 As T3)
        argument = Nothing
        argument2 = Nothing
        argument3 = Nothing
    End Sub
End Class", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

class TestClass
{
    public sealed void TestMethod<T, T2, T3>(out T argument, ref T2 argument2, T3 argument3)
        where T : class, new()
        where T2 : struct
    {
        argument = null;
        argument2 = default(T2);
        argument3 = default(T3);
    }
}");
        }

		[Fact(Skip = "Not implemented!")]
		public void TestExtensionMethod()
        {
            TestConversionVisualBasicToCSharp(
@"Imports System.Runtime.CompilerServices

Module TestClass
    <Extension()>
    Sub TestMethod(ByVal str As String)
    End Sub
End Module", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

static class TestClass
{
    public static void TestMethod(this String str)
    {
    }
}");
        }

		[Fact(Skip = "Not implemented!")]
		public void TestExtensionMethodWithExistingImport()
        {
            TestConversionVisualBasicToCSharp(
@"Imports System.Runtime.CompilerServices

Module TestClass
    <Extension()>
    Sub TestMethod(ByVal str As String)
    End Sub
End Module", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;
using System.Runtime.CompilerServices;

static class TestClass
{
    public static void TestMethod(this String str)
    {
    }
}");
        }

        [Fact]
        public void TestProperty()
        {
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Public Property Test As Integer

    Public Property Test2 As Integer
        Get
            Return 0
        End Get
    End Property

    Private m_test3 As Integer

    Public Property Test3 As Integer
        Get
            Return Me.m_test3
        End Get
        Set(ByVal value As Integer)
            Me.m_test3 = value
        End Set
    End Property
End Class", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

class TestClass
{
    public int Test { get; set; }

    public int Test2
    {
        get
        {
            return 0;
        }
    }

    private int m_test3;

    public int Test3
    {
        get
        {
            return this.m_test3;
        }

        set
        {
            this.m_test3 = value;
        }
    }
}");
        }

        [Fact]
        public void TestConstructor()
        {
            TestConversionVisualBasicToCSharp(
@"Class TestClass(Of T As {Class, New}, T2 As Structure, T3)
    Public Sub New(<Out> ByRef argument As T, ByRef argument2 As T2, ByVal argument3 As T3)
    End Sub
End Class", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

class TestClass<T, T2, T3>
    where T : class, new()
    where T2 : struct
{
    public TestClass(out T argument, ref T2 argument2, T3 argument3)
    {
    }
}");
        }

		[Fact]
        public void TestDestructor()
        {
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Protected Overrides Sub Finalize()
    End Sub
End Class", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

class TestClass
{
    ~TestClass()
    {
    }
}");
        }

        [Fact]
        public void TestEvent()
        {
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Public Event MyEvent As EventHandler
End Class", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

class TestClass
{
    public event EventHandler MyEvent;
}");
        }

		[Fact(Skip = "Not implemented!")]
		public void TestCustomEvent()
        {
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Private backingField As EventHandler

    Public Event MyEvent As EventHandler
        AddHandler(ByVal value As EventHandler)
            AddHandler Me.backingField, value
        End AddHandler
        RemoveHandler(ByVal value As EventHandler)
            RemoveHandler Me.backingField, value
        End RemoveHandler
    End Event
End Class", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

class TestClass
{
    EventHandler backingField;

    public event EventHandler MyEvent {
        add {
            this.backingField += value;
        }
        remove {
            this.backingField -= value;
        }
    }
}");
        }

		[Fact(Skip = "Not implemented!")]
		public void TestIndexer()
        {
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Default Public Property Item(ByVal index As Integer) As Integer

    Default Public Property Item(ByVal index As String) As Integer
        Get
            Return 0
        End Get
    End Property

    Private m_test3 As Integer

    Default Public Property Item(ByVal index As Double) As Integer
        Get
            Return Me.m_test3
        End Get
        Set(ByVal value As Integer)
            Me.m_test3 = value
        End Set
    End Property
End Class", @"using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic;

class TestClass
{
    public int this[int index] { get; set; }
    public int this[string index] {
        get { return 0; }
    }
    int m_test3;
    public int this[double index] {
        get { return this.m_test3; }
        set { this.m_test3 = value; }
    }
}");
        }
    }
}