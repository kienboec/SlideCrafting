using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace SlideCrafting.Config
{
    public static class ConfigExtensions
    {
        public static IOptions<SlideCraftingConfig> AssureOriginExists(this IOptions<SlideCraftingConfig> config)
        {
            if (!Directory.Exists(config.Value.OriginFolder))
            {
                throw new DirectoryNotFoundException("OriginFolder not found at " + config.Value.OriginFolder);
            }

            return config;
        }

        public static IOptions<SlideCraftingConfig> AssureDistributionExists(this IOptions<SlideCraftingConfig> config)
        {
            if (!Directory.Exists(config.Value.DistFolder))
            {
                throw new DirectoryNotFoundException("DistFolder not found at " + config.Value.DistFolder);
            }

            return config;
        }
    }
}
