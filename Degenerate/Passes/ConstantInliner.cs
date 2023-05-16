using AsmResolver.DotNet;
using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;

namespace Degenerate.Passes
{
    internal class ConstantInliner : Pass
    {
        public override string Name => "Constant Inliner";

        public override bool Recursive => false;

        public override (bool, CilMethodBody) Perform(CilMethodBody body)
        {
            bool patched = false;

            for (int i = 0; i < body.Instructions.Count; i++)
            {
                try
                {
                    if (body.Instructions[i].OpCode == CilOpCodes.Call)
                    {
                        var desc = (IMethodDescriptor)body.Instructions[i].Operand;
                        var method = desc.Resolve();

                        if (method == null || !method.HasMethodBody || method.CilMethodBody == null)
                            continue;
                        
                        if ((method.CilMethodBody.Instructions[0].OpCode == CilOpCodes.Ldc_I4 ||
                            method.CilMethodBody.Instructions[0].OpCode == CilOpCodes.Ldstr) &&
                            method.CilMethodBody.Instructions[1].OpCode == CilOpCodes.Ret)
                        {
                            var constant = method.CilMethodBody.Instructions[0].Operand;
                            Console.WriteLine($"Inlining {method.FullName} into {body.Owner.FullName} with value {constant}");

                            body.Instructions[i] = new CilInstruction(
                                method.CilMethodBody.Instructions[0].OpCode, constant);
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
