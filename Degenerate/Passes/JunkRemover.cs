using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace Degenerate.Passes
{
    internal class JunkRemover : Pass
    {
        public override string Name => "Junk Remover";

        public override bool Recursive => true;

        public override (bool, CilMethodBody) Perform(CilMethodBody body)
        {
            bool patched = false;

            for (int i = 0; i < body.Instructions.Count; i++)
            {
                try
                {
                    if (body.Instructions[i].OpCode == CilOpCodes.Ldc_I4 &&
                        (body.Instructions[i + 1].OpCode == CilOpCodes.Add || body.Instructions[i + 1].OpCode == CilOpCodes.Sub))
                    {
                        if (int.TryParse(body.Instructions[i].Operand.ToString(), out var result) && result == 0)
                        {
                            body.Instructions.RemoveAt(i);
                            body.Instructions.RemoveAt(i);
                            patched = true;
                        }
                    }
                }
                catch { }
            }

            return (patched, body);
        }
    }
}
