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
using System.Linq;
using System.Linq.Expressions;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.NotificationMessage"/> entity objects.
    /// </summary>
    public partial class NotificationMessageService : Service<NotificationMessage>
    {
        /// <summary>
        /// Gets the active messages for the person specified.
        /// A message is considered active as long as it is within the proper
        /// date range. Unread messages are still considered active and will be
        /// included.
        /// </summary>
        /// <param name="personId">The person identifier to use when filtering messages.</param>
        /// <param name="site">The site the messages will be displayed on.</param>
        /// <returns>A <see cref="IQueryable"/> of <see cref="NotificationMessage"/> objects that match the parameters.</returns>
        public IQueryable<NotificationMessage> GetActiveMessagesForPerson( int personId, SiteCache site )
        {
            if ( site == null )
            {
                throw new ArgumentNullException( nameof( site ) );
            }

            return GetMessagesForPerson( nm => nm.PersonAlias.PersonId == personId, site );
        }

        /// <summary>
        /// Gets the active messages for the person specified.
        /// A message is considered active as long as it is within the proper
        /// date range. Unread messages are still considered active and will be
        /// included.
        /// </summary>
        /// <param name="personGuid">The person unique identifier to use when filtering messages.</param>
        /// <param name="site">The site the messages will be displayed on.</param>
        /// <returns>A <see cref="IQueryable"/> of <see cref="NotificationMessage"/> objects that match the parameters.</returns>
        public IQueryable<NotificationMessage> GetActiveMessagesForPerson( Guid personGuid, SiteCache site )
        {
            if ( site == null )
            {
                throw new ArgumentNullException( nameof( site ) );
            }

            return GetMessagesForPerson( nm => nm.PersonAlias.Person.Guid == personGuid, site );
        }

        /// <summary>
        /// Gets the active messages for the person specified by the predicate.
        /// A message is considered active as long as it is within the proper
        /// date range. Unread messages are still considered active and will be
        /// included.
        /// </summary>
        /// <param name="personPredicate">The person predicate to use when filtering messages.</param>
        /// <param name="site">The site the messages will be displayed on.</param>
        /// <returns>A <see cref="IQueryable"/> of <see cref="NotificationMessage"/> objects that match the parameters.</returns>
        private IQueryable<NotificationMessage> GetActiveMessagesForPerson( Expression<Func<NotificationMessage, bool>> personPredicate, SiteCache site )
        {
            var now = RockDateTime.Now;

            return GetMessagesForPerson( personPredicate, site )
                .Where( nm => nm.MessageDateTime <= now && nm.ExpireDateTime > now );
        }

        /// <summary>
        /// Gets the messages for the person specified by the predicate.
        /// </summary>
        /// <param name="personPredicate">The person predicate to use when filtering messages.</param>
        /// <param name="site">The site the messages will be displayed on.</param>
        /// <returns>A <see cref="IQueryable"/> of <see cref="NotificationMessage"/> objects that match the parameters.</returns>
        private IQueryable<NotificationMessage> GetMessagesForPerson( Expression<Func<NotificationMessage, bool>> personPredicate, SiteCache site )
        {
            var now = RockDateTime.Now;

            // Filter to messages for this person.
            var qry = Queryable().Where( personPredicate );

            if ( site.SiteType == SiteType.Web )
            {
                // Filter to messages that support web and are either not
                // limited to a single site or match this site.
                qry = qry.Where( nm => nm.NotificationMessageType.IsWebSupported
                    && ( !nm.NotificationMessageType.RelatedWebSiteId.HasValue || nm.NotificationMessageType.RelatedWebSiteId == site.Id ) );
            }
            else if ( site.SiteType == SiteType.Mobile )
            {
                // Filter to messages that support mobile and are either not
                // limited to a single site or match this site.
                qry = qry.Where( nm => nm.NotificationMessageType.IsMobileApplicationSupported
                    && ( !nm.NotificationMessageType.RelatedMobileApplicationSiteId.HasValue || nm.NotificationMessageType.RelatedMobileApplicationSiteId == site.Id ) );
            }
            else if ( site.SiteType == SiteType.Tv )
            {
                // Filter to messages that support TV and are either not
                // limited to a single site or match this site.
                qry = qry.Where( nm => nm.NotificationMessageType.IsTvApplicationSupported
                    && ( !nm.NotificationMessageType.RelatedTvApplicationSiteId.HasValue || nm.NotificationMessageType.RelatedTvApplicationSiteId == site.Id ) );
            }
            else
            {
                throw new Exception( $"The site type '{site.SiteType}' is not supported by notification messages." );
            }

            return qry;
        }
    }
}
