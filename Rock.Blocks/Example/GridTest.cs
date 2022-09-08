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

using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Blocks.Example
{
    /// <summary>
    /// Allows testing the new Grid component.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianBlockType" />

    [DisplayName( "Grid Test" )]
    [Category( "Example" )]
    [Description( "Allows testing the new Grid component." )]
    [IconCssClass( "fa fa-list" )]

    [Rock.SystemGuid.EntityTypeGuid( "1934a378-57d6-44d0-b7cd-4443f347a1ee" )]
    public class GridTest : RockObsidianBlockType
    {
        public override object GetObsidianBlockInitialization()
        {
            return new {
            };
        }

        [BlockAction]
        public BlockActionResult GetGridData()
        {
            using ( var rockContext = new RockContext() )
            {
                var count = RequestContext.GetPageParameter( "count" )?.AsIntegerOrNull() ?? 10_000;

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var groups = new GroupService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( g => g.GroupTypeId == 38 )
                    .OrderBy( g => g.Id )
                    .Take( count )
                    .ToList();
                sw.Stop();
                System.Diagnostics.Debug.WriteLine( $"Entity load took {sw.Elapsed}." );

                sw.Restart();
                Rock.Attribute.Helper.LoadFilteredAttributes( groups, rockContext, a => a.IsGridColumn );
                sw.Stop();
                System.Diagnostics.Debug.WriteLine( $"Attribute load took {sw.Elapsed}." );

                sw.Restart();
                var rows = groups
                    .Select( g => new
                    {
                        g.Name,
                        g.Description,
                        Attr_Group1 = new
                        {
                            Guid = g.GetAttributeValue( "Group1" ),
                            Text = g.GetAttributeCondensedTextValue( "Group1" )
                        },
                        Attr_CheckList = g.GetAttributeCondensedHtmlValue( "CheckList" )
                    } )
                    .ToList();
                sw.Stop();
                System.Diagnostics.Debug.WriteLine( $"Row translation took {sw.Elapsed}." );

                var gridData = new
                {
                    Rows = rows
                };

                return ActionOk( gridData );
            }
        }
    }
}
