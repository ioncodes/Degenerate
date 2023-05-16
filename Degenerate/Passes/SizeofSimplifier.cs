using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace Degenerate.Passes
{
    internal class SizeofSimplifier : Pass
    {
        public override string Name => "Sizeof Simplifier";

        public override bool Recursive => false;

        public override (bool, CilMethodBody) Perform(CilMethodBody body)
        {
            bool patched = false;

            for (int i = 0; i < body.Instructions.Count; i++)
            {
                var instruction = body.Instructions[i];
                try
                {
                    if (instruction.OpCode == CilOpCodes.Sizeof &&
                        instruction.Operand.ToString().Contains("System."))
                    {
                        int ResolveSizeOf(string type)
                        {
                            switch (type)
                            {
                                case "System.SByte":
                                case "System.Byte":
                                case "System.Boolean":
                                    return 1;
                                default:
                                    throw new NotImplementedException();
                            }
                        }

                        int size = ResolveSizeOf(instruction.Operand.ToString());
                        Console.WriteLine($"Found sizeof({instruction.Operand}). Evaluated to: {size}");

                        body.Instructions[i] = new CilInstruction(CilOpCodes.Ldc_I4, size);

                        patched = true;
                    }
                }
                catch { }
            }

            return (patched, body);
        }
    }
}
