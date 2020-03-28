using System;

namespace EditorEssentials.Editor
{
    [AttributeUsage(AttributeTargets.Field)]
    public class NonNullableAttribute : Attribute { }
}