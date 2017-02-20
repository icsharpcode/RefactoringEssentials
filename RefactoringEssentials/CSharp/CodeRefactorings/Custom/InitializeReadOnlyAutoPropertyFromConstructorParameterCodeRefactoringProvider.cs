using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CodeRefactorings;
using static RefactoringEssentials.CSharp.Manipulations;

namespace RefactoringEssentials.CSharp.CodeRefactorings
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "Initialize readonly auto property from constructor parameter")]
    public class InitializeReadOnlyAutoPropertyFromConstructorParameterCodeRefactoringProvider : CodeRefactoringProvider
    {
        ConstructorParameterContextFinder ConstructorParameterContextFinder { get; }

        public InitializeReadOnlyAutoPropertyFromConstructorParameterCodeRefactoringProvider()
        {
            ConstructorParameterContextFinder = new ConstructorParameterContextFinder();
        }

        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var constructorParameterContext = await ConstructorParameterContextFinder.Find(context);

            if (constructorParameterContext == null)
                return;

            context.RegisterRefactoring(
                    CodeActionFactory.Create(
                        constructorParameterContext.TextSpan,
                        DiagnosticSeverity.Info,
                        GettextCatalog.GetString("Initialize readonly auto-property from parameter"),
                        t => CreateAndInitialiseReadOnlyAutoPropertyFromConstructorParameter(constructorParameterContext)
                    )
                );
        }

        static Task<Document> CreateAndInitialiseReadOnlyAutoPropertyFromConstructorParameter(ConstructorParameterContext context)
        {
            var trackedRoot = context.Root.TrackNodes(context.Constructor);

            var rootWithNewProperty = AddBefore(
                root: trackedRoot, 
                loationToAddBefore: context.Constructor, 
                nodeToAdd: CreateAutoProperty(
                    type: context.Type, 
                    identifier: context.PropertyName, 
                    accessors: GetAccessor(), 
                    accessibility: SyntaxKind.PublicKeyword));

            var rootWithAssignmentAndProperty = AddStatementToConstructorBody(
                root: rootWithNewProperty, 
                constructor: context.Constructor, 
                statement: CreateAssignmentStatement(
                    leftHandSidePropertyName: context.PropertyName, 
                    rightHandSidePropertyName: context.ParameterName));

            return Task.FromResult(context.Document.WithSyntaxRoot(rootWithAssignmentAndProperty));
        }

    }
}
