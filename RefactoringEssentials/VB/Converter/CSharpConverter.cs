using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using CS = Microsoft.CodeAnalysis.CSharp;
using CSS = Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using RefactoringEssentials.VB.CodeRefactorings;
using RefactoringEssentials.Converter;
using Microsoft.CodeAnalysis.Text;

namespace RefactoringEssentials.VB.Converter
{
    public partial class CSharpConverter
    {
        /* TODO
         * - Expressions
         *   - inline assignment / increment / decrement
         * - Handles clause
         * - Implements clause on members
         * 
         * 
         */
        enum TokenContext
        {
            Global,
            Member,
            Local
        }

        public static VisualBasicSyntaxNode Convert(CS.CSharpSyntaxNode input, SemanticModel semanticModel)
        {
            return input.Accept(new NodesVisitor(semanticModel));
        }

        public static ConversionResult ConvertText(string text, MetadataReference[] references)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (references == null)
                throw new ArgumentNullException(nameof(references));
            var tree = CS.SyntaxFactory.ParseSyntaxTree(SourceText.From(text));
            var compilation = CS.CSharpCompilation.Create("Conversion", new[] { tree }, references);
            try
            {
                return new ConversionResult(Convert((CS.CSharpSyntaxNode)tree.GetRoot(), compilation.GetSemanticModel(tree, true)).NormalizeWhitespace().ToFullString());
            }
            catch (Exception ex)
            {
                return new ConversionResult(ex);
            }
        }

        static SyntaxTokenList ConvertModifiers(SyntaxTokenList modifiers, TokenContext context = TokenContext.Global)
        {
            return SyntaxFactory.TokenList(modifiers.Select(m => SyntaxFactory.Token(ConvertToken(CS.CSharpExtensions.Kind(m), context))));
        }

        static SyntaxToken ConvertModifier(SyntaxToken m, TokenContext context = TokenContext.Global)
        {
            return SyntaxFactory.Token(ConvertToken(CS.CSharpExtensions.Kind(m), context));
        }

        static SeparatedSyntaxList<VariableDeclaratorSyntax> RemodelVariableDeclaration(CSS.VariableDeclarationSyntax declaration, NodesVisitor nodesVisitor)
        {
            var type = (TypeSyntax)declaration.Type.Accept(nodesVisitor);
            var declaratorsWithoutInitializers = new List<CSS.VariableDeclaratorSyntax>();
            var declarators = new List<VariableDeclaratorSyntax>();

            foreach (var v in declaration.Variables)
            {
                if (v.Initializer == null)
                {
                    declaratorsWithoutInitializers.Add(v);
                    continue;
                }
                else
                {
                    declarators.Add(
                        SyntaxFactory.VariableDeclarator(
                            SyntaxFactory.SingletonSeparatedList(ExtractIdentifier(v)),
                            declaration.Type.IsVar ? null : SyntaxFactory.SimpleAsClause(type),
                            SyntaxFactory.EqualsValue((ExpressionSyntax)v.Initializer.Value.Accept(nodesVisitor))
                        )
                    );
                }
            }

            if (declaratorsWithoutInitializers.Count > 0)
            {
                declarators.Insert(0, SyntaxFactory.VariableDeclarator(SyntaxFactory.SeparatedList(declaratorsWithoutInitializers.Select(ExtractIdentifier)), SyntaxFactory.SimpleAsClause(type), null));
            }

            return SyntaxFactory.SeparatedList(declarators);
        }

        static ModifiedIdentifierSyntax ExtractIdentifier(CSS.VariableDeclaratorSyntax v)
        {
            var id = SyntaxFactory.Identifier(v.Identifier.ValueText, SyntaxFacts.IsKeywordKind(v.Identifier.Kind()), v.Identifier.GetIdentifierText(), TypeCharacter.None);
            return SyntaxFactory.ModifiedIdentifier(id);
        }

