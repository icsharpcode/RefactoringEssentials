using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static RefactoringEssentials.CSharp.Manipulations;
using RefactoringEssentials.CSharp;
using System.Diagnostics.Contracts;

internal class ConvertAutoPropertyToPropertyManipulater : DocumentManipulator
{
    readonly PropertyDeclarationSyntax property;

    public ConvertAutoPropertyToPropertyManipulater(PropertyDeclarationContext context) : base(context.Document, context.Root)
    {
        Contract.Requires(context != null);
        Contract.Requires(context.Property != null);

        this.property = context.Property;
    }

    public Task<Document> Manipulate()
    {
        var newProperty = PropertyWithReplacedAccessors(
            property,
            GetAccessor(ThrowNotImplementedExceptionBlock),
            SetAccessor(ThrowNotImplementedExceptionBlock));

        return DocumentWithReplacedNode(property, newProperty);
    }

}