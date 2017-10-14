using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    public abstract class NamingConventionService
    {
        public abstract IEnumerable<NamingRule> Rules
        {
            get;
        }

        public string CheckName(SemanticModel ctx, string name, AffectedEntity entity, Modifiers accessibilty = Modifiers.Private, bool isStatic = false)
        {
            foreach (var rule in Rules)
            {
                if (!rule.AffectedEntity.HasFlag(entity))
                {
                    continue;
                }
                if (!rule.VisibilityMask.HasFlag(accessibilty))
                {
                    continue;
                }
                if (isStatic && !rule.IncludeStaticEntities || !isStatic && !rule.IncludeInstanceMembers)
                {
                    continue;
                }
                if (!rule.IsValid(name))
                {
                    IList<string> suggestedNames;
                    rule.GetErrorMessage(ctx, name, out suggestedNames);
                    if (suggestedNames.Any())
                        return suggestedNames[0];
                }
            }
            return name;
        }

        public bool IsValidName(string name, AffectedEntity entity, Modifiers accessibilty = Modifiers.Private, bool isStatic = false)
        {
            foreach (var rule in Rules)
            {
                if (!rule.AffectedEntity.HasFlag(entity))
                {
                    continue;
                }
                if (!rule.VisibilityMask.HasFlag(accessibilty))
                {
                    continue;
                }
                if (isStatic && !rule.IncludeStaticEntities || !isStatic && !rule.IncludeInstanceMembers)
                {
                    continue;
                }
                if (!rule.IsValid(name))
                {
                    return false;
                }
            }
            return true;
        }

        public bool HasValidRule(string name, AffectedEntity entity, Modifiers accessibilty = Modifiers.Private, bool isStatic = false)
        {
            foreach (var rule in Rules)
            {
                if (!rule.AffectedEntity.HasFlag(entity))
                {
                    continue;
                }
                if (!rule.VisibilityMask.HasFlag(accessibilty))
                {
                    continue;
                }
                if (isStatic && !rule.IncludeStaticEntities || !isStatic && !rule.IncludeInstanceMembers)
                {
                    continue;
                }
                if (rule.IsValid(name))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

