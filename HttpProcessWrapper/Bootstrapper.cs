using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HttpProcessWrapper
{
    public class Bootstrapper : DefaultNancyBootstrapper
    {
        // The bootstrapper enables you to reconfigure the composition of the framework,
        // by overriding the various methods and properties.
        // For more information https://github.com/NancyFx/Nancy/wiki/Bootstrapper

        protected override IEnumerable<Func<Assembly, bool>> AutoRegisterIgnoredAssemblies
        {
            get
            {
                return base.AutoRegisterIgnoredAssemblies.Union(new Func<Assembly, bool>[]
                {
                    asm => asm.FullName.Contains("MinecraftWebToolkit"),
                    asm => asm.FullName.Contains("MCWTK"),
                });
            }
        }
    }
}