using System.Collections.Generic;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    public static class DefaultRules
    {
        public static IEnumerable<NamingRule> GetFdgRules()
        {
            yield return new NamingRule(AffectedEntity.Namespace)
            {
                Name = "Namespaces",
                NamingStyle = NamingStyle.PascalCase
            };

            yield return new NamingRule(AffectedEntity.Class | AffectedEntity.Struct | AffectedEntity.Enum | AffectedEntity.Delegate)
            {
                Name = "Types",
                VisibilityMask = Modifiers.Public,
                NamingStyle = NamingStyle.PascalCase
            };

            yield return new NamingRule(AffectedEntity.Interface)
            {
                Name = "Interfaces",
                NamingStyle = NamingStyle.PascalCase,
                VisibilityMask = Modifiers.Public,
                RequiredPrefixes = new[] { "I" }
            };

            yield return new NamingRule(AffectedEntity.CustomAttributes)
            {
                Name = "Attributes",
                NamingStyle = NamingStyle.PascalCase,
                VisibilityMask = Modifiers.Public,
                RequiredSuffixes = new[] { "Attribute" }
            };

            yield return new NamingRule(AffectedEntity.CustomEventArgs)
            {
                Name = "Event Arguments",
                NamingStyle = NamingStyle.PascalCase,
                VisibilityMask = Modifiers.Public,
                RequiredSuffixes = new[] { "EventArgs" }
            };

            yield return new NamingRule(AffectedEntity.CustomExceptions)
            {
                Name = "Exceptions",
                NamingStyle = NamingStyle.PascalCase,
                RequiredSuffixes = new[] { "Exception" }
            };

            yield return new NamingRule(AffectedEntity.Methods)
            {
                Name = "Methods",
                VisibilityMask = Modifiers.Public | Modifiers.Protected,
                NamingStyle = NamingStyle.PascalCase
            };

            yield return new NamingRule(AffectedEntity.ReadonlyField)
            {
                Name = "Static Readonly Fields",
                VisibilityMask = Modifiers.Public | Modifiers.Protected,
                NamingStyle = NamingStyle.PascalCase,
                IncludeInstanceMembers = false
            };

            yield return new NamingRule(AffectedEntity.Field)
            {
                Name = "Fields",
                NamingStyle = NamingStyle.PascalCase,
                VisibilityMask = Modifiers.Public | Modifiers.Protected
            };

            yield return new NamingRule(AffectedEntity.ReadonlyField)
            {
                Name = "ReadOnly Fields",
                NamingStyle = NamingStyle.PascalCase,
                VisibilityMask = Modifiers.Public | Modifiers.Protected,
                IncludeStaticEntities = false
            };

            yield return new NamingRule(AffectedEntity.ConstantField)
            {
                Name = "Constant Fields",
                NamingStyle = NamingStyle.PascalCase,
                VisibilityMask = Modifiers.Public | Modifiers.Protected
            };

            yield return new NamingRule(AffectedEntity.Property)
            {
                Name = "Properties",
                VisibilityMask = Modifiers.Public | Modifiers.Protected,
                NamingStyle = NamingStyle.PascalCase
            };

            yield return new NamingRule(AffectedEntity.Event)
            {
                Name = "Events",
                VisibilityMask = Modifiers.Public | Modifiers.Protected,
                NamingStyle = NamingStyle.PascalCase
            };

            yield return new NamingRule(AffectedEntity.EnumMember)
            {
                Name = "Enum Members",
                NamingStyle = NamingStyle.PascalCase
            };

            yield return new NamingRule(AffectedEntity.Parameter)
            {
                Name = "Parameters",
                NamingStyle = NamingStyle.CamelCase
            };

            yield return new NamingRule(AffectedEntity.TypeParameter)
            {
                Name = "Type Parameters",
                NamingStyle = NamingStyle.PascalCase,
                RequiredPrefixes = new[] { "T" }
            };
        }
    }
}

