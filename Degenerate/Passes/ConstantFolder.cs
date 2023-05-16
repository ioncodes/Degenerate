using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace Degenerate.Passes
{
    internal class ConstantFolder : Pass
    {
        public override string Name => "Constant Folder";

        public override bool Recursive => true;

        public override (bool, CilMethodBody) Perform(CilMethodBody body)
        {
            bool patched = false;

            for (int i = 0; i < body.Instructions.Count; i++)
            {
                try
                {
                    if (body.Instructions[i].OpCode == CilOpCodes.Ldc_I4 &&
                        (body.Instructions[i + 1].OpCode == CilOpCodes.Add || body.Instructions[i + 1].OpCode == CilOpCodes.Sub) &&
                        body.Instructions[i + 2].OpCode == CilOpCodes.Ldc_I4)
                    {
                        int lhs = (int)body.Instructions[i].Operand;
                        int rhs = (int)body.Instructions[i + 2].Operand;
                        var op = body.Instructions[i + 1].OpCode;

                        if (op == CilOpCodes.Add)
                        {
                            Console.WriteLine($"Folding expression {lhs} + {rhs} = {lhs + rhs}");
                            body.Instructions[i].Operand = lhs + rhs;
                            body.Instructions.RemoveRange(i + 1, 2);
                            patched = true;
                        }
                        else if (op == CilOpCodes.Sub)
                        {
                            Console.WriteLine($"Folding expression {lhs} - {rhs} = {lhs - rhs}");
                            body.Instructions[i].Operand = lhs - rhs;
                            body.Instructions.RemoveRange(i + 1, 2);
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
