using AsmResolver.DotNet.Code.Cil;

namespace Degenerate.Passes
{
    internal class SymbolRenamer : Pass
    {
        public override string Name => throw new NotImplementedException();

        public override bool Recursive => throw new NotImplementedException();

        public override (bool, CilMethodBody) Perform(CilMethodBody body)
        {
            throw new NotImplementedException();
        }
    }
}
