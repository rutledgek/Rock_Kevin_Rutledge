using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace online.kevinrutledge.InvoiceSystem.Migrations
{
    [MigrationNumber(1, "1.15.0")]
    public class CreateDB_InvoiceType : Migration
    {
        public override void Up()
        {
            Sql(
                @" CREATE TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType]( 
	            [Id] [int] IDENTITY(1,1) NOT NULL,
	            [Name] [nvarchar](100) NOT NULL,
	            [Description] [nvarchar](max) NULL,
	            [IsActive] bit NOT NULL Default 1,
	            [InvoiceTerm] [nvarchar](100) Null, 
	            [InvoiceItemTerm] [nvarchar](100) Null,
	            [IconCssClass] nvarchar(100) null,
                [DefaultFinancialAccountId] [int] Null,
                [DefaultTaxRate] [decimal](18, 2) NULL Default 0,
	            [DefaultDaysUntilLate] int Null,
	            [DefaultLateFeeAmount] [decimal](18, 2) Not Null default 0,
	            [DefaultLateFeePercent] [decimal](5, 2) Not NULL Default 0,
                [CategoryId] [int] NULL,
                [InvoiceFromPersonAliasId] [int] NULL,
                [InvoiceFromName] [nvarchar](max) NULL,
                [InvoiceFromEmail] [nvarchar](max) NULL,
                [InvoiceSubject] [nvarchar](max) NULL,
	            [InvoiceCommunicationTemplate] nvarchar(max) null,
                [InvoiceSystemCommunicationId] int null,
                [LateNoticeFromPersonAliasId] [int] NULL,
                [LateNoticeFromName] [nvarchar](max) NULL,
                [LateNoticeFromEmail] [nvarchar](max) NULL,
	            [LateNoticeSubject] [nvarchar](max) NULL,
                [LateNoticeCommunicationTemplate] nvarchar(max) null,
                [LateNoticeSystemCommunicationId] int null,
                [PaymentPageId] int null,
	            [Guid] [uniqueidentifier] NOT NULL,
	            [CreatedDateTime] [datetime] NULL,
	            [ModifiedDateTime] [datetime] NULL,
	            [CreatedByPersonAliasId] [int] NULL,
	            [ModifiedByPersonAliasId] [int] NULL,
	            [ForeignKey] [nvarchar](50) NULL,
	            [ForeignGuid] [uniqueidentifier] NULL,
	            [ForeignId] [int] NULL,
	            CONSTRAINT [PK__online_kevinrutledge_InvoiceSystem_InvoiceType] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
 
	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType]  WITH CHECK ADD  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
		            REFERENCES [dbo].[PersonAlias] ([Id])

	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_CreatedByPersonAliasId]

	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType]  WITH CHECK ADD  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
		            REFERENCES [dbo].[PersonAlias] ([Id])

	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_ModifiedByPersonAliasId]


                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType]  WITH CHECK ADD  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_DefaultFinancialAccountId] FOREIGN KEY([DefaultFinancialAccountId])
		            REFERENCES [dbo].[FinancialAccount] ([Id])

	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_DefaultFinancialAccountId]


                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType]  WITH CHECK ADD  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_InvoiceSystemCommunicationId] FOREIGN KEY([InvoiceSystemCommunicationid])
		            REFERENCES [dbo].[SystemCommunication] ([Id])

	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_InvoiceSystemCommunicationId]

                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType]  WITH CHECK ADD  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_LateNoticeSystemCommunicationId] FOREIGN KEY([LateNoticeSystemCommunicationid])
		            REFERENCES [dbo].[SystemCommunication] ([Id])

	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_LateNoticeSystemCommunicationId]



                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType]  WITH CHECK ADD  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_InvoiceFromPersonAliasId] FOREIGN KEY([InvoiceFromPersonAliasId])
		                            REFERENCES [dbo].[PersonAlias] ([Id])

	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_InvoiceFromPersonAliasId]



                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType]  WITH CHECK ADD  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_LateNoticeFromPersonAliasId] FOREIGN KEY([LateNoticeFromPersonAliasId])
		                            REFERENCES [dbo].[PersonAlias] ([Id])

	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_LateNoticeFromPersonAliasId]


                Alter Table [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] WITH CHECK ADD CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_PaymentPageId] Foreign Key([PaymentPageId])
                                    References [dbo].[Page] ([Id])
                
                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_PaymentPageId]

                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType]  WITH CHECK ADD  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_CategoryId] FOREIGN KEY([CategoryId])
		            REFERENCES [dbo].[Category] ([Id])

	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_CategoryId]


               --Insert Sample Data
                    INSERT INTO [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] 
                    ([Name], [Description], [IsActive], [InvoiceTerm], [InvoiceItemTerm], [IconCssClass], 
                    [DefaultFinancialAccountId], [DefaultTaxRate], [DefaultDaysUntilLate], [DefaultLateFeeAmount], 
                    [DefaultLateFeePercent], [CategoryId], [InvoiceFromPersonAliasId], [InvoiceFromName], 
                    [InvoiceFromEmail], [InvoiceSubject], [InvoiceCommunicationTemplate], 
                    [InvoiceSystemCommunicationId], [LateNoticeFromPersonAliasId], [LateNoticeFromName], 
                    [LateNoticeFromEmail], [LateNoticeSubject], [LateNoticeCommunicationTemplate], 
                    [LateNoticeSystemCommunicationId], [Guid], [CreatedDateTime], [ModifiedDateTime], 
                    [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [ForeignGuid], [ForeignId])
                    VALUES ('Test Type', 'Test', 1, 'Childcare Bill', 'Child', '', 1, 0.06, 5, 0.00, 0.00, NULL, NULL, '', '', '', '', NULL, NULL, '', '', '', '', NULL, NEWID(), '2024-11-29 10:42:52', '2024-11-29 10:42:52', 10, 10, 'SampleData', NULL, NULL)
            "
            );


            /* Update the BlockType */
            RockMigrationHelper.UpdateBlockType("Invoice Type List", "", "~/Plugins/online_kevinrutledge/InvoiceSystem/InvoiceTypeList.ascx", "online_kevinrutledge > Invoice System", online.kevinrutledge.InvoiceSystem.SystemGuids.BlockTypeGuids.InvoiceTypeList);
            RockMigrationHelper.UpdateBlockType("Invoice Type Detail","", "~/Plugins/online_kevinrutledge/InvoiceSystem/InvoiceTypeDetail.ascx","online_kevinrutledge > Invoice System",online.kevinrutledge.InvoiceSystem.SystemGuids.BlockTypeGuids.InvoiceTypeDetail);
            

            /* Update the EntityType */
            RockMigrationHelper.UpdateEntityType("online.kevinrutledge.InvoiceSystem.Model.InvoiceType", online.kevinrutledge.InvoiceSystem.SystemGuids.EntityTypeGuids.Invoice_Type, true, true);
            

            /* Create Invoice System Page and add Page Menu Block */
            RockMigrationHelper.AddPage(online.kevinrutledge.InvoiceSystem.SystemGuids.SystemGuids.InstalledPluginsPage, online.kevinrutledge.InvoiceSystem.SystemGuids.SystemGuids.FullWidthLayout, "Invoice System","" , online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceSystemParentPage, "fa fa-file-list");
            RockMigrationHelper.AddBlock(true, online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceSystemParentPage, null, online.kevinrutledge.InvoiceSystem.SystemGuids.SystemGuids.PageMenuBlockType, "Page Menu", "Main", "", "", 0, "64734687-27D4-4F69-8621-411B9FADFDFB");
            RockMigrationHelper.AddBlockAttributeValue("64734687-27D4-4F69-8621-411B9FADFDFB", online.kevinrutledge.InvoiceSystem.SystemGuids.SystemGuids.PageMenuTemplateContent, online.kevinrutledge.InvoiceSystem.SystemGuids.AttributeContent.SubMenuContent, false);

            /* Create Invoice Type List Page and Add the Invoice Type List Block*/
            RockMigrationHelper.AddPage(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceSystemParentPage, online.kevinrutledge.InvoiceSystem.SystemGuids.SystemGuids.FullWidthLayout, "Invoice Type List Page", "", online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceTypeListPage, "fa fa-file-list-o");
            RockMigrationHelper.AddBlock(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceTypeListPage, null,online.kevinrutledge.InvoiceSystem.SystemGuids.BlockTypeGuids.InvoiceTypeList, "Invoice Type List", "Main", "", "", 0, online.kevinrutledge.InvoiceSystem.SystemGuids.BlockGuids.InvoiceTypeListBlock);

            /* Create Invoice Type Detail Page */
            RockMigrationHelper.AddPage(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceTypeListPage, online.kevinrutledge.InvoiceSystem.SystemGuids.SystemGuids.FullWidthLayout, "Invoice Type Detail Page", "Detail of an Invoice Type.", online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceTypeDetailPage, "fa fa-file-list-o");
            RockMigrationHelper.AddBlock(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceTypeDetailPage, null, online.kevinrutledge.InvoiceSystem.SystemGuids.BlockTypeGuids.InvoiceTypeDetail, "Invoice Type Detail", "Main", "", "", 0, online.kevinrutledge.InvoiceSystem.SystemGuids.BlockGuids.InvoiceTypeDetailBlock);


        }

        public override void Down()
        {
            Sql(
                @"
                -- Drop foreign key constraints
                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] 
                    DROP CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_CreatedByPersonAliasId];

                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] 
                    DROP CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_ModifiedByPersonAliasId];

                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] 
                    DROP CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_DefaultFinancialAccountId]

                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] 
                    DROP CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_InvoiceSystemCommunicationId]

                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] 
                    DROP CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_LateNoticeSystemCommunicationId]




                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] 
                    DROP CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_LateNoticeFromPersonAliasId]



                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] 
                    DROP CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_InvoiceFromPersonAliasId]


                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType]
                    DROP CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_PaymentPageId]

                -- Drop primary key constraint
                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] 
                    DROP CONSTRAINT [PK__online_kevinrutledge_InvoiceSystem_InvoiceType];

    
                
                -- Drop the table
                DROP TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType];"
            );

            RockMigrationHelper.DeleteEntityType(online.kevinrutledge.InvoiceSystem.SystemGuids.EntityTypeGuids.Invoice_Type);

            RockMigrationHelper.DeleteBlockType(online.kevinrutledge.InvoiceSystem.SystemGuids.BlockTypeGuids.InvoiceTypeList);
            RockMigrationHelper.DeletePage(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceTypeDetailPage);
            RockMigrationHelper.DeletePage(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceTypeListPage);
            RockMigrationHelper.DeletePage(online.kevinrutledge.InvoiceSystem.SystemGuids.PageGuids.InvoiceSystemParentPage);
        }
    }
}