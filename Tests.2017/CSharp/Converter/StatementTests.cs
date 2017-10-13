using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Converter
{
    public class StatementTests : ConverterTestBase
    {

        [Fact]
        public void ObjectInitializationStatement()
        {
            TestConversionVisualBasicToCSharp(
@"Class TestClass
    Private Sub TestMethod()
        Dim b As String
        b = New String(""test"")
    End Sub
End Class",
@"class TestClass
{
    private void TestMethod()
    {
        string b;
        b = new string(""test"");
    }
}");
        }

        [Fact]
        public void ObjectInitializationStatementInDeclarationNoArgs()
        {
            TestConversionVisualBasicToCSharp(@"Class TestClass
    Private Sub TestMethod()
        Dim b As String = New String
    End Sub
End Class", @"
class TestClass
{
    private void TestMethod()
    {
        string b = new string();
    }
}");
        }

        [Fact]
        public void AutoCast()
        {
            TestConversionVisualBasicToCSharp(@"Class TestClass
    Private Sub TestMethod()
        Dim request As HttpWebRequest = WebRequest.Create(uri)
    End Sub
End Class", @"
class TestClass
{
    private void TestMethod()
    {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
    }
}");
        }

        [Fact]
        public void AutoCastPropertySetterValue()
        {
            TestConversionVisualBasicToCSharp(@"
Class TestClass
    Private core_lockno as Long
    Public Overrides Property __primarykey() As Object
        Get
            Return core_lockno
        End Get
        Set(ByVal value As Object)
            core_lockno = value
        End Set
    End Property
End Class", @"
class TestClass
{
    private long core_lockno;

    public override object __primarykey
    {
        get
        {
            return core_lockno;
        }

        set
        {
            core_lockno = (Int64)value;
        }
    }
}");
        }

        [Fact]
        public void IndexCall()
        {
            TestConversionVisualBasicToCSharp(@"Class TestClass
    Private Sub TestMethod()
        Dim d as new Dictionary(of String,String)()
        Dim dectemp as Decimal
        d(""hello"") = ""World""
        dectemp = decimal.Ceiling(50.1)
    End Sub
End Class", @"
class TestClass
{
    private void TestMethod()
    {
        Dictionary<string, string> d = new Dictionary<string, string>();
        decimal dectemp;
        d[""hello""] = ""World"";
        dectemp = decimal.Ceiling(50.1);
    }
}");
        }

        [Fact]
        public void IndexedProperty()
        {
            TestConversionVisualBasicToCSharp(@"
Class TestClass
    Default Public Property Item(columnName as String) as String
        Get
            Return """"
        End Get
    End Property
End Class", @"
class TestClass
{
    public string this[string columnName]
    {
        get
        {
            return """";
        }
    }
}");
        }

        [Fact]
        public void MustOverrideReadOnlyProperty()
        {
            
            TestConversionVisualBasicToCSharp(@"
Class TestClass
    Public MustOverride ReadOnly Property __table() As String
End Class", @"
class TestClass
{
    public abstract string __table { get; }
}");
        }

        [Fact]
        public void IndexCallParameter()
        {

            TestConversionVisualBasicToCSharp(@"
Class TestClass
    Private Sub TestMethod(ByVal dict As Dictionary(Of String, Object))
        Dim value As Object
        Dim field as String = ""ABC""
        value = dict(field)
    End Sub
End Class", @"
class TestClass
{
    private void TestMethod(Dictionary<string, object> dict)
    {
        object value;
        string field = (string)""ABC"";
        value = dict[field];
    }
}");
        }

		[Fact]
		public void DateType()
		{

			TestConversionVisualBasicToCSharp(@"
Class TestClass
    Private Sub TestMethod()
        Dim value As Date
        value = Date.Today()
    End Sub
End Class", @"
class TestClass
{
    private void TestMethod()
    {
        DateTime value;
        value = DateTime.Today();
    }
}");
		}

	}
}
