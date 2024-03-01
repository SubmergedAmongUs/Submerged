// ReSharper disable All
// TODO: Update this to use source gens after translation issue is fixed

using System;
using System.IO;
using System.Reflection;

namespace ResourceEmbedderCompilerGenerated;

/// <summary>
///     Code that is injected into target assemblies.
///     Upon request for localized assemblies this will resolve and load the embedded resources.
/// </summary>
public static class ResourceEmbedderILInjected
{
    /// <summary>
    ///     Call once to attach the assembly resolve event.
    ///     All embedded satellite assemblies will then be loaded.
    ///     The convention is that each assembly stores it's own satellite assemblies as embedded resources.
    ///     If the application name is WpfExe, then the resources are stored as WpfExe.de.resources.dll,
    ///     WpfExe.fr.resources.dll, etc.
    ///     and will be loaded by this code.
    /// </summary>
    public static void Attach()
    {
        AppDomain currentDomain = AppDomain.CurrentDomain;
        currentDomain.AssemblyResolve += AssemblyResolve;
    }

    /// <summary>
    ///     Call to remove the hook event set by <see cref="Attach" />.
    /// </summary>
    public static void Detach()
    {
        AppDomain currentDomain = AppDomain.CurrentDomain;
        currentDomain.AssemblyResolve -= AssemblyResolve;
    }

    /// <summary>
    ///     Attach to resolve satellite assemblies from embedded resources.
    ///     Do not use directly, call <see cref="Attach" /> and <see cref="Detach" /> instead.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static Assembly AssemblyResolve(object sender, ResolveEventArgs args)
    {
        AssemblyName requestedAssemblyName;

        try
        {
            // validate user input
            // needed e.g. when Type.GetType is used as we are then part of the resolve chain
            requestedAssemblyName = new AssemblyName(args.Name);
        }
        catch (Exception e) when (e is ArgumentException || e is FileLoadException)
        {
            return null;
        }

        if (!IsLocalizedAssembly(requestedAssemblyName))
        {
            return null;
        }

        return LoadFromResource(requestedAssemblyName, args.RequestingAssembly);
    }

    /// <summary>
    ///     Finds the main assembly for the specific resource.
    ///     This requires that the resources name ends with .resources.
    /// </summary>
    /// <param name="requestedAssemblyName"></param>
    /// <returns></returns>
    private static Assembly FindMainAssembly(AssemblyName requestedAssemblyName)
    {
        if (requestedAssemblyName == null)
        {
            throw new ArgumentNullException(nameof(requestedAssemblyName));
        }

        if (!requestedAssemblyName.Name.EndsWith(".resources", StringComparison.InvariantCultureIgnoreCase))
        {
            throw new ArgumentException("Not a resource assembly");
        }

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // resources have the same name as their belonging assembly, so find by name
        string parentName = requestedAssemblyName.Name.Substring(0, requestedAssemblyName.Name.Length - ".resources".Length);
        // I'd love to use linq here, but Cecil starts fucking up when I do (null reference exception on assembly.Write)
        // without a Linq query it works fine, though

        foreach (Assembly assembly in assemblies)
        {
            if (assembly.GetName().Name == parentName)
            {
                return assembly;
            }
        }

        return null;
    }

    /// <summary>
    ///     Checks whether the requested assembly is a satellite assembly or not.
    /// </summary>
    /// <param name="requestedAssemblyName"></param>
    /// <returns></returns>
    private static bool IsLocalizedAssembly(AssemblyName requestedAssemblyName)
    {
        // only *.resources.dll files are satellite assemblies
        return requestedAssemblyName.Name.EndsWith(".resources", StringComparison.InvariantCultureIgnoreCase);
    }

    internal static Assembly LoadFromResource(AssemblyName requestedAssemblyName, Assembly requestingAssembly)
    {
        if (requestedAssemblyName == null || requestedAssemblyName.CultureInfo == null) return null; // without a concrete culture we cannot load a resource assembly

        // I haven't figured out how to add recursion to cecil (method cloner must know about the method itself already when copying it's instrutions)
        // so instead this is a loop with two possible exit points: localization found, or fallback route is depleted and we return null to let .Net locate the neutral resource
        while (true)
        {
            // requesting name in format: %assemblyname%.resources
            // rewrite to: %assemblyName%.%assemblyName%.%culture%.resources.dll
            //
            string baseName = requestedAssemblyName.Name.Substring(0, requestedAssemblyName.Name.Length - ".resources".Length);
            string name = $"{baseName}.{requestedAssemblyName.CultureInfo.Name}.resources.dll";

            // by default for resources the requestingAssembly will be null
            Assembly asm = requestingAssembly ?? FindMainAssembly(requestedAssemblyName);

            if (asm == null)
            {
                // cannot find assembly from which to load
                return null;
            }

            using (Stream stream = asm.GetManifestResourceStream(name))
            {
                if (stream != null)
                {
                    byte[] bytes = new byte[stream.Length];
                    // ReSharper disable once MustUseReturnValue
                    stream.Read(bytes, 0, bytes.Length);

                    return Assembly.Load(bytes);
                }
            }

            // did not find the specific resource yet
            // attempt to use the parent culture, this follows the .Net resource fallback system
            // e.g. if sub resource de-DE is not found, then .Parent will be "de", if that is not found parent will probably be default resource
            string fallback = requestedAssemblyName.CultureInfo.Parent.Name;

            if (string.IsNullOrEmpty(fallback))
            {
                // is empty if no longer a parent
                // return null so .Net can load the default resource
                return null;
            }

            string alteredAssemblyName = requestedAssemblyName.FullName;
            alteredAssemblyName = alteredAssemblyName.Replace($"Culture={requestedAssemblyName.CultureInfo.Name}", $"Culture={fallback}");

            requestedAssemblyName = new AssemblyName(alteredAssemblyName);
        }
    }
}
