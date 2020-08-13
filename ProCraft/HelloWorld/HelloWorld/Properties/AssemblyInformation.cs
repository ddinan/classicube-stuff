using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

using Tpa;

[assembly: ComVisible(true)]
[assembly: Guid("148e4a4c-4799-4e48-877b-8b3f1edc3eaf")]
[assembly: AssemblyTitle(Init.PlainDescription)]
[assembly: AssemblyProduct(Init.PlainName)]
[assembly: AssemblyVersion(Init.PlainVersion)]
[assembly: AssemblyFileVersion(Init.PlainVersion)]
[assembly: AssemblyDescription(Init.PlainName)]
[assembly: AssemblyCulture(Init.PlainRegion)]
[assembly: NeutralResourcesLanguage(Init.PlainRegion)]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyCopyright("Copyright(C) 2017 " + Init.PlainAuthor +
							 ". All rights reserved.")]
