using System.Linq;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Convert auto-property to computed property")]
    public class ConvertAutoPropertyToPropertyCodeRefactoringProvider : CodeRefactoringProvider
    {
        readonly PropertyDeclarationContextFinder propertyDeclarationContextFinder;

        public ConvertAutoPropertyToPropertyCodeRefactoringProvider()
        {
            this.propertyDeclarationContextFinder = new PropertyDeclarationContextFinder();
        }

        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var propertyDeclarationContext = await propertyDeclarationContextFinder.Find(context);

            if (propertyDeclarationContext == null) return;

            // Auto properties don't have any accessors
            if (propertyDeclarationContext.Property.AccessorList?.Accessors.Any(b => b.Body != null) != false)
                return;

            context.RegisterRefactoring(
                CodeActionFactory.Create(
                    propertyDeclarationContext.Property.Identifier.Span,
                    DiagnosticSeverity.Info,
                    GettextCatalog.GetString("To computed property"),
                    t2 => new ConvertAutoPropertyToPropertyManipulater(propertyDeclarationContext).Manipulate())
            );
        }
    }
}
