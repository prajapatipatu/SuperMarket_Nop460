using FluentMigrator;
using Nop.Data.Migrations;

namespace Nop.Plugin.InstantOnePageCheckout.Migrations
{
    [NopMigration("2022/05/13 12:00:00", "Nop.Plugin.InstantOnePageCheckout schema", MigrationProcessType.Installation)]
    public class SchemaMigration : AutoReversingMigration
    {
        private readonly IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
        }
    }
}
