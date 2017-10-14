/*
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MemberCanBeMadeStaticAnalyzer : DiagnosticAnalyzer
    {
        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
            CSharpDiagnosticIDs.MemberCanBeMadeStaticAnalyzerID,
            GettextCatalog.GetString("A member doesn't use 'this' object neither explicit nor implicit. It can be made static"),
            GettextCatalog.GetString("Member can be made static"),
            DiagnosticAnalyzerCategories.PracticesAndImprovements,
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            helpLinkUri: HelpLink.CreateFor(CSharpDiagnosticIDs.MemberCanBeMadeStaticAnalyzerID)
        );

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(descriptor);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction(
                  (nodeContext) =>
                    {
                        Diagnostic diagnostic;
                        if (TryGetDiagnosticMethodDeclaration(nodeContext, out diagnostic))
                        {
                            nodeContext.ReportDiagnostic(diagnostic);
                        }
                    },
                     SyntaxKind.MethodDeclaration
                );

                context.RegisterSyntaxNodeAction(
                    (nodeContext) =>
                    {
                        Diagnostic diagnostic;
                        if (TryGetDiagnosticPropertyDeclaration(nodeContext, out diagnostic))
                        {
                            nodeContext.ReportDiagnostic(diagnostic);
                        }
                    },
                        SyntaxKind.PropertyDeclaration
                );
                        context.RegisterSyntaxNodeAction(
                (nodeContext) =>
                {
                    Diagnostic diagnostic;
                    if (TryGetDiagnosticEventFieldDeclaration(nodeContext, out diagnostic))
                    {
                        nodeContext.ReportDiagnostic(diagnostic);
                    }
                },
                    SyntaxKind.EventDeclaration
            );
        }

        private static bool TryGetDiagnosticMethodDeclaration(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            var methodDeclaration = nodeContext.Node as MethodDeclarationSyntax;
            if (methodDeclaration == null)
                return false;

            if (methodDeclaration.AttributeLists.Any() &&
                DoesAttributeListContainObsoleteAttribute(methodDeclaration.AttributeLists.FirstOrDefault()))
                return false;

            if (DoesMethodContainModifier(methodDeclaration))
                return false;

            var body = methodDeclaration.Body;
            // skip empty methods
            if (body != null && body.Statements.Count == 0)
                return false;

            if (body.Statements.Count == 1)
            {
                if (body.Statements.First() is ThrowStatementSyntax)
                    return false;
            }

            var methodSymbolInfo = nodeContext.SemanticModel.GetDeclaredSymbol(methodDeclaration);
            if (methodSymbolInfo == null)
                return false;

            var isMethodImplementingInterface = methodSymbolInfo.ExplicitInterfaceImplementations();
            if (!isMethodImplementingInterface.IsEmpty)
                return false;

            diagnostic = Diagnostic.Create(descriptor, methodDeclaration.Identifier.GetLocation());
            return true;
        }

        private static bool TryGetDiagnosticPropertyDeclaration(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;
            var propertyDeclaration = nodeContext.Node as PropertyDeclarationSyntax;
            if (propertyDeclaration == null)
                return false;

            if (DoesPropertyContainModifier(propertyDeclaration))
                return false;

            var propSymbolInfo = nodeContext.SemanticModel.GetDeclaredSymbol(propertyDeclaration);
            if (propSymbolInfo == null)
                return false;

            var isPropertyImplementingInterface = propSymbolInfo.ExplicitInterfaceImplementations();
            if (!isPropertyImplementingInterface.IsEmpty)
                return false;

            foreach (var accessor in propertyDeclaration.AccessorList.Accessors) {
                if (IsEmpty(accessor))
                    return false;
                
            }

            diagnostic = Diagnostic.Create(descriptor, propertyDeclaration.Identifier.GetLocation());
            return true;
        }

        private static bool TryGetDiagnosticEventFieldDeclaration(SyntaxNodeAnalysisContext nodeContext, out Diagnostic diagnostic)
        {
            diagnostic = default(Diagnostic);
            if (nodeContext.IsFromGeneratedCode())
                return false;

            var eventField = nodeContext.Node as EventDeclarationSyntax;
            if (eventField == null)
                return false;

            if (DoesEventFieldContainModifier(eventField))
                return false;

            var addAccessor = eventField.AccessorList.Accessors.FirstOrDefault();
            if (addAccessor == null)
                return false;

            var removeAccessor = eventField.AccessorList.Accessors.ElementAt(1);
            if (removeAccessor == null)
                return false;

            if (IsEmpty(addAccessor) && IsEmpty(removeAccessor))
                return false;

            var eventSymbolInfo = nodeContext.SemanticModel.GetDeclaredSymbol(eventField);
            if (eventSymbolInfo == null)
                return false;

            var isPropertyImplementingInterface = eventSymbolInfo.ExplicitInterfaceImplementations();
            if (!isPropertyImplementingInterface.IsEmpty)
                return false;

            diagnostic = Diagnostic.Create(descriptor, eventField.Identifier.GetLocation());
            return true;
        }

        private static bool DoesAttributeListContainObsoleteAttribute(AttributeListSyntax als)
        {
           return als != null && als.Attributes.Any(x => x.Name.ToString() == "Obsolete");
        }

        private static bool DoesMethodContainModifier(MethodDeclarationSyntax methodDeclaration)
        {
            return
                methodDeclaration.Modifiers.Count != 0 && (
                methodDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword) ||
                methodDeclaration.Modifiers.Any(SyntaxKind.VirtualKeyword) ||
                methodDeclaration.Modifiers.Any(SyntaxKind.OverrideKeyword) ||
                methodDeclaration.Modifiers.Any(SyntaxKind.NewKeyword) || methodDeclaration.AttributeLists.Count > 0 &&
                methodDeclaration.AttributeLists.FirstOrDefault().Attributes.Any());
        }

        private static bool DoesPropertyContainModifier(PropertyDeclarationSyntax propertyDeclaration)
        {
            return
                propertyDeclaration.Modifiers.Count != 0 && !(
                propertyDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword) ||
                propertyDeclaration.Modifiers.Any(SyntaxKind.VirtualKeyword) ||
                propertyDeclaration.Modifiers.Any(SyntaxKind.OverrideKeyword) ||
                propertyDeclaration.Modifiers.Any(SyntaxKind.NewKeyword) ||
                propertyDeclaration.AttributeLists.FirstOrDefault().Attributes.Any());
        }

        private static bool DoesEventFieldContainModifier(EventDeclarationSyntax eventFieldDeclaration)
        {
            return
                eventFieldDeclaration.Modifiers.Count != 0 && !(
                eventFieldDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword) ||
                eventFieldDeclaration.Modifiers.Any(SyntaxKind.VirtualKeyword) ||
                eventFieldDeclaration.Modifiers.Any(SyntaxKind.OverrideKeyword) ||
                eventFieldDeclaration.Modifiers.Any(SyntaxKind.NewKeyword) ||
                eventFieldDeclaration.AttributeLists.FirstOrDefault().Attributes.Any());
        }

        static bool IsEmpty(AccessorDeclarationSyntax accessor)
        {
            return
                accessor == null ||
                accessor.Body == null || 
                !accessor.Body.Statements.Any() ||
                accessor.Body.Statements.Count == 1 && accessor.Body.Statements.First() is ThrowStatementSyntax;
        }

        //		private class GatherVisitor : GatherVisitorBase<MemberCanBeMadeStaticAnalyzer>
        //		{
        ////			bool CheckPrivateMember {
        ////				get {
        ////					return SubIssue == CommonSubIssues.PrivateMember;
        ////				}
        ////			}

        //			public GatherVisitor(SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, CancellationToken cancellationToken)
        //				: base (semanticModel, addDiagnostic, cancellationToken)
        //			{
        //			}

        ////			public override void VisitTypeDeclaration(TypeDeclaration typeDeclaration)
        ////			{
        ////				var rr = ctx.Resolve(typeDeclaration);
        ////                if (rr.Type.GetNonInterfaceBaseTypes().Any(t => t.Name == "MarshalByRefObject" && t.Namespace == "System"))
        ////                    return; // ignore MarshalByRefObject, as only instance method calls get marshaled
        ////
        ////				base.VisitTypeDeclaration(typeDeclaration);
        ////			}
        ////
        ////			bool SkipMember(IMember resolved)
        ////			{
        ////				return resolved.Accessibility == Accessibility.Private && !CheckPrivateMember ||
        ////					resolved.Accessibility != Accessibility.Private && CheckPrivateMember;
        ////			}
        ////
        ////			public override void VisitMethodDeclaration(MethodDeclaration methodDeclaration)
        ////			{
        ////				var context = ctx;
        ////				if (methodDeclaration.HasModifier(Modifiers.Static) ||
        ////				    methodDeclaration.HasModifier(Modifiers.Virtual) ||
        ////				    methodDeclaration.HasModifier(Modifiers.Override) ||
        ////				    methodDeclaration.HasModifier(Modifiers.New) ||
        ////				    methodDeclaration.Attributes.Any())
        ////					return;
        ////
        ////				var body = methodDeclaration.Body;
        ////				// skip empty methods
        ////				if (!body.Statements.Any())
        ////					return;
        ////
        ////				if (body.Statements.Count == 1) {
        ////					if (body.Statements.First () is ThrowStatement)
        ////						return;
        ////				}
        ////
        ////				var resolved = context.Resolve(methodDeclaration) as MemberResolveResult;
        ////				if (resolved == null || SkipMember(resolved.Member))
        ////					return;
        ////				var isImplementingInterface = resolved.Member.ImplementedInterfaceMembers.Any();
        ////				if (isImplementingInterface)
        ////					return;
        ////
        ////				if (StaticVisitor.UsesNotStaticMember(context, body))
        ////					return;
        ////
        ////				AddDiagnosticAnalyzer(new CodeIssue(
        ////					methodDeclaration.NameToken.StartLocation, methodDeclaration.NameToken.EndLocation,
        ////					string.Format(context.TranslateString("Method '{0}' can be made static."), methodDeclaration.Name),
        ////					string.Format(context.TranslateString("Make '{0}' static"), methodDeclaration.Name),
        ////					script => script.ChangeModifier(methodDeclaration, methodDeclaration.Modifiers | Modifiers.Static)) { IssueMarker = IssueMarker.DottedLine });
        ////			}
        ////
        ////			static bool IsEmpty(Accessor setter)
        ////			{
        ////				return setter.IsNull ||
        ////					!setter.Body.Statements.Any() ||
        ////					setter.Body.Statements.Count == 1 && setter.Body.Statements.First () is ThrowStatement;
        ////			}
        ////
        ////			public override void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
        ////			{
        ////				if (propertyDeclaration.HasModifier(Modifiers.Static) ||
        ////				    propertyDeclaration.HasModifier(Modifiers.Virtual) ||
        ////				    propertyDeclaration.HasModifier(Modifiers.Override) ||
        ////				    propertyDeclaration.HasModifier(Modifiers.New) ||
        ////				    propertyDeclaration.Attributes.Any())
        ////					return;
        ////
        ////				if (IsEmpty(propertyDeclaration.Setter) && IsEmpty(propertyDeclaration.Getter))
        ////					return;
        ////
        ////
        ////				var resolved = ctx.Resolve(propertyDeclaration) as MemberResolveResult;
        ////				if (resolved == null || SkipMember(resolved.Member))
        ////					return;
        ////				var isImplementingInterface = resolved.Member.ImplementedInterfaceMembers.Any();
        ////				if (isImplementingInterface)
        ////					return;
        ////
        ////				if (!propertyDeclaration.Getter.IsNull && StaticVisitor.UsesNotStaticMember(ctx, propertyDeclaration.Getter.Body) ||
        ////				    !propertyDeclaration.Setter.IsNull && StaticVisitor.UsesNotStaticMember(ctx, propertyDeclaration.Setter.Body))
        ////					return;
        ////
        ////				AddDiagnosticAnalyzer(new CodeIssue(propertyDeclaration.NameToken.StartLocation, propertyDeclaration.NameToken.EndLocation,
        //			//				         string.Format(ctx.TranslateString("Property '{0}' can be made static."), propertyDeclaration.Name),
        ////				         string.Format(ctx.TranslateString("Make '{0}' static"), propertyDeclaration.Name),
        ////					script => script.ChangeModifier(propertyDeclaration, propertyDeclaration.Modifiers | Modifiers.Static)) { IssueMarker = IssueMarker.DottedLine });
        ////			}
        ////
        ////			public override void VisitCustomEventDeclaration(CustomEventDeclaration eventDeclaration)
        ////			{
        ////				if (eventDeclaration.HasModifier(Modifiers.Static) ||
        ////				    eventDeclaration.HasModifier(Modifiers.Virtual) ||
        ////				    eventDeclaration.HasModifier(Modifiers.Override) ||
        ////				    eventDeclaration.HasModifier(Modifiers.New) ||
        ////				    eventDeclaration.Attributes.Any())
        ////					return;
        ////
        ////				if (IsEmpty(eventDeclaration.RemoveAccessor) && IsEmpty(eventDeclaration.AddAccessor))
        ////					return;
        ////
        ////
        ////				var resolved = ctx.Resolve(eventDeclaration) as MemberResolveResult;
        ////				if (resolved == null || SkipMember(resolved.Member))
        ////					return;
        ////				var isImplementingInterface = resolved.Member.ImplementedInterfaceMembers.Any();
        ////				if (isImplementingInterface)
        ////					return;
        ////
        ////				if (!eventDeclaration.AddAccessor.IsNull && StaticVisitor.UsesNotStaticMember(ctx, eventDeclaration.AddAccessor.Body) ||
        ////				    !eventDeclaration.RemoveAccessor.IsNull && StaticVisitor.UsesNotStaticMember(ctx, eventDeclaration.RemoveAccessor.Body))
        ////					return;
        ////
        ////				AddDiagnosticAnalyzer(new CodeIssue(eventDeclaration.NameToken.StartLocation, eventDeclaration.NameToken.EndLocation,
        ////				         string.Format(ctx.TranslateString("Event '{0}' can be made static."), eventDeclaration.Name),
        ////				         string.Format(ctx.TranslateString("Make '{0}' static"), eventDeclaration.Name),
        ////					script => script.ChangeModifier(eventDeclaration, eventDeclaration.Modifiers | Modifiers.Static)) { IssueMarker = IssueMarker.DottedLine });
        ////			}
        ////
        ////
        ////			static void DoStaticMethodGlobalOperation(AstNode fnode, SemanticModel fctx, MemberResolveResult rr,
        ////			                                                           Script fscript)
        ////			{
        ////				if (fnode is MemberReferenceExpression) {
        ////					var memberReference = fctx.CreateShortType(rr.Member.DeclaringType).Member(rr.Member.Name);
        ////					fscript.Replace(fnode, memberReference);
        ////				} else {
        ////					var invoke = fnode as InvocationExpression;
        ////					if (invoke == null)
        ////						return;
        ////					if ((invoke.Target is MemberReferenceExpression))
        ////						return;
        ////					var memberReference = fctx.CreateShortType(rr.Member.DeclaringType).Member(rr.Member.Name);
        ////					fscript.Replace(invoke.Target, memberReference);
        ////				}
        ////			}
        //		}
    }
}*/