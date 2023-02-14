﻿// <copyright>
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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 165, "1.14.1" )]
    public class EnableTextToGiveMultiAcct : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Set "Enable Multi-Account" Block Attribute Value for the Text To Give Utility Payment Entry block.
            RockMigrationHelper.AddBlockAttributeValue(
                "9684D991-8B26-4D39-BAD5-B520F91D27B8", // Utility Payment Entry block instance for Text To Give.
                "BF331D8A-E872-4E4A-8A44-57121D8B6E63", // "Enable Multi-Account" Attribute for "Utility Payment Entry" BlockType.
                "False" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}