        static SyntaxKind ConvertToken(CS.SyntaxKind t, TokenContext context = TokenContext.Global)
        {
            switch (t)
            {
                case CS.SyntaxKind.None:
                    return SyntaxKind.None;
                // built-in types
                case CS.SyntaxKind.BoolKeyword:
                    return SyntaxKind.BooleanKeyword;
                case CS.SyntaxKind.ByteKeyword:
                    return SyntaxKind.ByteKeyword;
                case CS.SyntaxKind.SByteKeyword:
                    return SyntaxKind.SByteKeyword;
                case CS.SyntaxKind.ShortKeyword:
                    return SyntaxKind.ShortKeyword;
                case CS.SyntaxKind.UShortKeyword:
                    return SyntaxKind.UShortKeyword;
                case CS.SyntaxKind.IntKeyword:
                    return SyntaxKind.IntegerKeyword;
                case CS.SyntaxKind.UIntKeyword:
                    return SyntaxKind.UIntegerKeyword;
                case CS.SyntaxKind.LongKeyword:
                    return SyntaxKind.LongKeyword;
                case CS.SyntaxKind.ULongKeyword:
                    return SyntaxKind.ULongKeyword;
                case CS.SyntaxKind.DoubleKeyword:
                    return SyntaxKind.DoubleKeyword;
                case CS.SyntaxKind.FloatKeyword:
                    return SyntaxKind.SingleKeyword;
                case CS.SyntaxKind.DecimalKeyword:
                    return SyntaxKind.DecimalKeyword;
                case CS.SyntaxKind.StringKeyword:
                    return SyntaxKind.StringKeyword;
                case CS.SyntaxKind.CharKeyword:
                    return SyntaxKind.CharKeyword;
                case CS.SyntaxKind.VoidKeyword:
                    // not supported
                    break;
                case CS.SyntaxKind.ObjectKeyword:
                    return SyntaxKind.ObjectKeyword;
                // literals
                case CS.SyntaxKind.NullKeyword:
                    return SyntaxKind.NothingKeyword;
                case CS.SyntaxKind.TrueKeyword:
                    return SyntaxKind.TrueKeyword;
                case CS.SyntaxKind.FalseKeyword:
                    return SyntaxKind.FalseKeyword;
                case CS.SyntaxKind.ThisKeyword:
                    return SyntaxKind.MeKeyword;
                case CS.SyntaxKind.BaseKeyword:
                    return SyntaxKind.MyBaseKeyword;
                // modifiers
                case CS.SyntaxKind.PublicKeyword:
                    return SyntaxKind.PublicKeyword;
                case CS.SyntaxKind.PrivateKeyword:
                    return SyntaxKind.PrivateKeyword;
                case CS.SyntaxKind.InternalKeyword:
                    return SyntaxKind.FriendKeyword;
                case CS.SyntaxKind.ProtectedKeyword:
                    return SyntaxKind.ProtectedKeyword;
                case CS.SyntaxKind.StaticKeyword:
                    return SyntaxKind.SharedKeyword;
                case CS.SyntaxKind.ReadOnlyKeyword:
                    break;
                case CS.SyntaxKind.SealedKeyword:
                    if (context == TokenContext.Global)
                        return SyntaxKind.NotInheritableKeyword;
                    else if (context == TokenContext.Member)
                        return SyntaxKind.NotOverridableKeyword;
                    break;
                case CS.SyntaxKind.ConstKeyword:
                    break;
                case CS.SyntaxKind.FixedKeyword:
                    break;
                case CS.SyntaxKind.StackAllocKeyword:
                    break;
                case CS.SyntaxKind.VolatileKeyword:
                    break;
                case CS.SyntaxKind.NewKeyword:
                    break;
                case CS.SyntaxKind.OverrideKeyword:
                    break;
                case CS.SyntaxKind.AbstractKeyword:
                    return context == TokenContext.Global ? SyntaxKind.MustInheritKeyword : SyntaxKind.MustOverrideKeyword;
                case CS.SyntaxKind.VirtualKeyword:
                    break;
                case CS.SyntaxKind.UnsafeKeyword:
                    break;
                case CS.SyntaxKind.ExternKeyword:
                    break;
                case CS.SyntaxKind.RefKeyword:
                    return SyntaxKind.ByRefKeyword;
                case CS.SyntaxKind.OutKeyword:
                    return SyntaxKind.ByRefKeyword;
                case CS.SyntaxKind.InKeyword:
                    break;
                case CS.SyntaxKind.IsKeyword:
                    break;
                case CS.SyntaxKind.AsKeyword:
                    break;
                case CS.SyntaxKind.ParamsKeyword:
                    break;
                case CS.SyntaxKind.ArgListKeyword:
                    break;
                case CS.SyntaxKind.MakeRefKeyword:
                    break;
                case CS.SyntaxKind.RefTypeKeyword:
                    break;
                case CS.SyntaxKind.RefValueKeyword:
                    break;
                case CS.SyntaxKind.OperatorKeyword:
                    break;
                case CS.SyntaxKind.ExplicitKeyword:
                    break;
                case CS.SyntaxKind.ImplicitKeyword:
                    break;
                case CS.SyntaxKind.YieldKeyword:
                    break;
                case CS.SyntaxKind.PartialKeyword:
                    break;
                case CS.SyntaxKind.AliasKeyword:
                    break;
                case CS.SyntaxKind.GlobalKeyword:
                    break;
                case CS.SyntaxKind.AssemblyKeyword:
                    break;
                case CS.SyntaxKind.ModuleKeyword:
                    break;
                case CS.SyntaxKind.TypeKeyword:
                    break;
                case CS.SyntaxKind.FieldKeyword:
                    break;
                case CS.SyntaxKind.MethodKeyword:
                    break;
                case CS.SyntaxKind.ParamKeyword:
                    break;
                case CS.SyntaxKind.PropertyKeyword:
                    break;
                case CS.SyntaxKind.TypeVarKeyword:
                    break;
                case CS.SyntaxKind.GetKeyword:
                    break;
                case CS.SyntaxKind.SetKeyword:
                    break;
                case CS.SyntaxKind.AddKeyword:
                    break;
                case CS.SyntaxKind.RemoveKeyword:
                    break;
                case CS.SyntaxKind.WhereKeyword:
                    break;
                case CS.SyntaxKind.FromKeyword:
                    break;
                case CS.SyntaxKind.GroupKeyword:
                    break;
                case CS.SyntaxKind.JoinKeyword:
                    break;
                case CS.SyntaxKind.IntoKeyword:
                    break;
                case CS.SyntaxKind.LetKeyword:
                    break;
                case CS.SyntaxKind.ByKeyword:
                    break;
                case CS.SyntaxKind.SelectKeyword:
                    break;
                case CS.SyntaxKind.OrderByKeyword:
                    break;
                case CS.SyntaxKind.OnKeyword:
                    break;
                case CS.SyntaxKind.EqualsKeyword:
                    break;
                case CS.SyntaxKind.AscendingKeyword:
                    break;
                case CS.SyntaxKind.DescendingKeyword:
                    break;
                case CS.SyntaxKind.NameOfKeyword:
                    break;
                case CS.SyntaxKind.AsyncKeyword:
                    break;
                case CS.SyntaxKind.AwaitKeyword:
                    break;
                case CS.SyntaxKind.WhenKeyword:
                    break;
                case CS.SyntaxKind.ElifKeyword:
                    break;
                case CS.SyntaxKind.EndIfKeyword:
                    break;
                case CS.SyntaxKind.RegionKeyword:
                    break;
                case CS.SyntaxKind.EndRegionKeyword:
                    break;
                case CS.SyntaxKind.DefineKeyword:
                    break;
                case CS.SyntaxKind.UndefKeyword:
                    break;
                case CS.SyntaxKind.WarningKeyword:
                    break;
                case CS.SyntaxKind.ErrorKeyword:
                    break;
                case CS.SyntaxKind.LineKeyword:
                    break;
                case CS.SyntaxKind.PragmaKeyword:
                    break;
                case CS.SyntaxKind.HiddenKeyword:
                    break;
                case CS.SyntaxKind.ChecksumKeyword:
                    break;
                case CS.SyntaxKind.DisableKeyword:
                    break;
                case CS.SyntaxKind.RestoreKeyword:
                    break;
                case CS.SyntaxKind.ReferenceKeyword:
                    break;
                case CS.SyntaxKind.InterpolatedStringStartToken:
                    break;
                case CS.SyntaxKind.InterpolatedStringEndToken:
                    break;
                case CS.SyntaxKind.InterpolatedVerbatimStringStartToken:
                    break;
                case CS.SyntaxKind.OmittedTypeArgumentToken:
                    break;
                case CS.SyntaxKind.OmittedArraySizeExpressionToken:
                    break;
                case CS.SyntaxKind.EndOfDirectiveToken:
                    break;
                case CS.SyntaxKind.EndOfDocumentationCommentToken:
                    break;
                case CS.SyntaxKind.EndOfFileToken:
                    break;
                case CS.SyntaxKind.BadToken:
                    break;
                case CS.SyntaxKind.IdentifierToken:
                    break;
                case CS.SyntaxKind.NumericLiteralToken:
                    break;
                case CS.SyntaxKind.CharacterLiteralToken:
                    break;
                case CS.SyntaxKind.StringLiteralToken:
                    break;
                case CS.SyntaxKind.XmlEntityLiteralToken:
                    break;
                case CS.SyntaxKind.XmlTextLiteralToken:
                    break;
                case CS.SyntaxKind.XmlTextLiteralNewLineToken:
                    break;
                case CS.SyntaxKind.InterpolatedStringToken:
                    break;
                case CS.SyntaxKind.InterpolatedStringTextToken:
                    break;
                case CS.SyntaxKind.TypeCref:
                    break;
                case CS.SyntaxKind.QualifiedCref:
                    break;
                case CS.SyntaxKind.NameMemberCref:
                    break;
                case CS.SyntaxKind.IndexerMemberCref:
                    break;
                case CS.SyntaxKind.OperatorMemberCref:
                    break;
                case CS.SyntaxKind.ConversionOperatorMemberCref:
                    break;
                case CS.SyntaxKind.CrefParameterList:
                    break;
                case CS.SyntaxKind.CrefBracketedParameterList:
                    break;
                case CS.SyntaxKind.CrefParameter:
                    break;
                case CS.SyntaxKind.IdentifierName:
                    break;
                case CS.SyntaxKind.QualifiedName:
                    break;
                case CS.SyntaxKind.GenericName:
                    break;
                case CS.SyntaxKind.TypeArgumentList:
                    break;
                case CS.SyntaxKind.AliasQualifiedName:
                    break;
                case CS.SyntaxKind.PredefinedType:
                    break;
                case CS.SyntaxKind.ArrayType:
                    break;
                case CS.SyntaxKind.ArrayRankSpecifier:
                    break;
                case CS.SyntaxKind.PointerType:
                    break;
                case CS.SyntaxKind.NullableType:
                    break;
                case CS.SyntaxKind.OmittedTypeArgument:
                    break;
                case CS.SyntaxKind.ParenthesizedExpression:
                    break;
                case CS.SyntaxKind.ConditionalExpression:
                    break;
                case CS.SyntaxKind.InvocationExpression:
                    break;
                case CS.SyntaxKind.ElementAccessExpression:
                    break;
                case CS.SyntaxKind.ArgumentList:
                    break;
                case CS.SyntaxKind.BracketedArgumentList:
                    break;
                case CS.SyntaxKind.Argument:
                    break;
                case CS.SyntaxKind.NameColon:
                    break;
                case CS.SyntaxKind.CastExpression:
                    break;
                case CS.SyntaxKind.AnonymousMethodExpression:
                    break;
                case CS.SyntaxKind.SimpleLambdaExpression:
                    break;
                case CS.SyntaxKind.ParenthesizedLambdaExpression:
                    break;
                case CS.SyntaxKind.ObjectInitializerExpression:
                    break;
                case CS.SyntaxKind.CollectionInitializerExpression:
                    break;
                case CS.SyntaxKind.ArrayInitializerExpression:
                    break;
                case CS.SyntaxKind.AnonymousObjectMemberDeclarator:
                    break;
                case CS.SyntaxKind.ComplexElementInitializerExpression:
                    break;
                case CS.SyntaxKind.ObjectCreationExpression:
                    break;
                case CS.SyntaxKind.AnonymousObjectCreationExpression:
                    break;
                case CS.SyntaxKind.ArrayCreationExpression:
                    break;
                case CS.SyntaxKind.ImplicitArrayCreationExpression:
                    break;
                case CS.SyntaxKind.StackAllocArrayCreationExpression:
                    break;
                case CS.SyntaxKind.OmittedArraySizeExpression:
                    break;
                case CS.SyntaxKind.InterpolatedStringExpression:
                    break;
                case CS.SyntaxKind.ImplicitElementAccess:
                    break;
                case CS.SyntaxKind.AddExpression:
                    break;
                case CS.SyntaxKind.SubtractExpression:
                    break;
                case CS.SyntaxKind.MultiplyExpression:
                    break;
                case CS.SyntaxKind.DivideExpression:
                    break;
                case CS.SyntaxKind.ModuloExpression:
                    break;
                case CS.SyntaxKind.LeftShiftExpression:
                    break;
                case CS.SyntaxKind.RightShiftExpression:
                    break;
                case CS.SyntaxKind.LogicalOrExpression:
                    return SyntaxKind.OrElseExpression;
                case CS.SyntaxKind.LogicalAndExpression:
                    return SyntaxKind.AndAlsoExpression;
                case CS.SyntaxKind.BitwiseOrExpression:
                    return SyntaxKind.OrExpression;
                case CS.SyntaxKind.BitwiseAndExpression:
                    return SyntaxKind.AndExpression;
                case CS.SyntaxKind.ExclusiveOrExpression:
                    return SyntaxKind.ExclusiveOrExpression;
                case CS.SyntaxKind.EqualsExpression:
                    return SyntaxKind.EqualsExpression;
                case CS.SyntaxKind.NotEqualsExpression:
                    return SyntaxKind.NotEqualsExpression;
                case CS.SyntaxKind.LessThanExpression:
                    return SyntaxKind.LessThanExpression;
                case CS.SyntaxKind.LessThanOrEqualExpression:
                    return SyntaxKind.LessThanOrEqualExpression;
                case CS.SyntaxKind.GreaterThanExpression:
                    return SyntaxKind.GreaterThanExpression;
                case CS.SyntaxKind.GreaterThanOrEqualExpression:
                    return SyntaxKind.GreaterThanOrEqualExpression;
                case CS.SyntaxKind.IsExpression:
                    break;
                case CS.SyntaxKind.AsExpression:
                    break;
                case CS.SyntaxKind.CoalesceExpression:
                    break;
                case CS.SyntaxKind.SimpleMemberAccessExpression:
                    break;
                case CS.SyntaxKind.PointerMemberAccessExpression:
                    break;
                case CS.SyntaxKind.ConditionalAccessExpression:
                    break;
                case CS.SyntaxKind.MemberBindingExpression:
                    break;
                case CS.SyntaxKind.ElementBindingExpression:
                    break;
                case CS.SyntaxKind.SimpleAssignmentExpression:
                    return SyntaxKind.SimpleAssignmentStatement;
                case CS.SyntaxKind.AddAssignmentExpression:
                    return SyntaxKind.AddAssignmentStatement;
                case CS.SyntaxKind.SubtractAssignmentExpression:
                    return SyntaxKind.SubtractAssignmentStatement;
                case CS.SyntaxKind.MultiplyAssignmentExpression:
                    return SyntaxKind.MultiplyAssignmentStatement;
                case CS.SyntaxKind.DivideAssignmentExpression:
                    return SyntaxKind.DivideAssignmentStatement;
                case CS.SyntaxKind.ModuloAssignmentExpression:
                    break;
                case CS.SyntaxKind.AndAssignmentExpression:
                    break;
                case CS.SyntaxKind.ExclusiveOrAssignmentExpression:
                    break;
                case CS.SyntaxKind.OrAssignmentExpression:
                    break;
                case CS.SyntaxKind.LeftShiftAssignmentExpression:
                    break;
                case CS.SyntaxKind.RightShiftAssignmentExpression:
                    break;
                case CS.SyntaxKind.UnaryPlusExpression:
                    break;
                case CS.SyntaxKind.UnaryMinusExpression:
                    break;
                case CS.SyntaxKind.BitwiseNotExpression:
                    break;
                case CS.SyntaxKind.LogicalNotExpression:
                    break;
                case CS.SyntaxKind.PreIncrementExpression:
                    return SyntaxKind.AddAssignmentStatement;
                case CS.SyntaxKind.PreDecrementExpression:
                    return SyntaxKind.SubtractAssignmentStatement;
                case CS.SyntaxKind.PointerIndirectionExpression:
                    break;
                case CS.SyntaxKind.AddressOfExpression:
                    break;
                case CS.SyntaxKind.PostIncrementExpression:
                    return SyntaxKind.AddAssignmentStatement;
                case CS.SyntaxKind.PostDecrementExpression:
                    return SyntaxKind.SubtractAssignmentStatement;
                case CS.SyntaxKind.AwaitExpression:
                    break;
                case CS.SyntaxKind.ThisExpression:
                    break;
                case CS.SyntaxKind.BaseExpression:
                    break;
                case CS.SyntaxKind.ArgListExpression:
                    break;
                case CS.SyntaxKind.NumericLiteralExpression:
                    break;
                case CS.SyntaxKind.StringLiteralExpression:
                    break;
                case CS.SyntaxKind.CharacterLiteralExpression:
                    break;
                case CS.SyntaxKind.TrueLiteralExpression:
                    break;
                case CS.SyntaxKind.FalseLiteralExpression:
                    break;
                case CS.SyntaxKind.NullLiteralExpression:
                    break;
                case CS.SyntaxKind.TypeOfExpression:
                    break;
                case CS.SyntaxKind.SizeOfExpression:
                    break;
                case CS.SyntaxKind.CheckedExpression:
                    break;
                case CS.SyntaxKind.UncheckedExpression:
                    break;
                case CS.SyntaxKind.DefaultExpression:
                    break;
                case CS.SyntaxKind.MakeRefExpression:
                    break;
                case CS.SyntaxKind.RefValueExpression:
                    break;
                case CS.SyntaxKind.RefTypeExpression:
                    break;
                case CS.SyntaxKind.QueryExpression:
                    break;
                case CS.SyntaxKind.QueryBody:
                    break;
                case CS.SyntaxKind.FromClause:
                    break;
                case CS.SyntaxKind.LetClause:
                    break;
                case CS.SyntaxKind.JoinClause:
                    break;
                case CS.SyntaxKind.JoinIntoClause:
                    break;
                case CS.SyntaxKind.WhereClause:
                    break;
                case CS.SyntaxKind.OrderByClause:
                    break;
                case CS.SyntaxKind.AscendingOrdering:
                    break;
                case CS.SyntaxKind.DescendingOrdering:
                    break;
                case CS.SyntaxKind.SelectClause:
                    break;
                case CS.SyntaxKind.GroupClause:
                    break;
                case CS.SyntaxKind.QueryContinuation:
                    break;
                case CS.SyntaxKind.Block:
                    break;
                case CS.SyntaxKind.LocalDeclarationStatement:
                    break;
                case CS.SyntaxKind.VariableDeclaration:
                    break;
                case CS.SyntaxKind.VariableDeclarator:
                    break;
                case CS.SyntaxKind.EqualsValueClause:
                    break;
                case CS.SyntaxKind.ExpressionStatement:
                    break;
                case CS.SyntaxKind.EmptyStatement:
                    break;
                case CS.SyntaxKind.LabeledStatement:
                    break;
                case CS.SyntaxKind.GotoStatement:
                    break;
                case CS.SyntaxKind.GotoCaseStatement:
                    break;
                case CS.SyntaxKind.GotoDefaultStatement:
                    break;
                case CS.SyntaxKind.BreakStatement:
                    break;
                case CS.SyntaxKind.ContinueStatement:
                    break;
                case CS.SyntaxKind.ReturnStatement:
                    break;
                case CS.SyntaxKind.YieldReturnStatement:
                    break;
                case CS.SyntaxKind.YieldBreakStatement:
                    break;
                case CS.SyntaxKind.ThrowStatement:
                    break;
                case CS.SyntaxKind.WhileStatement:
                    break;
                case CS.SyntaxKind.DoStatement:
                    break;
                case CS.SyntaxKind.ForStatement:
                    break;
                case CS.SyntaxKind.ForEachStatement:
                    break;
                case CS.SyntaxKind.UsingStatement:
                    break;
                case CS.SyntaxKind.FixedStatement:
                    break;
                case CS.SyntaxKind.CheckedStatement:
                    break;
                case CS.SyntaxKind.UncheckedStatement:
                    break;
                case CS.SyntaxKind.UnsafeStatement:
                    break;
                case CS.SyntaxKind.LockStatement:
                    break;
                case CS.SyntaxKind.IfStatement:
                    break;
                case CS.SyntaxKind.ElseClause:
                    break;
                case CS.SyntaxKind.SwitchStatement:
                    break;
                case CS.SyntaxKind.SwitchSection:
                    break;
                case CS.SyntaxKind.CaseSwitchLabel:
                    break;
                case CS.SyntaxKind.DefaultSwitchLabel:
                    break;
                case CS.SyntaxKind.TryStatement:
                    break;
                case CS.SyntaxKind.CatchClause:
                    break;
                case CS.SyntaxKind.CatchDeclaration:
                    break;
                case CS.SyntaxKind.CatchFilterClause:
                    break;
                case CS.SyntaxKind.FinallyClause:
                    break;
                case CS.SyntaxKind.CompilationUnit:
                    break;
                case CS.SyntaxKind.GlobalStatement:
                    break;
                case CS.SyntaxKind.NamespaceDeclaration:
                    break;
                case CS.SyntaxKind.UsingDirective:
                    break;
                case CS.SyntaxKind.ExternAliasDirective:
                    break;
                case CS.SyntaxKind.AttributeList:
                    break;
                case CS.SyntaxKind.AttributeTargetSpecifier:
                    break;
                case CS.SyntaxKind.Attribute:
                    break;
                case CS.SyntaxKind.AttributeArgumentList:
                    break;
                case CS.SyntaxKind.AttributeArgument:
                    break;
                case CS.SyntaxKind.NameEquals:
                    break;
                case CS.SyntaxKind.ClassDeclaration:
                    break;
                case CS.SyntaxKind.StructDeclaration:
                    break;
                case CS.SyntaxKind.InterfaceDeclaration:
                    break;
                case CS.SyntaxKind.EnumDeclaration:
                    break;
                case CS.SyntaxKind.DelegateDeclaration:
                    break;
                case CS.SyntaxKind.BaseList:
                    break;
                case CS.SyntaxKind.SimpleBaseType:
                    break;
                case CS.SyntaxKind.TypeParameterConstraintClause:
                    break;
                case CS.SyntaxKind.ConstructorConstraint:
                    break;
                case CS.SyntaxKind.ClassConstraint:
                    break;
                case CS.SyntaxKind.StructConstraint:
                    break;
                case CS.SyntaxKind.TypeConstraint:
                    break;
                case CS.SyntaxKind.ExplicitInterfaceSpecifier:
                    break;
                case CS.SyntaxKind.EnumMemberDeclaration:
                    break;
                case CS.SyntaxKind.FieldDeclaration:
                    break;
                case CS.SyntaxKind.EventFieldDeclaration:
                    break;
                case CS.SyntaxKind.MethodDeclaration:
                    break;
                case CS.SyntaxKind.OperatorDeclaration:
                    break;
                case CS.SyntaxKind.ConversionOperatorDeclaration:
                    break;
                case CS.SyntaxKind.ConstructorDeclaration:
                    break;
                case CS.SyntaxKind.BaseConstructorInitializer:
                    break;
                case CS.SyntaxKind.ThisConstructorInitializer:
                    break;
                case CS.SyntaxKind.DestructorDeclaration:
                    break;
                case CS.SyntaxKind.PropertyDeclaration:
                    break;
                case CS.SyntaxKind.EventDeclaration:
                    break;
                case CS.SyntaxKind.IndexerDeclaration:
                    break;
                case CS.SyntaxKind.AccessorList:
                    break;
                case CS.SyntaxKind.GetAccessorDeclaration:
                    break;
                case CS.SyntaxKind.SetAccessorDeclaration:
                    break;
                case CS.SyntaxKind.AddAccessorDeclaration:
                    break;
                case CS.SyntaxKind.RemoveAccessorDeclaration:
                    break;
                case CS.SyntaxKind.UnknownAccessorDeclaration:
                    break;
                case CS.SyntaxKind.ParameterList:
                    break;
                case CS.SyntaxKind.BracketedParameterList:
                    break;
                case CS.SyntaxKind.Parameter:
                    break;
                case CS.SyntaxKind.TypeParameterList:
                    break;
                case CS.SyntaxKind.TypeParameter:
                    break;
                case CS.SyntaxKind.IncompleteMember:
                    break;
                case CS.SyntaxKind.ArrowExpressionClause:
                    break;
                case CS.SyntaxKind.Interpolation:
                    break;
                case CS.SyntaxKind.InterpolatedStringText:
                    break;
                case CS.SyntaxKind.InterpolationAlignmentClause:
                    break;
                case CS.SyntaxKind.InterpolationFormatClause:
                    break;
                case CS.SyntaxKind.PlusPlusToken:
                    return SyntaxKind.PlusToken;
                case CS.SyntaxKind.MinusMinusToken:
                    return SyntaxKind.MinusToken;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            throw new NotSupportedException(t + " is not supported!");
        }
    }
}
