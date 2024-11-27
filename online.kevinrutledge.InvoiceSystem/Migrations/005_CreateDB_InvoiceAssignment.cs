using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace online.kevinrutledge.InvoiceSystem.Migrations
{
    [MigrationNumber(5, "1.15.0")]
    public class CreateDB_InvoiceAssignment : Migration
    {
        public override void Up()
        {
            Sql(
                @"
                CREATE TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceAssignment]( 
	            [Id] [int] IDENTITY(1,1) NOT NULL,
				[InvoiceId] [int] Not Null,
				[AuthorizedPersonAliasId] [int] Not Null,
				[AssignedPercent] [decimal](18,2) Not Null Default 1,
	            [Guid] [uniqueidentifier] NOT NULL,
	            [CreatedDateTime] [datetime] NULL,
	            [ModifiedDateTime] [datetime] NULL,
	            [CreatedByPersonAliasId] [int] NULL,
	            [ModifiedByPersonAliasId] [int] NULL,
	            [ForeignKey] [nvarchar](50) NULL,
	            [ForeignGuid] [uniqueidentifier] NULL,
	            [ForeignId] [int] NULL,
	            CONSTRAINT [PK__online_kevinrutledge_InvoiceSystem_InvoiceAssignment] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]
 
	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceAssignment]  WITH CHECK ADD  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceAssignment_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
		            REFERENCES [dbo].[PersonAlias] ([Id])

	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceAssignment] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceAssignment_CreatedByPersonAliasId]

	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceAssignment]  WITH CHECK ADD  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceAssignment_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
		            REFERENCES [dbo].[PersonAlias] ([Id])

	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceAssignment] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceAssignment_ModifiedByPersonAliasId]

				ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceAssignment]  WITH CHECK ADD  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceAssignment_AuthorizedPersonAliasId] FOREIGN KEY([AuthorizedPersonAliasId])
		            REFERENCES [dbo].[PersonAlias] ([Id])

	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceAssignment] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceAssignment_AuthorizedPersonAliasId]


                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceAssignment]  WITH CHECK ADD  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceAssignment_InvoiceId] FOREIGN KEY([InvoiceId])
		            REFERENCES [dbo].[_online_kevinrutledge_InvoiceSystem_Invoice] ([Id])

	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceAssignment] CHECK CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceAssignment_InvoiceId]

                ");

            /* Update the EntityType */
            RockMigrationHelper.UpdateEntityType("online.kevinrutledge.InvoiceSystem.Model.InvoiceAssignment", online.kevinrutledge.InvoiceSystem.SystemGuids.EntityTypeGuids.Invoice_Assignment, true, true);

        }

        public override void Down()
        {
            Sql(
               @"
                
	            ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceAssignment] Drop CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceAssignment_CreatedByPersonAliasId] 
                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceAssignment]  Drop CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceAssignment_ModifiedByPersonAliasId] 
                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceAssignment]  Drop  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceAssignment_AuthorizedPersonAliasId]
                ALTER TABLE [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceAssignment]  Drop  CONSTRAINT [FK__online_kevinrutledge_InvoiceSystem_InvoiceAssignment_InvoiceId]
	            Drop Table  [dbo].[_online_kevinrutledge_InvoiceSystem_InvoiceAssignment]
"
           );


        }
    }


}
