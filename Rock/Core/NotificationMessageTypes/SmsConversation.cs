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
using System.ComponentModel;
using System.ComponentModel.Composition;

using Rock.Enums.Core;
using Rock.Model;
using Rock.Net;
using Rock.ViewModels.Core;
using Rock.Web.Cache;

namespace Rock.Core.NotificationMessageTypes
{
    /// <summary>
    /// Displays notification messages about unread SMS conversations.
    /// </summary>
    [Description( "Displays notification messages about unread SMS conversations." )]
    [Export( typeof( NotificationMessageTypeComponent ) )]
    [ExportMetadata( "ComponentName", "SMS Conversation" )]

    [Rock.SystemGuid.EntityTypeGuid( "A6BC9CB4-9AA8-4D9C-90C8-0131A9ED3A3F" )]
    internal class SmsConversation : NotificationMessageTypeComponent
    {
        /// <inheritdoc/>
        public override NotificationMessageActionBag GetActionForNotificationMessage( NotificationMessage message, SiteCache site, RockRequestContext context )
        {
            var siteTerm = site.SiteType == SiteType.Web ? "web site" : "application";
            int? conversationPageId = 12; // = site.ConversationPageId
            var messageData = message.ComponentDataJson.FromJsonOrNull<MessageData>();
            var conversationPage = conversationPageId.HasValue ? PageCache.Get( conversationPageId.Value ) : null;

            if ( conversationPage == null )
            {
                return new NotificationMessageActionBag
                {
                    Type = NotificationMessageActionType.ShowMessage,
                    Message = $"This {siteTerm} has not been configured to SMS conversations."
                };
            }

            if ( messageData == null )
            {
                return new NotificationMessageActionBag
                {
                    Type = NotificationMessageActionType.Invalid
                };
            }

            if ( site.SiteType == SiteType.Web )
            {
                return new NotificationMessageActionBag
                {
                    Type = NotificationMessageActionType.LinkToPage,
                    Url = $"{context.RootUrlPath}/page/{conversationPage.Id}?PhoneNumberId={messageData.PhoneNumberId}&PersonId={messageData.PersonId}"
                };
            }
            else
            {
                return new NotificationMessageActionBag
                {
                    Type = NotificationMessageActionType.LinkToPage,
                    Url = $"{conversationPage.Guid}?PhoneNumberGuid={messageData.PhoneNumberGuid}&PersonGuid={messageData.PersonGuid}"
                };
            }
        }

        /// <inheritdoc/>
        public override int DeleteObsoleteNotificationMessageTypes( int commandTimeout )
        {
            return 0;
        }

        /// <inheritdoc/>
        public override int DeleteObsoleteNotificationMessages( int commandTimeout )
        {
            return 0;
        }

        /// <summary>
        /// Provides the data stored on individual notification messages used
        /// by this component.
        /// </summary>
        private class MessageData
        {
            /// <summary>
            /// Gets or sets the Rock system phone number identifier.
            /// </summary>
            /// <value>The Rock system phone number identifier.</value>
            public int PhoneNumberId { get; set; }

            /// <summary>
            /// Gets or sets the Rock system phone number unique identifier.
            /// </summary>
            /// <value>The Rock system phone number unique identifier.</value>
            public Guid PhoneNumberGuid { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the person that sent the messages.
            /// </summary>
            /// <value>The person identifier.</value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the unique identifier of the person that sent the messages.
            /// </summary>
            /// <value>The person unique identifier.</value>
            public Guid PersonGuid { get; set; }
        }
    }
}
