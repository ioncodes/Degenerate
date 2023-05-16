using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace Degenerate.Passes
{
    internal class MathSimplifier : Pass
    {
        public override string Name => "Math Simplifier";

        public override bool Recursive => false;

        public override (bool, CilMethodBody) Perform(CilMethodBody body)
        {
            bool patched = false;

            for (int i = 0; i < body.Instructions.Count; i++)
            {

                var instruction = body.Instructions[i];
                try
                {
                    if (instruction.OpCode == CilOpCodes.Call &&
                        instruction.Operand.ToString().Contains("System.Double System.Math::Sin(System.Double)"))
                    {
                        // 49	00A2	ldc.r8	1.5707963267948966
                        // 50	00AB	call	float64 [mscorlib]System.Math::Sin(float64)

                        Console.WriteLine($"Found Math.Sin in {body.Owner.FullName}!");

                        var loadInstruction = body.Instructions[i - 1];
                        if (loadInstruction.OpCode != CilOpCodes.Ldc_R8)
                            continue;

                        double value = Math.Sin((double)loadInstruction.Operand);
                        Console.WriteLine($"Evaluated Math.Sin expression: {value}");

                        // TODO: this might cause stack imbalance as we nop 1 stack push

                        // ldc.r8 <value>
                        body.Instructions[i - 1].Operand = value;
                        body.Instructions.RemoveAt(i);
                        
                        patched = true;
                    }
                    else if (instruction.OpCode == CilOpCodes.Call &&
                             instruction.Operand.ToString().Contains("System.Double System.Math::Cos(System.Double)"))
                    {
                        // 58	00E6	ldc.r8	1.7205647006611141E-09
                        // 59	00EF	call	float64 [mscorlib]System.Math::Cos(float64)
                        Console.WriteLine($"Found Math.Cos in {body.Owner.FullName}!");

                        var loadInstruction = body.Instructions[i - 1];
                        if (loadInstruction.OpCode != CilOpCodes.Ldc_R8)
                            continue;

                        double value = Math.Cos((double)loadInstruction.Operand);
                        Console.WriteLine($"Evaluated Math.Cos expression: {value}");

                        // TODO: this might cause stack imbalance as we nop 1 stack push

                        // ldc.r8 <value>
                        body.Instructions[i - 1].Operand = value;
                        body.Instructions.RemoveAt(i);

                        patched = true;
                    }
                }
                catch { }
            }

            return (patched, body);
        }
    }
}
