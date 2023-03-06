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
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks.Groups.GroupAttendanceDetail;

namespace Rock.RealTime.Topics
{
    [RealTimeTopic]
    public class GroupAttendanceTopic : Topic<IGroupAttendance>
    {
        public async Task MarkAttendance( GroupAttendanceDetailMarkAttendanceRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var attendanceOccurrence = GetAuthorizedAttendanceOccurrence( rockContext, bag.AttendanceOccurrenceGuid );

                var person = attendanceOccurrence.Attendees.FirstOrDefault( a => a.PersonAlias.Person.Guid == bag.PersonGuid );

                if ( person == null )
                {
                    // TODO JMH Add attendee.
                }
                else
                {
                    person.DidAttend = bag.DidAttend;
                }

                rockContext.SaveChanges();

                await Clients.Channel( GetChannelNameForAttendanceOccurrence( bag.AttendanceOccurrenceGuid ) )
                    .UpdateAttendance( bag.PersonGuid, bag.DidAttend );
            }
        }

        public async Task StartMonitoringAttendanceOccurrence( Guid attendanceOccurrenceGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var attendanceOccurrence = GetAuthorizedAttendanceOccurrence( rockContext, attendanceOccurrenceGuid );

                var channelName = GetChannelNameForAttendanceOccurrence( attendanceOccurrenceGuid );
                await Channels.AddToChannelAsync( Context.ConnectionId, channelName );
            }
        }

        public async Task StopMonitoringAttendanceOccurrence( Guid attendanceOccurrenceGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var channelName = GetChannelNameForAttendanceOccurrence( attendanceOccurrenceGuid );
                await Channels.RemoveFromChannelAsync( Context.ConnectionId, channelName );
            }
        }

        public static string GetChannelNameForAttendanceOccurrence( Guid attendanceOccurrenceGuid )
        {
            return $"AttendanceOccurrence:{attendanceOccurrenceGuid}";
        }

        private AttendanceOccurrence GetAuthorizedAttendanceOccurrence( RockContext rockContext, Guid attendanceOccurrenceGuid )
        {
            var attendanceOccurrence = new AttendanceOccurrenceService( rockContext ).Get( attendanceOccurrenceGuid );

            if ( attendanceOccurrence == null )
            {
                throw new RealTimeException( "Occurrence not found." );
            }

            var currentPerson = Context.CurrentPersonId.HasValue
                ? new PersonService( rockContext ).Get( Context.CurrentPersonId.Value )
                : null;

            if ( !attendanceOccurrence.Group.IsAuthorized( Authorization.VIEW, currentPerson ) )
            {
                throw new RealTimeException( "Not authorized for this group." );
            }

            return attendanceOccurrence;
        }
    }
}
