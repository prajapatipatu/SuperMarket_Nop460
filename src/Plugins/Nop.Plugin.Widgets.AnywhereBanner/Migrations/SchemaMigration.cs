using FluentMigrator;
using Nop.Data.Extensions;
using Nop.Data.Mapping;
using Nop.Data.Migrations;
using Nop.Plugin.Widgets.AnywhereBanner.Domains.AnywhereBanner;
using Nop.Plugin.Widgets.AnywhereBanner.Domains.GridBannerSetting;

namespace Nop.Plugin.Widgets.AnywhereBanner.Migrations
{
    [NopMigration("2021/06/15 12:05:00", "Nop.Plugin.Widgets.AnywhereBanner schema", MigrationProcessType.Installation)]
    public class SchemaMigration : Migration
    {
        /// <summary>
        /// Collect the UP migration expressions
        /// </summary>
        public override void Up()
        {
            if (!Schema.Table("Nop_AnywhereBannerDetails").Exists())
                Create.TableFor<AnywhereBannerDetails>();

            if (!Schema.Table("Nop_GridBannerSetting").Exists())
                Create.TableFor<GridBannerSetting>();
        }
        public override void Down()
        {
            if (Schema.Table("Nop_AnywhereBannerDetails").Exists())
                Delete.Table(NameCompatibilityManager.GetTableName(typeof(AnywhereBannerDetails)));

            if (Schema.Table("Nop_GridBannerSetting").Exists())
                Delete.Table(NameCompatibilityManager.GetTableName(typeof(GridBannerSetting)));
        }
    }
}
