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
using System;
using System.Web.Http;
using Rock.Data;
using Rock.Rest.Filters;
using Rock.Web.Cache;
using System.Linq;
using System.Data.Entity;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// The Entity Sets Controller.
    /// </summary>
    public partial class EntitySetsController
    {
        /// <summary>
        /// Posts the entity set from unique identifier.
        /// </summary>
        /// <param name="entityItemGuids">The entity item guids.</param>
        /// <param name="entityTypeGuid">The entity type unique identifier.</param>
        /// <returns>IHttpActionResult.</returns>
        [System.Web.Http.Route( "api/EntitySets/CreateFromItems/{entityTypeGuid:guid}" )]
        [HttpPost]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "50B248D7-C52A-4698-B4AF-C9DE305394EC" )]
        public IHttpActionResult PostEntitySetFromGuid( [FromBody] List<Guid> entityItemGuids, Guid entityTypeGuid )
        {
            var entityType = EntityTypeCache.Get( entityTypeGuid );

            if( entityType == null )
            {
                return BadRequest("Invalid EntityType.");
            }

            using( var rockContext = new RockContext() )
            {
                // Dynamically get the IService for the entity type and
                // then get a queryable to load them.
                var entityService = Rock.Reflection.GetServiceForEntityType( entityType.GetEntityType(), rockContext );
                var asQueryableMethod = entityService?.GetType()
                    .GetMethod( "Queryable", Array.Empty<Type>() );

                // Must not really be an IEntity type...
                if ( asQueryableMethod == null )
                {
                    return BadRequest("Unsupported EntityType.");
                }

                var entityQry = ( IQueryable<IEntity> ) asQueryableMethod?.Invoke( entityService, Array.Empty<object>() );

                var entityIds = new List<int>();
                while ( entityItemGuids.Any() )
                {
                    // Work with at most 1,000 records at a time since it
                    // translates to an IN query which doesn't perform well
                    // on large sets.
                    var guidsToProcess = entityItemGuids.Take( 1_000 ).ToList();
                    entityItemGuids = entityItemGuids.Skip( 1_000 ).ToList();

                    // Load all entities from the Guids.
                    var ids = entityQry
                        .AsNoTracking()
                        .Where( e => guidsToProcess.Contains( e.Guid ) )
                        .Select( e => e.Id )
                        .ToList();

                    entityIds.AddRange( ids );
                }

                var entitySetId = CreateEntitySet( entityIds, entityType.Id );
                if ( !entitySetId.HasValue )
                {
                    return InternalServerError();
                }

                return Ok( entitySetId.Value.Guid );
            }
        }

        /// <summary>
        /// Posts the entity set from int.
        /// </summary>
        /// <param name="entityItemIds">The entity item ids.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <returns>System.Web.Http.IHttpActionResult.</returns>
        [System.Web.Http.Route( "api/EntitySets/CreateFromItems/{entityTypeId:int}" )]
        [HttpPost]
        [Authenticate]
        [Rock.SystemGuid.RestActionGuid( "32374DFE-6478-41A5-AE7D-43DD58DC6176" )]
        public IHttpActionResult PostEntitySetFromInt( [FromBody] List<int> entityItemIds, int entityTypeId )
        {
            var entitySetId = CreateEntitySet( entityItemIds, entityTypeId );
            if ( !entitySetId.HasValue )
            {
                return InternalServerError();
            }

            return Ok( entitySetId.Value.Id );
        }

        /// <summary>
        /// Creates the entity set.
        /// </summary>
        /// <param name="entityItemIds">The entity item ids.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>System.Int32.</returns>
        private static (int Id, Guid Guid)? CreateEntitySet( List<int> entityItemIds, int entityTypeId, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            var entitySet = new Rock.Model.EntitySet();
            entitySet.EntityTypeId = entityTypeId;
            entitySet.ExpireDateTime = RockDateTime.Now.AddMinutes( 5 );
            List<Rock.Model.EntitySetItem> entitySetItems = new List<Rock.Model.EntitySetItem>();

            foreach ( var entityItemId in entityItemIds )
            {
                try
                {
                    var item = new Rock.Model.EntitySetItem();
                    item.EntityId = ( int ) entityItemId;
                    entitySetItems.Add( item );
                }
                catch
                {
                    // ignore
                }
            }

            if ( entitySetItems.Any() )
            {
                var service = new Rock.Model.EntitySetService( rockContext );
                service.Add( entitySet );
                rockContext.SaveChanges();
                entitySetItems.ForEach( a =>
                {
                    a.EntitySetId = entitySet.Id;
                } );

                rockContext.BulkInsert( entitySetItems );

                return (entitySet.Id, entitySet.Guid);
            }

            return null;
        }
    }
}