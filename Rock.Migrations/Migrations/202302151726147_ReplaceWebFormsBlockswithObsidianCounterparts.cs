// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///
    /// </summary>
    public partial class ReplaceWebFormsBlockswithObsidianCounterparts : Rock.Migrations.RockMigration
    {
        private static readonly string JobClassName = "PostV15DataMigrationsReplaceWebFormsBlocksWithObsidianCounterparts";
        private static readonly string FullyQualifiedJobClassName = $"Rock.Jobs.{JobClassName}";
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // add ServiceJob: Rock Update Helper v15.0 - System Phone Numbers
            Sql( $@"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = '{FullyQualifiedJobClassName}' AND [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_150_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_COUNTERPARTS}' )
            BEGIN
               INSERT INTO [ServiceJob] (
                  [IsSystem]
                  ,[IsActive]
                  ,[Name]
                  ,[Description]
                  ,[Class]
                  ,[CronExpression]
                  ,[NotificationStatus]
                  ,[Guid] )
               VALUES ( 
                  0
                  ,1
                  ,'Rock Update Helper v15.0 - Replace WebForms Blocks with Obsidian Counterparts'
                  ,'This job will replace WebForms blocks with their Obsidian counterparts on all sites, pages, and layouts.'
                  ,'{FullyQualifiedJobClassName}'
                  ,'0 0 21 1/1 * ? *'
                  ,1
                  ,'{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_150_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_COUNTERPARTS}'
                  );
            END" );

            var serviceJobEntityTypeId = Rock.Web.Cache.EntityTypeCache.GetId( SystemGuid.EntityType.SERVICE_JOB.AsGuid() );

            // Attribute: Rock.Jobs.PostV15DataMigrationsReplaceWebFormsBlocksWithObsidianCounterparts: Command Timeout
            var commandTimeoutAttributeGuid = "F4C7151F-864A-4E36-8AF7-79D27DB41C07";
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", Rock.SystemGuid.FieldType.INTEGER, "Class", FullyQualifiedJobClassName, "Command Timeout", "Command Timeout", "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.", 0, "14000", commandTimeoutAttributeGuid, "CommandTimeout" );
            RockMigrationHelper.AddServiceJobAttributeValue( Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_150_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_COUNTERPARTS, commandTimeoutAttributeGuid, "14000" );

            // Attribute: Rock.Jobs.PostV15DataMigrationsReplaceWebFormsBlocksWithObsidianCounterparts: Block Type Guid Replacement Pairs
            var blockTypeGuidReplacementPairsAttributeGuid = "9431CD4D-A25A-4730-8724-5D107C6CDDA5";
            var blockTypeReplacements = new Dictionary<string, string>
            {
                // Login Block Type
                { "7B83D513-1178-429E-93FF-E76430E038E4", "5437C991-536D-4D9C-BE58-CBDB59D1BBB3" }
            };
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", Rock.SystemGuid.FieldType.KEY_VALUE_LIST, "Class", FullyQualifiedJobClassName, "Block Type Guid Replacement Pairs", "Block Type Guid Replacement Pairs", "The key-value pairs of replacement BlockType.Guid values, where the key is the existing BlockType.Guid and the value is the new BlockType.Guid. Blocks of BlockType.Guid == key will be replaced by blocks of BlockType.Guid == value in all sites, pages, and layouts.", 1, "", blockTypeGuidReplacementPairsAttributeGuid, "BlockTypeGuidReplacementPairs" );
            RockMigrationHelper.AddServiceJobAttributeValue( Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_150_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_COUNTERPARTS, blockTypeGuidReplacementPairsAttributeGuid, SerializeDictionary( blockTypeReplacements ) );

            // Attribute: Rock.Jobs.PostV15DataMigrationsReplaceWebFormsBlocksWithObsidianCounterparts: Should Keep Old Blocks
            var shouldKeepOldBlocksAttributeGuid = "A1B097B3-310B-445E-ADED-80AB1EFFCEC6";
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", Rock.SystemGuid.FieldType.BOOLEAN, "Class", FullyQualifiedJobClassName, "Should Keep Old Blocks", "Should Keep Old Blocks", "Determines if old blocks should be kept instead of being deleted. By default, old blocks will be deleted.", 2, "False", shouldKeepOldBlocksAttributeGuid, "ShouldKeepOldBlocks" );
            RockMigrationHelper.AddServiceJobAttributeValue( Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_150_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_COUNTERPARTS, shouldKeepOldBlocksAttributeGuid, "True" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( $"DELETE FROM [ServiceJob] WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_150_REPLACE_WEB_FORMS_BLOCKS_WITH_OBSIDIAN_COUNTERPARTS}'" );
        }

        private string SerializeDictionary( Dictionary<string, string> dictionary )
        {
            const string keyValueSeparator = "^";

            if ( dictionary?.Any() != true )
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            var first = dictionary.First();
            sb.Append( $"{first.Key}{keyValueSeparator}{first.Value}" );

            foreach ( var kvp in dictionary.Skip(1) )
            {
                sb.Append( $"|{kvp.Key}{keyValueSeparator}{kvp.Value}" );
            }

            return sb.ToString();
        }
    }
}
