using System;

namespace EditorEssentials.Runtime
{
    [AttributeUsage(AttributeTargets.Field)]
    public class NonNullableAttribute : Attribute { }
}