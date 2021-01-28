using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using DbUp;

namespace Peskybird.Migrations
{
    public static class Migrator
    {
        public static bool Migrate()
        {
            var upgrader = DeployChanges.To
                .SQLiteDatabase("Data Source=pesky.sqlite")
                .WithScriptsEmbeddedInAssembly(typeof(Migrator).Assembly)
                .LogToConsole()
                .Build();
            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {

                return false;
            }

            return true;
        }
    }
}