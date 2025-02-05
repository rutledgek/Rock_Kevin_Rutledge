using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace online.kevinrutledge.InvoiceSystem.Migrations
{
    [MigrationNumber(6, "1.15.0")]
    public class CreateDB_ScheduledInvoice : Migration
    {
        public override void Up()
        {
            Sql(
                @" CREATE TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_ScheduledInvoice]( 
	            [Id] [int] IDENTITY(1,1) NOT NULL,
                [InvoiceTypeId] [int] NOT NULL,
                [IsActive] bit NOT NULL Default 1,
                [Name] [nvarchar](100) NOT NULL,
                [Summary] [nvarchar](max) NULL,
                [ScheduleId] [int] NOT NULL,
                [SendInvoiceDaysBefore] [int] NULL,
                [DaysUntilLate] [int] NULL,
	            [Guid] [uniqueidentifier] NOT NULL,
	            [CreatedDateTime] [datetime] NULL,
	            [ModifiedDateTime] [datetime] NULL,
	            [CreatedByPersonAliasId] [int] NULL,
	            [ModifiedByPersonAliasId] [int] NULL,
	            [ForeignKey] [nvarchar](50) NULL,
	            [ForeignGuid] [uniqueidentifier] NULL,
	            [ForeignId] [int] NULL,
	            CONSTRAINT [PK__online_kevinrutledge_InvoiceSystem_ScheduledInvoice] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
 
	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_ScheduledInvoice]  WITH CHECK ADD  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_ScheduledInvoice_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
		            REFERENCES [dbo].[PersonAlias] ([Id])

	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_ScheduledInvoice] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_ScheduledInvoice_CreatedByPersonAliasId]

	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_ScheduledInvoice]  WITH CHECK ADD  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_ScheduledInvoice_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
		            REFERENCES [dbo].[PersonAlias] ([Id])

	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_ScheduledInvoice] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_ScheduledInvoice_ModifiedByPersonAliasId]


                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_ScheduledInvoice]  WITH CHECK ADD  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_ScheduledInvoice_InvoiceTypeId] FOREIGN KEY([InvoiceTypeId])
		            REFERENCES [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] ([Id])

	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_ScheduledInvoice] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_ScheduledInvoice_InvoiceTypeId]
 

                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_ScheduledInvoice] WITH CHECK ADD  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_ScheduledInvoice_ScheduleInvoiceSheduleId] FOREIGN KEY([ScheduleId])
                    REFERENCES [dbo].[Schedule] ([Id])

                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_ScheduledInvoice] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_ScheduledInvoice_ScheduleInvoiceSheduleId]


            "
            );




            /* Update the EntityType */
            RockMigrationHelper.UpdateEntityType("online.kevinrutledge.InvoiceSystem.Model.ScheduledInvoice", online.kevinrutledge.InvoiceSystem.SystemGuids.EntityTypeGuids.Scheduled_Invoice, true, true);
            RockMigrationHelper.UpdateBlockType("Scheduled Invoice Detail", "", "~/Plugins/online_kevinrutledge/InvoiceSystem/ScheduledInvoiceDetail.ascx", "online_kevinrutledge > Invoice System", online.kevinrutledge.InvoiceSystem.SystemGuids.BlockTypeGuids.ScheduledInvoiceDetail);
            RockMigrationHelper.UpdateBlockType("Invoice List", "", "~/Plugins/online_kevinrutledge/InvoiceSystem/ScheduledInvoiceList.ascx", "online_kevinrutledge > Invoice System", online.kevinrutledge.InvoiceSystem.SystemGuids.BlockTypeGuids.ScheduledInvoiceList);

            /* Create Invoice List Page */
            RockMigrationHelper.AddPage(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceSystemParentPage, online.kevinrutledge.InvoiceSystem.SystemGuids.SystemGuids.FullWidthLayout, "Scheduled Invoice List Page", "", online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.ScheduledInvoiceListPage);
            RockMigrationHelper.AddPageRoute(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.ScheduledInvoiceListPage, "scheduledinvoices");
            RockMigrationHelper.AddPageRoute(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.ScheduledInvoiceListPage, "scheduledinvoices/{InvoiceTypeId}");
            RockMigrationHelper.AddBlock(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.ScheduledInvoiceListPage, null, online.kevinrutledge.InvoiceSystem.SystemGuids.BlockTypeGuids.ScheduledInvoiceList, "Invoice List Page", "Main", "", "", 0, online.kevinrutledge.InvoiceSystem.SystemGuids.BlockGuids.ScheduledInvoiceListBlock);

            /* Create Invoice Detail Page */
            RockMigrationHelper.AddPage(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.ScheduledInvoiceListPage, online.kevinrutledge.InvoiceSystem.SystemGuids.SystemGuids.FullWidthLayout, "Scheduled Invoice Detail Page", "", online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.ScheduledInvoiceDetailPage, "fa fa-pencil");
            RockMigrationHelper.AddPageRoute(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.ScheduledInvoiceDetailPage, "scheduledinvoice");
            RockMigrationHelper.AddPageRoute(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.ScheduledInvoiceDetailPage, "scheduledinvoice/{SchedueldInvoiceId}");
            RockMigrationHelper.AddPageRoute(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.ScheduledInvoiceDetailPage, "scheduledinvoice/{InvoiceTypeId}/{ScheduledInvoiceId}");

            RockMigrationHelper.AddBlock(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.ScheduledInvoiceDetailPage, null, online.kevinrutledge.InvoiceSystem.SystemGuids.BlockTypeGuids.ScheduledInvoiceDetail, "Scheduled Invoice Detail Page", "Main", "", "", 0, online.kevinrutledge.InvoiceSystem.SystemGuids.BlockGuids.ScheduledInvoiceDetailBlock);

        }

        public override void Down()
        {
            Sql(
                @"
                
	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_ScheduledInvoice] Drop CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_ScheduledInvoice_CreatedByPersonAliasId] 
		        ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_ScheduledInvoice]  Drop CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_ScheduledInvoice_ModifiedByPersonAliasId] 
                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_ScheduledInvoice]  Drop  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_ScheduledInvoice_InvoiceTypeId]
                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_ScheduledInvoice] DROP CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_ScheduledInvoice_ScheduleInvoiceSheduleId]
				Drop Table  [dbo].[_online_kevinrutledge_InvoiceSystem_ScheduledInvoice]
"
            );

            

        }
    }
}