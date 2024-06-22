using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using SS.Plugin.Widgets.ProductReview.Domains;

namespace SS.Plugin.Widgets.ProductReview.Migrations
{
    [NopMigration("2023/02/03 14:47:00", "SS.Plugin.Widgets.ProductReview schema", MigrationProcessType.Installation)]
    public class SchemaMigration : Migration
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
            if (!Schema.Table("SS_ProductReviewImages").Exists())
                Create.TableFor<ProductReviewImages>();
        }
        public override void Down()
        {
            if (Schema.Table("SS_ProductReviewImages").Exists())
                Delete.Table(NameCompatibilityManager.GetTableName(typeof(ProductReviewImages)));
        }
    }
}
