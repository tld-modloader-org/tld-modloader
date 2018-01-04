using System;

namespace Loader
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class CommandAttribute : Attribute
    {
        public readonly string Name;
        
        public CommandAttribute() { }

        public CommandAttribute(string name) => Name = name;
    }
}