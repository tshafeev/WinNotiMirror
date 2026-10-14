using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace WinNotiMirror;

public static class SparsePackageRegistrar
{
    private const string PackageName = "WinNotiMirror";

    public static bool HasIdentity()
    {
        try
        {
            _ = Package.Current;
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string? FindSparseMsixPath()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "packaging", "sparse.msix");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        return null;
    }

    private static Package? FindDeployedPackage()
    {
        var packageManager = new PackageManager();
        return packageManager.FindPackagesForUser(string.Empty)
            .FirstOrDefault(p => p.Id.Name == PackageName);
    }

    /// <summary>
    /// Ensures the sparse package is deployed (registering it if needed) and returns its
    /// PackageFamilyName, which is required to relaunch the app with identity via
    /// shell:AppsFolder activation.
    /// </summary>
    public static async Task<string?> EnsureDeployedAndGetFamilyNameAsync(string? msixPath)
    {
        var existing = FindDeployedPackage();
        if (existing is not null)
        {
            return existing.Id.FamilyName;
        }

        if (msixPath is null || !File.Exists(msixPath))
        {
            return null;
        }

        var packageManager = new PackageManager();
        var packageUri = new Uri(msixPath);
        var externalUri = new Uri(AppContext.BaseDirectory);

        var options = new AddPackageOptions
        {
            ExternalLocationUri = externalUri
        };

        try
        {
            await packageManager.AddPackageByUriAsync(packageUri, options).AsTask();
        }
        catch
        {
            return null;
        }

        return FindDeployedPackage()?.Id.FamilyName;
    }

    /// <summary>
    /// A raw-launched .exe never has package identity, even after the sparse package is
    /// deployed — only a process activated via shell:AppsFolder\{familyName}!App does.
    /// </summary>
    public static void RelaunchViaAppActivation(string familyName)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"shell:AppsFolder\\{familyName}!App",
            UseShellExecute = true
        };
        Process.Start(psi);
    }
}
