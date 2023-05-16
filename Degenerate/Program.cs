using AsmResolver.DotNet;
using AsmResolver.DotNet.Builder;
using AsmResolver.PE.DotNet.Builder;
using Degenerate;
using Degenerate.Passes;

string path = args[0];

if (File.Exists(path))
{
    Console.WriteLine($"Deobfuscating file {path}...");
    var image = ModuleDefinition.FromFile(path);
    Console.WriteLine($"Entrypoint: {image.ManagedEntryPoint}");

    List<Pass> passes = new()
    {
        //new SymbolRenamer(),
        new ConstantInliner(),
        new StringDecrypter(),
        new MathSimplifier(),
        new SizeofSimplifier(),
        new TypeTruncater(),
        new ConstantFolder(),
        new JunkRemover()
    };

    foreach (var type in image.GetAllTypes())
    {
        foreach (var method in type.Methods)
        {
            if (!method.HasMethodBody || method.CilMethodBody == null)
                continue;

            foreach (var pass in passes)
            {
                bool patched = false;
                do
                {
                    Console.WriteLine($"Executing pass \"{pass.Name}\" on \"{method.FullName}\"...");
                    pass.SetModule(image);
                    (patched, var body) = pass.Perform(method.CilMethodBody);
                    if (patched)
                        method.CilMethodBody = body;
                } while (patched && pass.Recursive);
            }
        }
    }

    var builder = new ManagedPEImageBuilder();
    var factory = new DotNetDirectoryFactory
    {
        MetadataBuilderFlags = MetadataBuilderFlags.None
    };
    builder.DotNetDirectoryFactory = factory;

    var result = builder.CreateImage(image);

    Console.WriteLine("Construction finished with {0} errors.", result.DiagnosticBag.Exceptions.Count);
    foreach (var error in result.DiagnosticBag.Exceptions)
        Console.WriteLine(error.Message);

    if (!result.DiagnosticBag.IsFatal)
    {
        var fileBuilder = new ManagedPEFileBuilder();
        var file = fileBuilder.CreateFile(result.ConstructedImage);
        file.Write(path + ".degenerate");
    }
    else
    {
        // for save injector:
        // run de4dot (fixes some metadata)
        // then check error output. change all eventtypes in metadata to 4
        Console.WriteLine("Fatal error!");
    }
}
else
{
    Console.WriteLine("File not found...");
}