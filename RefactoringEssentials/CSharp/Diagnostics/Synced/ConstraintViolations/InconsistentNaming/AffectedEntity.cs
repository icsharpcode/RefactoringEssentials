using System;

namespace RefactoringEssentials.CSharp.Diagnostics
{
    [Flags]
    public enum AffectedEntity
    {
        None,

        Namespace = 1 << 0,

        Class = 1 << 1,
        Struct = 1 << 2,
        Enum = 1 << 3,
        Interface = 1 << 4,
        Delegate = 1 << 5,

        CustomAttributes = 1 << 6,
        CustomEventArgs = 1 << 7,
        CustomExceptions = 1 << 8,

        Property = 1 << 9,
        Method = 1 << 10,
        AsyncMethod = 1 << 11,
        Field = 1 << 12,
        ReadonlyField = 1 << 13,
        ConstantField = 1 << 14,

        Event = 1 << 15,
        EnumMember = 1 << 16,

        Parameter = 1 << 17,
        TypeParameter = 1 << 18,

        // Unit test special case
        TestType = 1 << 19,
        TestMethod = 1 << 20,

        // private entities
        LambdaParameter = 1 << 21,
        LocalVariable = 1 << 22,
        LocalConstant = 1 << 23,
        Label = 1 << 24,

        LocalVars = LocalVariable | Parameter | LambdaParameter | LocalConstant,
        Methods = Method | AsyncMethod,
        Fields = Field | ReadonlyField | ConstantField,
        Member = Property | Methods | Fields | Event | EnumMember,

        Type = Class | Struct | Enum | Interface | Delegate,

    }
}
