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
            Sql(@" CREATE TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType]( 
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[IsActive] bit NOT NULL Default 1,
	[InvoiceTerm] [nvarchar](100) Null, 
	[InvoiceItemTerm] [nvarchar](100) Null,
	[FinancialBatchPrefix nvarchar(100) Null,
	[IconCssClass] nvarchar(100) null,
	[DefaultCommunicationTemplate] nvarchar(max) null,
	[LateInvoiceCommunicationTemplate] nvarchar(max) null,
	[GlobalDaysUntilLate] int null,
	[GlobalLateFeeAmount] [decimal](18, 2) NULL,
	[GlobalLateFeePercentage] [decimal](5, 2) NULL,
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

	ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_ModifiedByPersonAliasId]");


            RockMigrationHelper.UpdateEntityType("online.kevinrutledge.InvoiceSystem.Model.InvoiceType", online.kevinrutledge.InvoiceSystem.SystemGuids.EntityTypeGuids.Invoice_Type, true, true);

        }

        public override void Down()
        {
            Sql(@"
                -- Drop foreign key constraints
                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] 
                    DROP CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_CreatedByPersonAliasId];

                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] 
                    DROP CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceType_ModifiedByPersonAliasId];

                -- Drop primary key constraint
                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType] 
                    DROP CONSTRAINT [PK__online_kevinrutledge_InvoiceSystem_InvoiceType];

                -- Drop the table
                DROP TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceType];"
            );
            RockMigrationHelper.DeleteEntityType(online.kevinrutledge.InvoiceSystem.SystemGuids.EntityTypeGuids.Invoice_Type);
        }
    }
}