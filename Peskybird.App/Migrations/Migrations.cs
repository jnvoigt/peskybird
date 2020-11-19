using System.Collections.Generic;
using System.Linq;

namespace Peskybird.App.Migrations
{
    public static class MigrationCollector
    
    {
        public static IEnumerable<string> MigrationNames()
        {
            var type = typeof(MigrationCollector);
            var baseNamespace = type.Namespace;
            return type.Assembly.GetManifestResourceNames().Where(name => name.StartsWith(baseNamespace!));
        }
    }
}