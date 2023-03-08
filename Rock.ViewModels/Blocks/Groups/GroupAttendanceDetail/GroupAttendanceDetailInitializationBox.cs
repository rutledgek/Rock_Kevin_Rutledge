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
using Rock.Enums.Blocks.Groups.GroupAttendanceDetail;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Groups.GroupAttendanceDetail
{
    public class GroupAttendanceDetailInitializationBox : BlockBox
    {
        public bool AreAttendeesSortedByFirstName { get; set; }
        public string CampusName { get; set; }
        public Guid? CampusGuid { get; set; }
        public Guid GroupGuid { get; set; }
        public string GroupName { get; set; }
        public bool IsNewAttendanceDateAdditionRestricted { get; set; }
        public bool IsGroupNotFoundError { get; set; }
        public bool IsFutureOccurrenceDateSelectionRestricted { get; set; }
        public bool IsNotAuthorizedError { get;set; }
        public bool IsCampusFilteringAllowed { get; set; }
        public string NotesSectionLabel { get; set; }
        public bool IsNotesSectionHidden { get; set; }
        public GroupAttendanceDetailDateSelectionMode AttendanceOccurrenceDateSelectionMode { get; set; }
        public string LocationLabel { get; set; }
        public GroupAttendanceDetailLocationSelectionMode LocationSelectionMode { get; set; }
        public GroupAttendanceDetailScheduleSelectionMode ScheduleSelectionMode { get; set; }
        public string ScheduleLabel { get; set; }
        public DateTime AttendanceOccurrenceDate { get; set; }
        public Guid? LocationGuid { get; set; }
        public bool IsAttendanceOccurrenceTypesSectionShown { get; set; }
        public string AttendanceOccurrenceTypesSectionLabel { get; set; }
        public List<ListItemBag> AttendanceOccurrenceTypes { get; set; }
        public string SelectedAttendanceOccurrenceTypeValue { get; set; }
        public string GroupMembersSectionLabel { get; set; }
        public string Notes { get; set; }
        public bool IsDidNotMeetChecked { get; set; }
        public bool IsNewAttendeeAdditionAllowed { get; set; }
        public string AddPersonAs { get; set; }
        public string AddGroupMemberPageUrl { get; set; }
        public List<GroupAttendanceDetailAttendanceBag> Roster { get; set; }
        public bool IsNoAttendanceOccurrencesError { get; set; }
        public bool IsConfigError { get; set; }
        public Guid? ScheduleGuid { get; set; }
        public Guid? AttendanceOccurrenceGuid { get; set; }
        public bool IsLongListDisabled { get; set; }
        public bool IsDidNotMeetDisabled { get; set; }
    }
}
