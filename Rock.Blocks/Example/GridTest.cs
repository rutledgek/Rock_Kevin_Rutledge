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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

using Rock.Attribute;
using Rock.Blocks.Types.Mobile.Prayer;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

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
        public override string BlockFileUrl => $"{base.BlockFileUrl}.vue";

        public override object GetObsidianBlockInitialization()
        {
            return new
            {
                GridDefinition = GetGridBuilder( GetGridAttributes() ).BuildDefinition()
            };
        }

        [BlockAction]
        public BlockActionResult GetGridData()
        {
            using ( var rockContext = new RockContext() )
            {
                var count = RequestContext.GetPageParameter( "count" )?.AsIntegerOrNull() ?? 10_000;

                var gridAttributes = GetGridAttributes();
                var gridAttributeIds = gridAttributes.Select( a => a.Id ).ToList();

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var prayerRequests = new PrayerRequestService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Take( count )
                    .ToList();
                sw.Stop();
                System.Diagnostics.Debug.WriteLine( $"Entity load took {sw.Elapsed.TotalMilliseconds}ms." );

                sw.Restart();
                Helper.LoadFilteredAttributes( prayerRequests, rockContext, a => gridAttributeIds.Contains( a.Id ) );
                sw.Stop();
                System.Diagnostics.Debug.WriteLine( $"Attribute load took {sw.Elapsed.TotalMilliseconds}ms." );

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
                System.Diagnostics.Debug.WriteLine( $"Manual row translation took {sw.Elapsed.TotalMilliseconds}ms." );

                sw.Restart();
                rows = GetGridBuilder( gridAttributes ).BuildRows( prayerRequests );

                sw.Stop();
                System.Diagnostics.Debug.WriteLine( $"Row translation took {sw.Elapsed.TotalMilliseconds}ms." );

                var definition = GetGridBuilder( gridAttributes ).BuildDefinition();

                var gridData = new
                {
                    Rows = rows
                };

                return ActionOk( gridData );
            }
        }

        private List<AttributeCache> GetGridAttributes()
        {
            var entityTypeId = EntityTypeCache.GetId<PrayerRequest>().Value;

            return AttributeCache.GetByEntityTypeQualifier( entityTypeId, string.Empty, string.Empty, false )
                .Where( a => a.IsGridColumn )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
        }

        private GridBuilder<PrayerRequest> GetGridBuilder( List<AttributeCache> gridAttributes )
        {
            return new GridBuilder<PrayerRequest>()
                .AddField( "name", pr => new { pr.FirstName, pr.LastName } )
                .AddTextField( "email", pr => pr.Email )
                .AddDateTimeField( "enteredDateTime", pr => pr.EnteredDateTime )
                .AddDateTimeField( "expirationDateTime", pr => pr.ExpirationDate )
                .AddField( "isUrgent", pr => pr.IsUrgent )
                .AddField( "isPublic", pr => pr.IsPublic )
                .AddAttributeFields( gridAttributes );
        }
    }

    public class GridBuilder<T>
    {
        private readonly Dictionary<string, Func<T, object>> _fieldActions = new Dictionary<string, Func<T, object>>();

        private readonly List<Action<GridDefinition>> _definitionActions = new List<Action<GridDefinition>>();

        public GridBuilder()
        {
        }

        public GridBuilder<T> AddPersonField( string name, Func<T, Person> valueExpression )
        {
            return AddField( name, row =>
            {
                var person = valueExpression( row );

                if ( person == null )
                {
                    return null;
                }

                return new
                {
                    person.FirstName,
                    person.NickName,
                    person.LastName,
                    person.PhotoUrl
                };
            } );
        }

        public GridBuilder<T> AddDateTimeField( string name, Func<T, DateTime?> valueExpression )
        {
            return AddField( name, row => valueExpression( row )?.ToRockDateTimeOffset() );
        }

        public GridBuilder<T> AddTextField( string name, Func<T, string> valueExpression )
        {
            return AddField( name, row => valueExpression( row ) );
        }

        public GridBuilder<T> AddAttributeFields( IEnumerable<AttributeCache> attributes )
        {
            if ( !typeof( IHasAttributes ).IsAssignableFrom( typeof( T ) ) )
            {
                throw new Exception( $"The type '{typeof( T ).FullName}' does not support attributes." );
            }

            foreach ( var attribute in attributes )
            {
                var key = attribute.Key;
                var fieldKey = $"attr_{key}";

                AddField( fieldKey, item =>
                {
                    var attributeRow = item as IHasAttributes;

                    return attributeRow.GetAttributeCondensedHtmlValue( key );
                } );

                _definitionActions.Add( definition =>
                {
                    definition.AttributeColumns.Add( new AttributeColumnDefinition
                    {
                        Name = fieldKey,
                        Title = attribute.Name
                    } );
                } );
            }

            return this;
        }

        public GridBuilder<T> AddField( string name, Func<T, object> valueExpression )
        {
            if ( _fieldActions.ContainsKey( name ) )
            {
            }

            _fieldActions.Add( name, valueExpression );

            return this;
        }

        public List<Dictionary<string, object>> BuildRows( IEnumerable<T> items )
        {
            var fieldKeys = _fieldActions.Keys.ToArray();

            return items
                .Select( item =>
                {
                    var row = new Dictionary<string, object>();

                    for ( int i = 0; i < fieldKeys.Length; i++ )
                    {
                        var key = fieldKeys[i];
                        var value = _fieldActions[key]( item );

                        row[key] = value;
                    }

                    return row;
                } )
                .ToList();
        }

        public GridDefinition BuildDefinition()
        {
            var definition = new GridDefinition
            {
                AttributeColumns = new List<AttributeColumnDefinition>()
            };

            foreach ( var action in _definitionActions )
            {
                action( definition );
            }

            return definition;
        }
    }

    public class GridDefinition
    {
        public List<AttributeColumnDefinition> AttributeColumns { get; set; }
    }

    public class AttributeColumnDefinition
    {
        public string Name { get; set; }

        public string Title { get; set; }
    }

    public class GridData
    {
        public List<Dictionary<string, object>> Rows { get; set; }
    }
}
