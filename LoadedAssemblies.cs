using System.Reflection;

public static class LoadedAssemblies
{

    public static List<Assembly> assemblies = new();

    public static Assembly Get(string assemblyName)
    {
        return assemblies.FirstOrDefault(assembly => assembly.GetName().Name == assemblyName);
    }

}