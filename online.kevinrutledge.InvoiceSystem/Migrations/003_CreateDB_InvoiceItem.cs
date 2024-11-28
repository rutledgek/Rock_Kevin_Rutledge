using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace online.kevinrutledge.InvoiceSystem.Migrations
{
    [MigrationNumber(3, "1.15.0")]
    public class CreateDB_InvoiceItem : Migration
    {
        public override void Up()
        {
            Sql(
                @"
                CREATE TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceItem](
                    [Id] [int] IDENTITY(1,1) NOT NULL,
                    [InvoiceId] [int] NOT NULL,
                    [Description] [nvarchar](max) NULL,
                    [Quantity] [int] NOT NULL Default 1,
                    [UnitPrice] [decimal](18, 2) NOT NULL,
                    [DiscountPercentage] [decimal](18, 2) NULL,
                    [DiscountAmount] [decimal](18, 2) NULL,
                    [TaxPercentage] [decimal](18, 2) NULL,
				    [EntityTypeId] [int] Null,
				    [EntityId] [int] Null,
                    [Guid] [uniqueidentifier] NOT NULL,
                    [CreatedDateTime] [datetime] NULL,
                    [ModifiedDateTime] [datetime] NULL,
                    [CreatedByPersonAliasId] [int] NULL,
                    [ModifiedByPersonAliasId] [int] NULL,
                    [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                    CONSTRAINT [PK__online_kevinrutledge_InvoiceSystem_InvoiceItem] PRIMARY KEY CLUSTERED
                        (
	                        [Id] ASC
                        )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                        ) ON [PRIMARY]


                    ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceItem]  WITH CHECK ADD  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceItem_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
		            REFERENCES [dbo].[PersonAlias] ([Id])

	                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceItem] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceItem_CreatedByPersonAliasId]

	                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceItem]  WITH CHECK ADD  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceItem_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
		            REFERENCES [dbo].[PersonAlias] ([Id])

	                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceItem] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceItem_ModifiedByPersonAliasId]


                    ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceItem]  WITH CHECK ADD  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceItem_InvoiceId] FOREIGN KEY([InvoiceId])
		            REFERENCES [dbo].[_online_kevinrutledge_InvoiceSystem_Invoice] ([Id])

	                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceItem] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceItem_InvoiceId]



            ");


            /* Update the EntityType */
            RockMigrationHelper.UpdateEntityType("online.kevinrutledge.InvoiceSystem.Model.InvoiceItem", online.kevinrutledge.InvoiceSystem.SystemGuids.EntityTypeGuids.Invoice_Item, true, true);
        }

        public override void Down()
        {
            Sql(
                @"
                
	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceItem] Drop CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceItem_CreatedByPersonAliasId]
                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceItem]  Drop CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceItem_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceItem]  Drop CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceItem_InvoiceId] 
                Drop Table  [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceItem]
                "
            );

            RockMigrationHelper.DeleteEntityType(online.kevinrutledge.InvoiceSystem.SystemGuids.EntityTypeGuids.Invoice_Item);
        }
    }

    
}
