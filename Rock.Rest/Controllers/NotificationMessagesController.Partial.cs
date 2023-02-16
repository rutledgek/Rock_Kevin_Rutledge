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
using System.Data.Entity;
using System.Linq;
using System.Web.Http;

using Rock.Data;
using Rock.Enums.Core;
using Rock.Model;
using Rock.Net;
using Rock.Rest.Filters;
using Rock.ViewModels.Core;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    ///
    /// </summary>
    public partial class NotificationMessagesController
    {
        /// <summary>
        /// Gets the active notification messages for the currently logged in person.
        /// </summary>
        /// <param name="siteId">The identifier of the site this request is for.</param>
        /// <returns>A collection of <see cref="NotificationMessage"/> objects.</returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/NotificationMessages/MyMessages/Active" )]
        [Rock.SystemGuid.RestActionGuid( "dc9aee3e-93ed-48cd-9c9f-4b6217f198a2" )]
        public IHttpActionResult GetMyActiveMessages( string siteId = null )
        {
            var rockContext = ( RockContext ) Service.Context;
            var service = ( NotificationMessageService ) Service;
            var person = GetPerson( rockContext );

            if ( person == null )
            {
                return BadRequest( "Must be logged in to perform this action." );
            }

            if ( !TryGetSite( siteId, out var site ) )
            {
                return BadRequest( "Unable to determine site to use from request." );
            }

            var messages = service.GetActiveMessagesForPerson( person.Id, site )
                .OrderByDescending( nm => nm.MessageDateTime );

            return Ok( messages );
        }

        /// <summary>
        /// Gets the number that should be displayed in the badge for the
        /// currently logged in person.
        /// </summary>
        /// <param name="siteId">The identifier of the site this request is for.</param>
        /// <returns>An integer that represents the number to display in the badge.</returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/NotificationMessages/MyMessages/ActiveBadgeCount" )]
        [Rock.SystemGuid.RestActionGuid( "61be0d8f-0e4c-4ce1-b473-903b8bac9a5c" )]
        public IHttpActionResult GetMyActiveMessagesBadgeCount( string siteId = null )
        {
            var rockContext = ( RockContext ) Service.Context;
            var service = ( NotificationMessageService ) Service;
            var person = GetPerson( rockContext );

            if ( person == null )
            {
                return BadRequest( "Must be logged in to perform this action." );
            }

            if ( !TryGetSite( siteId, out var site ) )
            {
                return BadRequest( "Unable to determine site to use from request." );
            }

            var count = service.GetActiveMessagesForPerson( person.Id, site )
                .Sum( nm => nm.Count );

            return Ok( count );
        }

        /// <summary>
        /// Gets my message action to be performed for the specified action.
        /// </summary>
        /// <param name="id">The identifier of the notification message.</param>
        /// <param name="siteId">The site identifier.</param>
        /// <returns>An instance of <see cref="NotificationMessageActionBag"/> that represents the action to be performed.</returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/NotificationMessages/MyMessages/Action/{id}" )]
        [Rock.SystemGuid.RestActionGuid( "a3f89052-9cde-466c-861f-58d6df0c3966" )]
        public IHttpActionResult GetMyMessageAction( string id, string siteId = null )
        {
            var rockContext = ( RockContext ) Service.Context;
            var service = ( NotificationMessageService ) Service;
            var person = GetPerson( rockContext );

            if ( person == null )
            {
                return BadRequest( "Must be logged in to perform this action." );
            }

            if ( !TryGetSite( siteId, out var site ) )
            {
                return BadRequest( "Unable to determine site to use from request." );
            }

            var message = service.GetQueryableByKey( id, !site.DisablePredictableIds )
                .Include( nm => nm.PersonAlias )
                .FirstOrDefault();

            if ( message.PersonAlias.PersonId != person.Id )
            {
                return NotFound();
            }

            var messageType = NotificationMessageTypeCache.Get( message.NotificationMessageTypeId );

            if ( messageType?.Component == null )
            {
                return Ok( new NotificationMessageActionBag
                {
                    Type = NotificationMessageActionType.Invalid
                } );
            }

            return Ok( messageType.Component.GetActionForNotificationMessage( message, site, RockRequestContext ) );
        }

        /// <summary>
        /// Tries to get the <see cref="SiteCache"/> object. If the
        /// <paramref name="siteKey"/> parameter is specified then it will be
        /// used to load the site. Otherwise the domain in the request will
        /// be used automatically search for a site.
        /// </summary>
        /// <param name="siteKey">The site identifier, as an Id, Guid or IdKey value.</param>
        /// <param name="site">On return this will contain the <see cref="SiteCache"/> instance.</param>
        /// <returns><c>true</c> if a site was found, <c>false</c> otherwise.</returns>
        private bool TryGetSite( string siteKey, out SiteCache site )
        {
            site = SiteCache.GetSiteByDomain( RockRequestContext.RequestUri.Host );

            if ( siteKey.IsNotNullOrWhiteSpace() )
            {
                site = SiteCache.Get( siteKey, !( site?.DisablePredictableIds ?? true ) );
            }
            else
            {
                site = SiteCache.GetSiteByDomain( RockRequestContext.RequestUri.Host );
            }

            return site != null;
        }
    }
}