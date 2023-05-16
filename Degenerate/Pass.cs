using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;

namespace Degenerate
{
    abstract class Pass
    {
        public abstract string Name { get; }
        public abstract bool Recursive { get; }
        public ModuleDefinition Module { get; private set; }

        public void SetModule(ModuleDefinition module) => Module = module;

        // we need the bool to denote whether the body has changed in case of recursive passes
        public abstract (bool, CilMethodBody) Perform(CilMethodBody body);
    }
}
