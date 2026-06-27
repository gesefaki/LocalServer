using System.Runtime.CompilerServices;

namespace LocalServer.Server.Utils;

public static class PathUtil
{
    public static string GetProjectDirectory([CallerFilePath] string sourceFilePath = "")
    {
        return Path.GetDirectoryName(sourceFilePath) ?? throw new Exception("Cannot get project directory");
    }

    public static string GetParentPath(this string path, int levels = 1)
    {
        string result = path;
        for (int i = 0; i < levels; i++)
        {
            result = Directory.GetParent(result)?.FullName ?? throw new InvalidOperationException($"Cannot go up {levels} levels from {path}");
        }
        return result;
    }

    public static string Combine(this string path, params string[] parts)
    {
        return Path.Combine([path, .. parts]);
    }
}