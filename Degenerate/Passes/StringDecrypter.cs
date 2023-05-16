using AsmResolver.DotNet.Code.Cil;
using AsmResolver.PE.DotNet.Cil;
using System.Security.Cryptography;
using System.Text;

namespace Degenerate.Passes
{
    internal class StringDecrypter : Pass
    {
        public override string Name => "String Decrypter";

        public override bool Recursive => false;

        public override (bool, CilMethodBody) Perform(CilMethodBody body)
        {
            bool patched = false;

            for (int i = 0; i < body.Instructions.Count; i++)
            {
                try
                {
                    if (body.Instructions[i].OpCode == CilOpCodes.Ldstr)
                    {
                        string value = body.Instructions[i].Operand.ToString();
                        if (!value.Contains("==") || body.Instructions[i + 1].OpCode != CilOpCodes.Call)
                            continue;

                        byte[] array = Convert.FromBase64String(value);
                        byte[] bytes = new Rfc2898DeriveBytes("p7K95451qB88sZ7J", Encoding.ASCII.GetBytes("2GM23j301t60Z96T")).GetBytes(32);
                        RijndaelManaged rijndaelManaged = new RijndaelManaged
                        {
                            Mode = CipherMode.CBC,
                            Padding = PaddingMode.PKCS7
                        };
                        ICryptoTransform cryptoTransform = rijndaelManaged.CreateDecryptor(bytes, Encoding.ASCII.GetBytes("IzTdhG6S8uwg141S"));
                        MemoryStream memoryStream = new MemoryStream(array);
                        CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read);
                        byte[] array2 = new byte[array.Length];
                        int num = cryptoStream.Read(array2, 0, array2.Length);
                        memoryStream.Close();
                        cryptoStream.Close();
                        string decrypted = Encoding.UTF8.GetString(array2, 0, num).TrimEnd("\0".ToCharArray());

                        body.Instructions[i].Operand = decrypted;
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
