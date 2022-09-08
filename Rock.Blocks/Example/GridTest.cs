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

using System.Collections.Generic;
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
                var prayerRequests = new PrayerRequestService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Take( count )
                    .ToList();
                sw.Stop();
                System.Diagnostics.Debug.WriteLine( $"Entity load took {sw.Elapsed}." );

                sw.Restart();
                Rock.Attribute.Helper.LoadFilteredAttributes( prayerRequests, rockContext, a => a.IsGridColumn );
                sw.Stop();
                System.Diagnostics.Debug.WriteLine( $"Attribute load took {sw.Elapsed}." );

                sw.Restart();
                var rows = prayerRequests
                    .Select( pr =>
                    {
                        var row = new Dictionary<string, object>
                        {
                            ["name"] = new { pr.FirstName, pr.LastName },
                            ["email"] = pr.Email,
                            ["enteredDateTime"] = pr.EnteredDateTime,
                            ["expirationDateTime"] = pr.ExpirationDate,
                            ["isUrgent"] = pr.IsUrgent,
                            ["isPublic"] = pr.IsPublic
                        };

                        foreach ( var attrValue in pr.AttributeValues )
                        {
                            row[$"attr_{attrValue.Key}"] = pr.GetAttributeCondensedHtmlValue( attrValue.Key );
                        }

                        return row;
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
