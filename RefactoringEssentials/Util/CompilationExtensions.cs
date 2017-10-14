using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using System.Threading;
using System.Linq;

namespace RefactoringEssentials
{
#if NR6
	public
#endif
    static class CompilationExtensions
    {
        //public static IEnumerable<INamedTypeSymbol> GetAllTypesInMainAssembly(this Compilation compilation, CancellationToken cancellationToken = default(CancellationToken))
        //{
        //	if (compilation == null)
        //		throw new ArgumentNullException("compilation");
        //	return compilation.Assembly.GlobalNamespace.GetAllTypes(cancellationToken);
        //}

        static INamespaceSymbol FindNamespace(INamespaceSymbol globalNamespace, string fullName, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(fullName))
                return globalNamespace;
            var stack = new Stack<INamespaceSymbol>();
            stack.Push(globalNamespace);

            while (stack.Count > 0)
            {
                if (cancellationToken.IsCancellationRequested)
                    return null;
                var currentNs = stack.Pop();
                if (currentNs.GetFullName() == fullName)
                    return currentNs;
                foreach (var childNamespace in currentNs.GetNamespaceMembers())
                    stack.Push(childNamespace);
            }
            return null;
        }

        public static ITypeSymbol GetTypeSymbol(this Compilation compilation, string ns, string name, int arity, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (compilation == null)
                throw new ArgumentNullException("compilation");
            var nsSymbol = FindNamespace(compilation.GlobalNamespace, ns, cancellationToken);
            if (nsSymbol == null)
                return null;
            return nsSymbol.GetTypeMembers(name, arity).FirstOrDefault();
        }
    }
}

