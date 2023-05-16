using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace Degenerate.Passes
{
    internal class TypeTruncater : Pass
    {
        public override string Name => "Type Truncater";

        public override bool Recursive => false;

        public override (bool, CilMethodBody) Perform(CilMethodBody body)
        {
            bool patched = false;

            for (int i = 0; i < body.Instructions.Count; i++)
            {
                var instruction = body.Instructions[i];
                try
                {
                    if (instruction.OpCode == CilOpCodes.Ldc_R8 &&
                        !instruction.Operand.ToString().Contains('.'))
                    {
                        if (int.TryParse(instruction.Operand.ToString(), out var result4))
                        {
                            body.Instructions[i] = new CilInstruction(CilOpCodes.Ldc_I4, result4);
                            patched = true;
                        }
                        else if (long.TryParse(instruction.Operand.ToString(), out var result8))
                        {
                            body.Instructions[i] = new CilInstruction(CilOpCodes.Ldc_I8, result8);
                            patched = true;
                        }
                    }

                    if (body.Instructions[i + 1].OpCode == CilOpCodes.Conv_I4 ||
                        body.Instructions[i + 1].OpCode == CilOpCodes.Conv_I8)
                    {
                        body.Instructions.RemoveAt(i + 1);
                        patched = true;
                    }
                }
                catch { }
            }

            return (patched, body);
        }
    }
}
