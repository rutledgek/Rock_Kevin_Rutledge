using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace online.kevinrutledge.InvoiceSystem.Migrations
{
    [MigrationNumber(4, "1.15.0")]
    public class CreateDB_SystemCommunications : Migration
    {
        public override void Up()
        {

            RockMigrationHelper.UpdateCategory(Rock.SystemGuid.EntityType.SYSTEM_COMMUNICATION,"Invoice System",null,"Category for Invoice System Communications",online.kevinrutledge.InvoiceSystem.SystemGuids.Categories.InvoiceSystemCommumincations);

        }

        public override void Down()
        {

            RockMigrationHelper.DeleteCategory(online.kevinrutledge.InvoiceSystem.SystemGuids.Categories.InvoiceSystemCommumincations);
           
        }
    }


}
