using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace online.kevinrutledge.InvoiceSystem.Migrations
{
    [MigrationNumber(2, "1.15.0")]
    public class CreateDB_Invoice : Migration
    {
        public override void Up()
        {
            Sql(
                @" CREATE TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_Invoice]( 
	            [Id] [int] IDENTITY(1,1) NOT NULL,
                [InvoiceTypeId] [int] NOT NULL,
                [InvoiceStatus] [int] Null,
                [Name] [nvarchar](100) NOT NULL,
                [Summary] [nvarchar](max) NULL,
                [DueDate] [datetime] NULL,
                [LateDate] [datetime] NULL,
                [LastSentDate] [datetime] Null,
	            [Guid] [uniqueidentifier] NOT NULL,
	            [CreatedDateTime] [datetime] NULL,
	            [ModifiedDateTime] [datetime] NULL,
	            [CreatedByPersonAliasId] [int] NULL,
	            [ModifiedByPersonAliasId] [int] NULL,
	            [ForeignKey] [nvarchar](50) NULL,
	            [ForeignGuid] [uniqueidentifier] NULL,
	            [ForeignId] [int] NULL,
	            CONSTRAINT [PK__online_kevinrutledge_InvoiceSystem_Invoice] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
 
	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_Invoice]  WITH CHECK ADD  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_Invoice_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
		            REFERENCES [dbo].[PersonAlias] ([Id])

	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_Invoice] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_Invoice_CreatedByPersonAliasId]

	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_Invoice]  WITH CHECK ADD  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_Invoice_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
		            REFERENCES [dbo].[PersonAlias] ([Id])

	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_Invoice] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_Invoice_ModifiedByPersonAliasId]


                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_Invoice]  WITH CHECK ADD  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_Invoice_InvoiceTypeId] FOREIGN KEY([InvoiceTypeId])
		            REFERENCES [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] ([Id])

	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_Invoice] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_Invoice_InvoiceTypeId]
 



            "
            );




            /* Update the EntityType */
            RockMigrationHelper.UpdateEntityType("online.kevinrutledge.InvoiceSystem.Model.Invoice", online.kevinrutledge.InvoiceSystem.SystemGuids.EntityTypeGuids.Invoice, true, true);
            RockMigrationHelper.UpdateBlockType("Invoice Detail", "", "~/Plugins/online_kevinrutledge/InvoiceSystem/InvoiceDetail.ascx", "online_kevinrutledge > Invoice System", online.kevinrutledge.InvoiceSystem.SystemGuids.BlockTypeGuids.InvoiceDetail);
            RockMigrationHelper.UpdateBlockType("Invoice List", "", "~/Plugins/online_kevinrutledge/InvoiceSystem/InvoiceList.ascx", "online_kevinrutledge > Invoice System", online.kevinrutledge.InvoiceSystem.SystemGuids.BlockTypeGuids.InvoiceList);

            /* Create Invoice List Page */
            RockMigrationHelper.AddPage(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceSystemParentPage, online.kevinrutledge.InvoiceSystem.SystemGuids.SystemGuids.FullWidthLayout, "Invoice List Page", "", online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceListPage);
            RockMigrationHelper.AddPageRoute(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceListPage, "invoices");
            RockMigrationHelper.AddPageRoute(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceListPage, "invoices/{InvoiceTypeId}");
            RockMigrationHelper.AddBlock(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceListPage, null, online.kevinrutledge.InvoiceSystem.SystemGuids.BlockTypeGuids.InvoiceList, "Invoice List Page", "Main", "", "", 0, online.kevinrutledge.InvoiceSystem.SystemGuids.BlockGuids.InvoiceListBlock);

            /* Create Invoice Detail Page */
            RockMigrationHelper.AddPage(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceListPage, online.kevinrutledge.InvoiceSystem.SystemGuids.SystemGuids.FullWidthLayout, "Invoice Detail Page", "", online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceDetailPage, "fa fa-pencil");
            RockMigrationHelper.AddPageRoute(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceDetailPage,"invoice");
            RockMigrationHelper.AddPageRoute(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceDetailPage, "invoice/{InvoiceId}");
            RockMigrationHelper.AddPageRoute(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceDetailPage, "invoice/{InvoiceTypeId}/{InvoiceId}");

            RockMigrationHelper.AddBlock(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceDetailPage, null, online.kevinrutledge.InvoiceSystem.SystemGuids.BlockTypeGuids.InvoiceDetail, "Invoice Detail Page", "Main", "", "", 0, online.kevinrutledge.InvoiceSystem.SystemGuids.BlockGuids.InvoiceDetailBlock);

           

        }

        public override void Down()
        {
            Sql(
                @"
                
	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_Invoice] Drop CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_Invoice_CreatedByPersonAliasId] 
		        ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_Invoice]  Drop CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_Invoice_ModifiedByPersonAliasId] 
                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_Invoice]  Drop  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_Invoice_InvoiceTypeId]
				Drop Table  [dbo].[_online_kevinrutledge_InvoiceSystem_Invoice]
"
            );

            RockMigrationHelper.DeleteEntityType(online.kevinrutledge.InvoiceSystem.SystemGuids.EntityTypeGuids.Invoice);
            RockMigrationHelper.DeleteBlockType(online.kevinrutledge.InvoiceSystem.SystemGuids.BlockTypeGuids.InvoiceDetail);
            RockMigrationHelper.DeletePage(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceDetailPage);
            RockMigrationHelper.DeletePage(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceListPage);

        }
    }
}