using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace RefactoringEssentials.Tests.CSharp.Converter
{
    public class NamespaceLevelTests : ConverterTestBase
    {
        [Fact]
        public void GenericClassNewConstraint()
        {
            TestConversionVisualBasicToCSharp(
@"Class TestClass(of T as {new,object})
End Class",
@"
class TestClass<T> where T : object, new()
{
}
");
        }

        [Fact]
        public void Testinterface()
        {
            TestConversionVisualBasicToCSharp(
@"
Public Interface IProblem
    Function __table() As String

    Sub _FromSQLReader(reader As IDataReader, colidx As Dictionary(Of String, Integer))

    'Function 
    Default Property Item(colname As String) As Object
End Interface
",
@"
public interface IProblem
{
    string __table();
    void _FromSQLReader(IDataReader reader, Dictionary<string, int> colidx);

    object this[string colname] { get; set; }
}
");
        }

        [Fact]
        public void TestBlankInterfaceWithComment()
        {
            TestConversionVisualBasicToCSharp(
@"
Public Interface IProblem
End Interface
",
@"
public interface IProblem
{
}
");
        }

        [Fact]
        public void InterfaceChangedMethodName()
        {

            TestConversionVisualBasicToCSharp(@"
Class TestClass
    Implements INotifyPropertyChanged

    Public Event PropertyChangedCHANGED(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
End Class", @"
class TestClass : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
}");
        }


    }
}
