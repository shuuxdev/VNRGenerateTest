using System.Reflection;
using System.ComponentModel;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
public static class CustomAssemblies
{

    public static List<Assembly> assemblies = new();

    public static Assembly Get(string assemblyName)
    {
        return assemblies.FirstOrDefault(assembly => assembly.GetName().Name == assemblyName);
    }
    public static void LoadAllNecessaryDll()
    {
        string rootDirectory = System.IO.Directory.GetCurrentDirectory();
        string[] DLLs = Directory.GetFiles($"{rootDirectory}\\Dll");
        foreach (string dll in DLLs)
        {
            var assembly = Assembly.LoadFrom(dll);
            if (assembly == null)
            {
                throw new Exception("Cannot load assemby: " + dll);
            }
            CustomAssemblies.assemblies.Add(assembly);
        }
    }
    public static Assembly LoadModelIntoAssembly(string classContent, string nameSpace, string className)
    {
        // define source code, then parse it (to the type used for compilation)

        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(classContent);
        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);


        // define other necessary objects for compilation
        string assemblyName = Path.GetRandomFileName();
        List<MetadataReference> references =
        [
                MetadataReference.CreateFromFile(CustomAssemblies.Get("HRM.Presentation.Service").Location),
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DisplayNameAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.RangeAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Xml.Serialization.XmlSerializer).Assembly.Location),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
                .. CustomAssemblies.assemblies.Select(assembly => MetadataReference.CreateFromFile(assembly.Location)),

        ];


        // analyse and generate IL code from syntax tree
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using (var ms = new MemoryStream())
        {
            // write IL code into memory
            EmitResult result = compilation.Emit(ms);

            if (!result.Success)
            {
                // handle exceptions
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);

                foreach (Diagnostic diagnostic in failures)
                {
                    Console.Error.WriteLine("{0}: {1}", diagnostic.Id, diagnostic.GetMessage());
                }
            }
            // load this 'virtual' DLL so that we can use
            ms.Seek(0, SeekOrigin.Begin);
            Assembly assembly = Assembly.Load(ms.ToArray());
            return assembly;
        }
    }
}