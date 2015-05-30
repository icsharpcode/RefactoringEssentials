using System.Collections.Generic;
using System;

namespace ICSharpCode.NRefactory6.CSharp
{
    static class RefactoringExtensions
    {
        /// <summary>
        /// Gets the local variable declaration space, as defined by §3.3 Declarations.
        /// </summary>
        /// <returns>The local variable declaration space.</returns>
        public static AstNode GetLocalVariableDeclarationSpace(this AstNode self)
        {
            var node = self.Parent;
            while (node != null && !CreatesLocalVariableDeclarationSpace(node))
                node = node.Parent;
            return node;
        }

        static ISet<Type> localVariableDeclarationSpaceCreators = new HashSet<Type>() {
            typeof(BlockStatement),
            typeof(SwitchStatement),
            typeof(ForeachStatement),
            typeof(ForStatement),
            typeof(UsingStatement),
            typeof(LambdaExpression),
            typeof(AnonymousMethodExpression)
        };

        static bool CreatesLocalVariableDeclarationSpace(AstNode node)
        {
            return localVariableDeclarationSpaceCreators.Contains(node.GetType());
        }
    }
}

