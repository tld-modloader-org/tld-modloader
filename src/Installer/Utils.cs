using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Linq;

namespace Installer
{
    public static class Utils
    {
        public static MethodDef FindMethod(this ModuleDefMD module, UTF8String @class, UTF8String method, MethodSig signature = null) =>
            signature == null ?
                module.GetTypes().First(x => x.Name == @class).FindMethod(method) :
                module.GetTypes().First(x => x.Name == @class).FindMethod(method, signature);

        public static void InsertCall(ModuleDefMD module, UTF8String @class, UTF8String method, int line, IMethod inject, MethodSig signature = null) =>
            InsertCall(FindMethod(module, @class, method, signature), line, inject);

        public static void InsertCall(MethodDef method, int line, IMethod inject)
        {
            var body = method.Body;
            if (line == -1) line = body.Instructions.Count - 1;
            body.Instructions.Insert(line, new Instruction(OpCodes.Call, inject));
            body.OptimizeBranches();
        }
    }
}
