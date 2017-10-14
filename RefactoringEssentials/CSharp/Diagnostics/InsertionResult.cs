using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;

namespace RefactoringEssentials
{
    public sealed class InsertionResult
    {
        /// <summary>
        /// Gets the context the insertion is invoked at.
        /// </summary>
        public CodeRefactoringContext Context { get; private set; }

        /// <summary>
        /// Gets the node that should be inserted.
        /// </summary>
        public SyntaxNode Node { get; private set; }

        /// <summary>
        /// Gets the type the node should be inserted to.
        /// </summary>
        public INamedTypeSymbol Type { get; private set; }

        /// <summary>
        /// Gets the location of the type part the node should be inserted to.
        /// </summary>
        public Location Location { get; private set; }

        public InsertionResult(CodeRefactoringContext context, SyntaxNode node, INamedTypeSymbol type, Location location)
        {
            this.Context = context;
            this.Node = node;
            this.Type = type;
            this.Location = location;
        }

        public static Location GuessCorrectLocation(CodeRefactoringContext context, System.Collections.Immutable.ImmutableArray<Location> locations)
        {
            foreach (var loc in locations)
            {
                if (context.Document.FilePath == loc.SourceTree.FilePath)
                    return loc;
            }
            return locations[0];
        }
    }
}

