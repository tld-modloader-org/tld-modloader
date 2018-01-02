using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Loader
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ScriptAttribute : Attribute { }
}
