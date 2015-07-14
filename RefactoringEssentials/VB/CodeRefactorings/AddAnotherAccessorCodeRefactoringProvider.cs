using System.Linq;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.Formatting;

namespace RefactoringEssentials.VB.CodeRefactorings
{
    /// <summary>
    /// Add another accessor to a property declaration that has only one.
    /// </summary>
    [ExportCodeRefactoringProvider(LanguageNames.VisualBasic, Name = "Adds another accessor")]
    public class AddAnotherAccessorCodeRefactoringProvider : CodeRefactoringProvider
    {

        public static Microsoft.CodeAnalysis.VisualBasic.Syntax.ThrowStatementSyntax GetNotImplementedThrowStatement()
        {
            return SyntaxFactory.ThrowStatement(SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName(@"System"), SyntaxFactory.IdentifierName(@"NotImplementedException")))
                .WithArgumentList(SyntaxFactory.ArgumentList()));
        }

        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var document = context.Document;
            if (document.Project.Solution.Workspace.Kind == WorkspaceKind.MiscellaneousFiles)
                return;
            var span = context.Span;
            if (!span.IsEmpty)
                return;
            var cancellationToken = context.CancellationToken;
            if (cancellationToken.IsCancellationRequested)
                return;
            var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            if (model.IsFromGeneratedCode(cancellationToken))
                return;
            var root = await model.SyntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var token = root.FindToken(span.Start);
            var propertyDeclaration = token.GetAncestor(a => a is PropertyBlockSyntax) as PropertyBlockSyntax;

            if (propertyDeclaration == null || !propertyDeclaration.Accessors.Any())
                return;
            var accessors = propertyDeclaration.Accessors;

            // Ignore if it has both accessors
            if (accessors.Count == 2)
                return;
            // Ignore interfaces
            if (propertyDeclaration.Parent is InterfaceBlockSyntax)
                return;
            // If it has a getter, then we need a setter (we've checked for 2 accessors)
            bool needsSetter = accessors.Any(m => m.IsKind(SyntaxKind.GetAccessorBlock));

            context.RegisterRefactoring(CodeActionFactory.Create(token.Span, DiagnosticSeverity.Info, GettextCatalog.GetString("Add another accessor"), t2 =>
            {
                return Task.FromResult(PerformAction(document, model, root, propertyDeclaration, needsSetter));
            })
            );
        }

        static IFieldSymbol ScanGetter(SemanticModel model, AccessorBlockSyntax getter)
        {
            if (getter == null || getter.Statements.Count != 1)
                return null;
            var retStatement = getter.Statements.First() as ReturnStatementSyntax;
            if (retStatement == null)
                return null;
            if (!IsPossibleExpression(retStatement.Expression))
                return null;
            var retSymbol = model.GetSymbolInfo(retStatement.Expression).Symbol;
            return ((IFieldSymbol)retSymbol);
        }

        static IFieldSymbol ScanSetter(SemanticModel model, AccessorBlockSyntax setter)
        {
            if (setter == null || setter.Statements.Count != 1) //no getter/get;/get we can't easily work out
                return null;
            var setAssignment = setter.Statements.OfType<StatementSyntax>().FirstOrDefault();
            var assignment = setAssignment != null ? setAssignment as AssignmentStatementSyntax : null;
            if (assignment == null || !assignment.OperatorToken.IsKind(SyntaxKind.EqualsToken))
                return null;
            var id = assignment.Right as IdentifierNameSyntax;
            if (id == null || id.Identifier.ValueText != "value")
                return null;
            if (!IsPossibleExpression(assignment.Left))
                return null;
            var retSymbol = model.GetSymbolInfo(assignment.Left).Symbol;
            return ((IFieldSymbol)retSymbol);

        }

        static bool IsPossibleExpression(ExpressionSyntax left)
        {
            if (left.IsKind(SyntaxKind.IdentifierName))
                return true;
            var mr = left as MemberAccessExpressionSyntax;
            if (mr == null)
                return false;
            return mr.Expression is MyClassExpressionSyntax;
        }

        Document PerformAction(Document document, SemanticModel model, SyntaxNode root, PropertyBlockSyntax propertyDeclaration, bool needsSetter)
        {
            AccessorBlockSyntax accessor = null;
            PropertyBlockSyntax newProp = null;
            if (needsSetter)
            {
                accessor = SyntaxFactory.AccessorBlock(
                    SyntaxKind.SetAccessorBlock,
                    SyntaxFactory.AccessorStatement(SyntaxKind.SetAccessorStatement, SyntaxFactory.Token(SyntaxKind.SetKeyword))
                        .WithParameterList(SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(new[] {
                            SyntaxFactory.Parameter(SyntaxFactory.List<AttributeListSyntax>(), SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ByValKeyword)), SyntaxFactory.ModifiedIdentifier("value"), SyntaxFactory.SimpleAsClause(propertyDeclaration.PropertyStatement.AsClause.Type().WithoutTrailingTrivia()), null)
                        }))),
                    SyntaxFactory.EndBlockStatement(SyntaxKind.EndSetStatement, SyntaxFactory.Token(SyntaxKind.SetKeyword)));

                var getter = propertyDeclaration.Accessors.FirstOrDefault(m => m.IsKind(SyntaxKind.GetAccessorBlock));
                if (getter != null)
                {
                    var getField = ScanGetter(model, getter);
                    /*if (getField == null)
                    {
                        // Get ... End Get
                        accessor = accessor.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)).WithTrailingTrivia(getter.GetTrailingTrivia());
                    }
                    else*/
                    if (getField == null || getField.IsReadOnly)
                    {
                        // Readonly or no field can be found
                        accessor = accessor.WithStatements(SyntaxFactory.List<StatementSyntax>(new[] { GetNotImplementedThrowStatement() }));
                    }
                    else
                    {
                        //now we add a 'field = value'.
                        accessor = accessor.WithStatements(SyntaxFactory.List<StatementSyntax>(new[] {
                                SyntaxFactory.AssignmentStatement(SyntaxKind.SimpleAssignmentStatement, SyntaxFactory.IdentifierName(getField.Name), SyntaxFactory.Token(SyntaxKind.EqualsToken), SyntaxFactory.IdentifierName("value")) }));
                    }
                }
                var modifierList = SyntaxFactory.TokenList(propertyDeclaration.PropertyStatement.Modifiers.Where(m => !m.IsKind(SyntaxKind.ReadOnlyKeyword)));
                newProp = propertyDeclaration
                    .WithAccessors(propertyDeclaration.Accessors.Add(accessor))
                    .WithPropertyStatement(propertyDeclaration.PropertyStatement.WithModifiers(modifierList));
            }
            else
            {
                accessor = SyntaxFactory.AccessorBlock(SyntaxKind.GetAccessorBlock,
                    SyntaxFactory.AccessorStatement(SyntaxKind.GetAccessorStatement, SyntaxFactory.Token(SyntaxKind.GetKeyword)),
                    SyntaxFactory.EndBlockStatement(SyntaxKind.EndGetStatement, SyntaxFactory.Token(SyntaxKind.GetKeyword)));

                var setter = propertyDeclaration.Accessors.FirstOrDefault(m => m.IsKind(SyntaxKind.SetAccessorBlock));
                var accessorDeclList = new SyntaxList<AccessorBlockSyntax>();
                if (setter == null)
                {
                    //set;
                    //accessor = accessor.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                }
                else
                {
                    var setField = ScanSetter(model, setter);
                    if (setField == null)
                    {
                        //    //set;
                        //    //accessor = accessor.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)).WithTrailingTrivia(setter.GetTrailingTrivia());
                        //}
                        //else if (setField == null)
                        //{
                        // No field can be found
                        accessor = accessor.WithStatements(SyntaxFactory.List<StatementSyntax>(new[] { GetNotImplementedThrowStatement() }));
                    }
                    else
                    {
                        // Add a 'Return field'.
                        accessor = accessor.WithStatements(SyntaxFactory.List<StatementSyntax>(
                            new[] { SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(setField.Name)) }));
                    }
                    accessorDeclList = accessorDeclList.Add(propertyDeclaration.Accessors.First(m => m.IsKind(SyntaxKind.SetAccessorBlock)));
                }
                accessorDeclList = accessorDeclList.Insert(0, accessor);
                var accessorList = SyntaxFactory.List(accessorDeclList);
                var modifierList = SyntaxFactory.TokenList(propertyDeclaration.PropertyStatement.Modifiers.Where(m => !m.IsKind(SyntaxKind.WriteOnlyKeyword)));
                newProp = propertyDeclaration
                    .WithAccessors(accessorList)
                    .WithPropertyStatement(propertyDeclaration.PropertyStatement.WithModifiers(modifierList));
            }
            var newRoot = root.ReplaceNode(propertyDeclaration, newProp).WithAdditionalAnnotations(Formatter.Annotation);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}

