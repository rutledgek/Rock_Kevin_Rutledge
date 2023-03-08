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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Rock.Attribute;
using Rock.Badge.Component;
using Rock.Communication;
using Rock.Data;
using Rock.Enums.Blocks.Groups.GroupAttendanceDetail;
using Rock.Logging;
using Rock.Model;
using Rock.RealTime;
using Rock.RealTime.Topics;
using Rock.Security;
using Rock.ViewModels.Blocks.Groups.GroupAttendanceDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Groups
{
    [DisplayName( "Group Attendance Detail" )]
    [Category( "Obsidian > Groups" )]
    [Description( "Lists the group members for a specific occurrence date time and allows selecting if they attended or not." )]

    #region Block Attributes

    [BooleanField(
        "Allow Add",
        Category = AttributeCategory.None,
        DefaultBooleanValue = true,
        Description = "Should block support adding new attendance dates outside of the group's configured schedule and group type's exclusion dates?",
        IsRequired = false,
        Key = AttributeKey.AllowAdd,
        Order = 0 )]

    [BooleanField(
        "Allow Adding Person",
        Category = AttributeCategory.None,
        DefaultBooleanValue = false,
        Description = "Should block support adding new people as attendees?",
        IsRequired = false,
        Key = AttributeKey.AllowAddingPerson,
        Order = 1 )]

    [CustomDropdownListField(
        "Add Person As",
        Category = AttributeCategory.None,
        DefaultValue = "Attendee",
        Description = "'Attendee' will only add the person to attendance. 'Group Member' will add them to the group with the default group role.",
        IsRequired = true,
        Key = AttributeKey.AddPersonAs,
        ListSource = "Attendee,Group Member",
        Order = 2 )]

    [LinkedPage(
        "Group Member Add Page",
        Category = AttributeCategory.Pages,
        Description = "Page to use for adding a new group member. If no page is provided the built in group member edit panel will be used. This panel allows the individual to search the database.",
        IsRequired = false,
        Key = AttributeKey.GroupMemberAddPage,
        Order = 3 )]

    [BooleanField(
        "Allow Campus Filter",
        Category = AttributeCategory.None,
        DefaultBooleanValue = false,
        Description = "Should block add an option to allow filtering people and attendance counts by campus?",
        IsRequired = false,
        Key = AttributeKey.AllowCampusFilter,
        Order = 4 )]

    [WorkflowTypeField(
        "Workflow",
        AllowMultiple = false,
        Category = AttributeCategory.None,
        Description = "An optional workflow type to launch whenever attendance is saved. The Group will be used as the workflow 'Entity' when processing is started. Additionally if a 'StartDateTime' and/or 'Schedule' attribute exist, their values will be set with the corresponding saved attendance values.",
        IsRequired = false,
        Key = AttributeKey.Workflow,
        Order = 5 )]

    [MergeTemplateField(
        "Attendance Roster Template",
        Category = AttributeCategory.None,
        IsRequired = false,
        Key = AttributeKey.AttendanceRosterTemplate,
        Order = 6 )]

    [CodeEditorField(
        "List Item Details Template",
        Category = AttributeCategory.None,
        DefaultValue = DefaultListItemDetailsTemplate,
        Description = "An optional lava template to appear next to each person in the list.",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = false,
        Key = AttributeKey.ListItemDetailsTemplate,
        Order = 7 )]

    [BooleanField(
        "Restrict Future Occurrence Date",
        Category = AttributeCategory.None,
        DefaultBooleanValue = false,
        Description = "Should user be prevented from selecting a future Occurrence date?",
        IsRequired = false,
        Key = AttributeKey.RestrictFutureOccurrenceDate,
        Order = 8 )]

    [BooleanField(
        "Show Notes",
        Category = AttributeCategory.None,
        DefaultBooleanValue = true,
        Description = "Should the notes field be displayed?",
        IsRequired = false,
        Key = AttributeKey.ShowNotes,
        Order = 9 )]

    [TextField(
        "Attendance Note Label",
        Category = AttributeCategory.Labels,
        DefaultValue = "Notes",
        Description = "The text to use to describe the notes",
        IsRequired = true,
        Key = AttributeKey.AttendanceNoteLabel,
        Order = 10 )]

    [EnumsField(
        "Send Summary Email To",
        Category = AttributeCategory.None,
        EnumSourceType = typeof( SendSummaryEmailType ),
        IsRequired = false,
        Key = AttributeKey.SendSummaryEmailTo,
        Order = 11 )]

    [SystemCommunicationField( "Attendance Email",
        Category = AttributeCategory.CommunicationTemplates,
        DefaultSystemCommunicationGuid = Rock.SystemGuid.SystemCommunication.ATTENDANCE_NOTIFICATION,
        Description = "The System Email to use to send the attendance",
        IsRequired = false,
        Key = AttributeKey.AttendanceEmailTemplate,
        Order = 12 )]

    [BooleanField(
        "Sort By First Name",
        Category = AttributeCategory.None,
        DefaultBooleanValue = true,
        Description = "Should the block allow sorting the Members list by First Name?",
        IsRequired = false,
        Key = AttributeKey.AllowSorting,
        Order = 13 )]

    [DefinedValueField(
        "Configured Attendance Types",
        AllowMultiple = true,
        Category = AttributeCategory.None,
        DefaultValue = "",
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CHECK_IN_ATTENDANCE_TYPES,
        Description = "The Attendance types that an occurrence can have. If no or one Attendance type is selected, then none will be shown.",
        IsRequired = false,
        Key = AttributeKey.AttendanceOccurrenceTypes,
        Order = 14 )]

    [TextField(
        "Attendance Type Label",
        Category = AttributeCategory.Labels,
        DefaultValue = "Attendance Location",
        Description = "The label that will be shown for the attendance types section.",
        IsRequired = false,
        Key = AttributeKey.AttendanceOccurrenceTypesLabel,
        Order = 15 )]

    [BooleanField(
        "Disable Long-List",
        Category = AttributeCategory.None,
        DefaultBooleanValue = false,
        Description = "Will disable the long-list feature which groups individuals by the first character of their last name. When enabled, this only shows when there are more than 50 individuals on the list.",
        IsRequired = false,
        Key = AttributeKey.DisableLongList,
        Order = 16 )]

    [BooleanField(
        "Disable Did Not Meet",
        Category = AttributeCategory.None,
        DefaultBooleanValue = false,
        Description = "Allows for hiding the flag that the group did not meet.",
        IsRequired = false,
        Key = AttributeKey.DisableDidNotMeet,
        Order = 16 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "64ECB2E0-218F-4EB4-8691-7DC94A767037" )]
    [Rock.SystemGuid.BlockTypeGuid( "308DBA32-F656-418E-A019-9D18235027C1" )]

    // TODO JMH Can this block play nicely with the WebForms block?

    public class GroupAttendanceDetail : RockObsidianBlockType
    {
        #region Attribute Values

        private const string DefaultListItemDetailsTemplate = @"<div style=""width: 320px; display: flex; align-items: center; gap: 8px; padding: 12px;"">
    <img width=""80px"" height=""80px"" src=""{{ Person.PhotoUrl }}"" style=""border-radius: 80px; width: 80px; height: 80px"" />
    <div>
        <strong>{{ Person.LastName }}, {{ Person.NickName }}</strong>
        {% if GroupMember.GroupRole %}<div>{{ GroupMember.GroupRole.Name }}</div>{% endif %}
        {% if GroupMember.GroupMemberStatus and GroupMember.GroupMemberStatus != 'Active' %}<span class=""label label-info"" style=""position: absolute; right: 10px; top: 10px;"">{{ GroupMember.GroupMemberStatus }}</span>{% endif %}
    </div>
</div>";

        #endregion

        #region Categories

        private static class AttributeCategory
        {
            public const string None = "";

            public const string CommunicationTemplates = "Communication Templates";

            public const string Labels = "Labels";

            public const string Pages = "Pages";
        }

        #endregion

        #region Keys

        /// <summary>
        /// Keys for attributes.
        /// </summary>
        private static class AttributeKey
        {
            public const string AllowAdd = "AllowAdd";
            public const string AllowAddingPerson = "AllowAddingPerson";
            public const string AddPersonAs = "AddPersonAs";
            public const string GroupMemberAddPage = "GroupMemberAddPage";
            public const string AllowCampusFilter = "AllowCampusFilter";
            public const string Workflow = "Workflow";
            public const string AttendanceRosterTemplate = "AttendanceRosterTemplate";
            public const string ListItemDetailsTemplate = "ListItemDetailsTemplate";
            public const string RestrictFutureOccurrenceDate = "RestrictFutureOccurrenceDate";
            public const string ShowNotes = "ShowNotes";
            public const string AttendanceNoteLabel = "AttendanceNoteLabel";
            public const string SendSummaryEmailTo = "SendSummaryEmailTo";
            public const string AttendanceEmailTemplate = "AttendanceEmailTemplate";
            public const string AllowSorting = "AllowSorting";
            public const string AttendanceOccurrenceTypes = "AttendanceTypes";
            public const string AttendanceOccurrenceTypesLabel = "AttendanceTypeLabel";
            public const string DisableLongList = "DisableLongList";
            public const string DisableDidNotMeet = "DisableDidNotMeet";
        }

        /// <summary>
        /// Keys for page parameters.
        /// </summary>
        private static class PageParameterKey
        {
            public const string GroupId = "GroupId";
            public const string GroupName = "GroupName";
            public const string GroupTypeIds = "GroupTypeIds";
            public const string OccurrenceId = "OccurrenceId";
            public const string Occurrence = "Occurrence";
            public const string Date = "Date";
            public const string LocationId = "LocationId";
            public const string ScheduleId = "ScheduleId";
            public const string ReturnUrl = "ReturnUrl";
        }

        private static class UserPreferenceKeys
        {
            public const string AreGroupAttendanceAttendeesSortedByFirstName = "Attendance_List_Sorting_Toggle";
            public const string Campus = "Campus";
        }

        private static class MergeFieldKeys
        {
            public const string AttendanceDate = "AttendanceDate";

            public const string AttendanceNoteLabel = "AttendanceNoteLabel";

            public const string AttendanceOccurrence = "AttendanceOccurrence";

            public const string Attended = "Attended";

            public const string Group = "Group";

            public const string GroupMember = "GroupMember";

            public const string Person = "Person";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Should block restrict adding new attendance dates outside of the group's configured schedule and group type's exclusion dates?
        /// </summary>
        /// <value>
        ///   <c>true</c> if using new attendance dates is restricted; otherwise, <c>false</c>.
        /// </value>
        private bool IsNewAttendanceDateAdditionRestricted => !GetAttributeValue( AttributeKey.AllowAdd ).AsBoolean();

        /// <summary>
        /// Gets the attendance note label.
        /// </summary>
        private string AttendanceNoteLabel => GetAttributeValue( AttributeKey.AttendanceNoteLabel );

        /// <summary>
        /// Should block support adding new people as attendees?
        /// </summary>
        /// <value>
        ///   <c>true</c> if new attendee addition is allowed; otherwise, <c>false</c>.
        /// </value>
        private bool IsNewAttendeeAdditionAllowed => GetAttributeValue( AttributeKey.AllowAddingPerson ).AsBoolean();

        /// <summary>
        /// 'Attendee' will only add the person to attendance. 'Group Member' will add them to the group with the default group role.
        /// </summary>
        private string AddPersonAs => GetAttributeValue( AttributeKey.AddPersonAs );

        /// <summary>
        /// Page to use for adding a new group member. If no page is provided the built in group member edit panel will be used. This panel allows the individual to search the database.
        /// </summary>
        private string AddGroupMemberPage => GetAttributeValue( AttributeKey.GroupMemberAddPage );

        /// <summary>
        /// Should block add an option to allow filtering people and attendance counts by campus?
        /// </summary>
        /// <value>
        ///   <c>true</c> if filtering by campus is allowed; otherwise, <c>false</c>.
        /// </value>
        private bool IsCampusFilteringAllowed => GetAttributeValue( AttributeKey.AllowCampusFilter ).AsBoolean();

        /// <summary>
        /// An optional workflow type to launch whenever attendance is saved.
        /// <para>The Group will be used as the workflow 'Entity' when processing is started. Additionally if a 'StartDateTime' and/or 'Schedule' attribute exist, their values will be set with the corresponding saved attendance values.</para>
        /// </summary>
        private Guid? WorkflowGuid => GetAttributeValue( AttributeKey.Workflow ).AsGuidOrNull();

        /// <summary>
        /// An optional workflow type to launch whenever attendance is saved.
        /// <para>The Group will be used as the workflow 'Entity' when processing is started. Additionally if a 'StartDateTime' and/or 'Schedule' attribute exist, their values will be set with the corresponding saved attendance values.</para>
        /// </summary>
        /// <value>
        /// The workflow cache.
        /// </value>
        private WorkflowTypeCache WorkflowType
        {
            get
            {
                var guid = this.WorkflowGuid;

                if ( guid.HasValue )
                {
                    return WorkflowTypeCache.Get( guid.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the attendance roster template unique identifier.
        /// </summary>
        private Guid AttendanceRosterTemplateGuid => GetAttributeValue( AttributeKey.AttendanceRosterTemplate ).AsGuid();

        /// <summary>
        /// An optional lava template to appear next to each person in the list.
        /// </summary>
        private string ListItemDetailsTemplate => GetAttributeValue( AttributeKey.ListItemDetailsTemplate );

        /// <summary>
        /// Should user be restricted from selecting a future Occurrence date?
        /// </summary>
        /// <value>
        ///   <c>true</c> if future occurrence date selection is restricted; otherwise, <c>false</c>.
        /// </value>
        private bool IsFutureOccurrenceDateSelectionRestricted => GetAttributeValue( AttributeKey.RestrictFutureOccurrenceDate ).AsBoolean();

        /// <summary>
        /// Should the notes field be hidden?
        /// </summary>
        /// <value>
        ///   <c>true</c> if the notes section is hidden; otherwise, <c>false</c>.
        /// </value>
        private bool IsNotesSectionHidden => !GetAttributeValue( AttributeKey.ShowNotes ).AsBoolean();

        /// <summary>
        /// Gets the notes section label.
        /// </summary>
        private string NotesSectionLabel => GetAttributeValue( AttributeKey.AttendanceNoteLabel );

        /// <summary>
        /// Gets the summary email recipients.
        /// </summary>
        private List<SendSummaryEmailType> SummaryEmailRecipients => GetAttributeValue( AttributeKey.SendSummaryEmailTo )
            .SplitDelimitedValues()
            .Select( a => a.ConvertToEnumOrNull<SendSummaryEmailType>() )
            .Where( a => a.HasValue )
            .Select( a => a.Value )
            .ToList();

        /// <summary>
        /// Gets the System Communication template unique identifier of the attendance system email.
        /// </summary>
        private Guid AttendanceEmailTemplateGuid => GetAttributeValue( AttributeKey.AttendanceEmailTemplate ).AsGuid();

        /// <summary>
        /// Should the block allow sorting the attendees list by First Name?
        /// </summary>
        private bool AreAttendeesSortedByFirstName => GetAttributeValue( AttributeKey.AllowSorting ).AsBoolean();

        /// <summary>
        /// The Attendance types that an occurrence can have.
        /// <para>If no or one Attendance type is selected, then none will be shown.</para>
        /// </summary>
        private List<string> AttendanceOccurrenceTypes => GetAttributeValues( AttributeKey.AttendanceOccurrenceTypes );

        /// <summary>
        /// The Attendance type values that an occurrence can have.
        /// <para>If no or one Attendance type is selected, then none will be shown.</para>
        /// </summary>
        private List<DefinedValueCache> AttendanceOccurrenceTypeValues => AttendanceOccurrenceTypes.Select( attendanceOccurrenceType => DefinedValueCache.Get( attendanceOccurrenceType ) ).ToList();

        /// <summary>
        /// The label that will be shown for the attendance types section.
        /// </summary>
        private string AttendanceOccurrenceTypesLabel => GetAttributeValue( AttributeKey.AttendanceOccurrenceTypesLabel );

        /// <summary>
        /// Gets the group identifier page parameter or null if missing.
        /// </summary>
        private int? GroupIdPageParameter => PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();

        /// <summary>
        /// Gets the group type identifiers page parameter or null if missing.
        /// </summary>
        private string GroupTypeIdsPageParameter => PageParameter( PageParameterKey.GroupTypeIds );

        /// <summary>
        /// The Campus ID filter.
        /// </summary>
        private int? CampusIdBlockUserPreference
        {
            get
            {
                return GetCurrentUserPreferenceForBlock( UserPreferenceKeys.Campus ).AsIntegerOrNull();
            }
            set
            {
                SetCurrentUserPreferenceForBlock( UserPreferenceKeys.Campus, value.ToString() );
            }
        }

        /// <summary>
        /// Gets the Occurrence ID page parameter or null if missing.
        /// </summary>
        private int? OccurrenceIdPageParameter => PageParameter( PageParameterKey.OccurrenceId ).AsIntegerOrNull();

        /// <summary>
        /// Gets the Date page parameter or null if missing.
        /// </summary>
        private DateTime? DatePageParameter => PageParameter( PageParameterKey.Date ).AsDateTime();

        /// <summary>
        /// Gets the Occurrence page parameter or null if missing.
        /// </summary>
        private DateTime? OccurrencePageParameter => PageParameter( PageParameterKey.Occurrence ).AsDateTime();

        /// <summary>
        /// Gets the Location ID page parameter or null if missing.
        /// </summary>
        private int? LocationIdPageParameter => PageParameter( PageParameterKey.LocationId ).AsIntegerOrNull();

        /// <summary>
        /// Gets the Schedule ID page parameter.
        /// </summary>
        private int? ScheduleIdPageParameter => PageParameter( PageParameterKey.ScheduleId ).AsIntegerOrNull();

        /// <summary>
        /// Will disable the long-list feature which groups individuals by the first character of their last name.
        /// <para>When enabled, this only shows when there are more than 50 individuals on the list.</para>
        /// </summary>
        public bool IsLongListDisabled => GetAttributeValue( AttributeKey.DisableLongList ).AsBoolean();

        /// <summary>
        /// Allows for hiding the flag that the group did not meet.
        /// </summary>
        public bool IsDidNotMeetDisabled => GetAttributeValue( AttributeKey.DisableDidNotMeet ).AsBoolean();

        #endregion

        #region IRockObsidianBlockType Implementation

        /// <inheritdoc/>
        public override string BlockFileUrl => $"{base.BlockFileUrl}.obs";

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var occurrenceDataClientService = GetOccurrenceDataClientService( rockContext );
                var searchParameters = occurrenceDataClientService.GetAttendanceOccurrenceSearchParameters( campusIdOverride: this.CampusIdBlockUserPreference );
                var occurrenceData = occurrenceDataClientService.GetOccurrenceData( searchParameters, asNoTracking: false );
                var box = new GroupAttendanceDetailInitializationBox();

                if ( !occurrenceData.IsValid )
                {
                    SetErrorData( occurrenceData, box );
                    return box;
                }

                SetInitializationBox( rockContext, occurrenceData, box );

                return box;
            }
        }

        #endregion

        #region Block Actions

        [BlockAction( "GetAttendance" )]
        public BlockActionResult GetAttendance( GroupAttendanceDetailGetAttendanceRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var attendanceInfo = new AttendanceService( rockContext )
                    .Queryable()
                    .Where( a =>  a.Guid == bag.AttendanceGuid )
                    .Select( a => new
                    {
                        GroupId = a.Occurrence.GroupId,
                        Person = a.PersonAlias.Person,
                        DidAttend = a.DidAttend
                    } )
                    .FirstOrDefault();

                if ( attendanceInfo == null )
                {
                    return ActionBadRequest( "Attendance not found." );
                }

                if ( !attendanceInfo.GroupId.HasValue )
                {
                    return ActionBadRequest( "Group not found." );
                }

                var group = new GroupService( rockContext ).Get( attendanceInfo.GroupId.Value );

                if (!group.IsAuthorized(Authorization.VIEW, GetCurrentPerson() ) )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.Forbidden );
                }

                // The attendee may not be a member of the group.
                var groupMember = new GroupMemberService( rockContext )
                    .Queryable()
                    .Where( m =>
                        m.GroupId == group.Id
                        && m.GroupId == group.Id
                        && m.PersonId == attendanceInfo.Person.Id )
                    .FirstOrDefault();

                var attendanceBag = GetRosterAttendeeBag( attendanceInfo.Person, attendanceInfo.DidAttend ?? false, groupMember );

                return ActionOk( attendanceBag );
            }
        }

        /// <summary>
        /// Saves the Attendance Occurrence.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [BlockAction( "SaveAttendanceOccurrence" )]
        public BlockActionResult SaveAttendanceOccurrence( GroupAttendanceDetailSaveAttendanceOccurrenceRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var occurrenceDataClientService = GetOccurrenceDataClientService( rockContext );
                var searchParameters = occurrenceDataClientService.GetAttendanceOccurrenceSearchParameters( bag );
                var occurrenceData = occurrenceDataClientService.GetOccurrenceData( searchParameters, asNoTracking: false );

                if ( !occurrenceData.IsValid )
                {
                    return ActionBadRequest( occurrenceData.ErrorMessage );
                }

                if ( occurrenceDataClientService.Save( occurrenceData, bag ) )
                {
                    rockContext.SaveChanges();

                    if ( occurrenceData.AttendanceOccurrence.LocationId.HasValue )
                    {
                        Rock.CheckIn.KioskLocationAttendance.Remove( occurrenceData.AttendanceOccurrence.LocationId.Value );
                    }

                    var workflowType = this.WorkflowType;
                    if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                    {
                        try
                        {
                            var workflow = Rock.Model.Workflow.Activate( workflowType, occurrenceData.Group.Name );

                            workflow.SetAttributeValue( "StartDateTime", occurrenceData.AttendanceOccurrence.OccurrenceDate.ToString( "o" ) );

                            if ( occurrenceData.Group.Schedule != null )
                            {
                                workflow.SetAttributeValue( "Schedule", occurrenceData.Group.Schedule.Guid.ToString() );
                            }

                            new WorkflowService( rockContext ).Process( workflow, occurrenceData.Group, out var workflowErrors );
                        }
                        catch ( Exception ex )
                        {
                            // ExceptionLogService.LogException( ex, this.Context );
                        }
                    }

                    // Use a new RockContext when emailing the attendance summary to get the freshest data.
                   // EmailAttendanceSummary( occurrenceData.AttendanceOccurrence.Id, occurrenceData.Group.Id );

                    return ActionOk( new GroupAttendanceDetailSaveAttendanceOccurrenceResponseBag
                    {
                        RedirectUrl = GetRedirectUrl( occurrenceData.Group.Id, occurrenceData.AttendanceOccurrence.Id ),
                        AttendanceOccurrenceGuid = occurrenceData.AttendanceOccurrence.Guid
                    } );

                    // TODO JMH Need to push updates to all listeners.
                }
                else
                {
                    return ActionBadRequest( occurrenceData.ErrorMessage );
                }
            }
        }

        /// <summary>
        /// Prints the group attendance occurrence roster.
        /// </summary>
        [BlockAction( "PrintRoster" )]
        public BlockActionResult PrintRoster()
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = GetOccurrenceDataClientService( rockContext );

                // Use the default search parameters so we only print the persisted AttendanceOccurrence.
                var searchParameters = clientService.GetAttendanceOccurrenceSearchParameters();
                var occurrenceData = clientService.GetOccurrenceData( searchParameters, asNoTracking: true );

                if ( !occurrenceData.IsValid )
                {
                    return ActionBadRequest( occurrenceData.ErrorMessage );
                }

                var roster = GetRoster( rockContext, occurrenceData );

                var mergeObjects = new Dictionary<int, object>();

                if ( roster.Any() == true )
                {
                    var personGuids = roster.Select( a => a.PersonGuid ).ToList();
                    var personList = new PersonService( rockContext )
                        .GetByGuids( personGuids )
                        .OrderBy( a => a.LastName )
                        .ThenBy( a => a.NickName )
                        .ToList();
                    foreach ( var person in personList )
                    {
                        mergeObjects.AddOrIgnore( person.Id, person );
                    }
                }

                var mergeFields = this.RequestContext.GetCommonMergeFields();
                mergeFields.Add( MergeFieldKeys.Group, occurrenceData.Group );
                mergeFields.Add( MergeFieldKeys.AttendanceDate, occurrenceData.AttendanceOccurrence.OccurrenceDate );

                var mergeTemplate = new MergeTemplateService( rockContext ).Get( this.AttendanceRosterTemplateGuid );

                if ( mergeTemplate == null )
                {
                    RockLogger.Log.Error( RockLogDomains.Group, new Exception( "Error printing Attendance Roster: No merge template selected. Please configure an 'Attendance Roster Template' in the block settings." ) );
                    return ActionBadRequest( "Unable to print Attendance Roster: No merge template selected. Please configure an 'Attendance Roster Template' in the block settings." );
                }

                var mergeTemplateType = mergeTemplate.GetMergeTemplateType();

                if ( mergeTemplateType == null )
                {
                    RockLogger.Log.Error( RockLogDomains.Group, new Exception( "Error printing Attendance Roster: Unable to determine Merge Template Type from the 'Attendance Roster Template' in the block settings." ) );
                    return ActionBadRequest( $"Error printing Attendance Roster: Unable to determine Merge Template Type from the 'Attendance Roster Template' in the block settings." );
                }

                var mergeObjectList = mergeObjects.Select( a => a.Value ).ToList();

                var outputBinaryFileDoc = mergeTemplateType.CreateDocument( mergeTemplate, mergeObjectList, mergeFields );

                // Set the name of the output doc.
                outputBinaryFileDoc = new BinaryFileService( rockContext ).Get( outputBinaryFileDoc.Id );
                outputBinaryFileDoc.FileName = occurrenceData.Group.Name + " Attendance Roster" + Path.GetExtension( outputBinaryFileDoc.FileName ?? string.Empty ) ?? ".docx";
                rockContext.SaveChanges();

                if ( mergeTemplateType.Exceptions != null && mergeTemplateType.Exceptions.Any() )
                {
                    if ( mergeTemplateType.Exceptions.Count == 1 )
                    {
                        RockLogger.Log.Error(RockLogDomains.Group, mergeTemplateType.Exceptions[0] );
                    }
                    else if ( mergeTemplateType.Exceptions.Count > 50 )
                    {
                        RockLogger.Log.Error( RockLogDomains.Group, new AggregateException( $"Exceptions merging template {mergeTemplate.Name}. See InnerExceptions for top 50.", mergeTemplateType.Exceptions.Take( 50 ).ToList() ) );
                    }
                    else
                    {
                        RockLogger.Log.Error( RockLogDomains.Group, new AggregateException( $"Exceptions merging template {mergeTemplate.Name}. See InnerExceptions", mergeTemplateType.Exceptions.ToList() ) );
                    }
                }

                var uri = new UriBuilder( outputBinaryFileDoc.Url );
                var queryString = uri.Query.ParseQueryString();
                queryString["attachment"] = true.ToTrueFalse();
                uri.Query = queryString.ToString();

                return ActionOk( new GroupAttendanceDetailPrintRosterResponseBag
                {
                    RedirectUrl = uri.ToString()
                } );
            }
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppAddPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [BlockAction( "AddPerson" )]
        public BlockActionResult AddPerson( GroupAttendanceDetailAddPersonRequestBag bag )
        {
            if ( !bag.PersonGuid.HasValue )
            {
                return ActionOk();
            }

            using ( var rockContext = new RockContext() )
            {
                var clientService = GetOccurrenceDataClientService( rockContext );
                var searchParameters = clientService.GetAttendanceOccurrenceSearchParameters();
                var occurrenceData = new OccurrenceData();

                if ( !clientService.TrySetGroup( occurrenceData, searchParameters, asNoTracking: true ) )
                {
                    return ActionBadRequest( occurrenceData.ErrorMessage );
                }

                var person = new PersonService( rockContext ).Get( bag.PersonGuid.Value );

                if ( person == null )
                {
                    return ActionBadRequest( "Person not found." );
                }

                var addPersonAs = this.AddPersonAs;

                GroupMember groupMember = null;

                if ( !addPersonAs.IsNullOrWhiteSpace() && addPersonAs == "Group Member" )
                {
                    groupMember = AddPersonAsGroupMember( occurrenceData.Group, person, rockContext );
                    rockContext.SaveChanges();
                }

                // TODO JMH We'll need to add an Attendee record for real-time.
                var attendee = GetRosterAttendeeBag( person, true, groupMember );

                return ActionOk( new GroupAttendanceDetailAddPersonResponseBag
                {
                    Attendee = attendee
                } );
            }
        }

        [BlockAction("GetOrCreate")]
        public BlockActionResult GetOrCreate( GroupAttendanceDetailSaveAttendanceOccurrenceRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = GetOccurrenceDataClientService( rockContext );
                var searchParameters = clientService.GetAttendanceOccurrenceSearchParameters( bag );
                var occurrenceData = clientService.GetOccurrenceData( searchParameters );

                if ( !occurrenceData.IsValid )
                {
                    return ActionBadRequest( occurrenceData.ErrorMessage );
                }

                if ( occurrenceData.IsNewOccurrence )
                {
                    var result = clientService.Save( occurrenceData, bag );

                    if (!result)
                    {
                        return ActionBadRequest( occurrenceData.ErrorMessage );
                    }

                    rockContext.SaveChanges();
                }

                var box = new GroupAttendanceDetailInitializationBox();

                SetInitializationBox( rockContext, occurrenceData, box );

                return ActionOk( box );
            }
        }

        #region Real-time Actions

        [BlockAction( "SubscribeToRealTime" )]
        public async Task<BlockActionResult> SubscribeToRealTime( string connectionId, Guid groupGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var group = new GroupService( rockContext ).Get( groupGuid );

                // Authorize the current user.
                if ( group == null )
                {
                    return ActionNotFound( "Group not found." );
                }

                if ( !group.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.Forbidden );
                }

                await RealTimeHelper.GetTopicContext<IEntityUpdated>().Channels.AddToChannelAsync( connectionId, EntityUpdatedTopic.GetAttendanceChannelForGroup( groupGuid ) );

                return ActionOk();
            }
        }

        [BlockAction( "UnsubscribeFromRealTime" )]
        public async Task<BlockActionResult> UnsubscribeFromRealTime( string connectionId, Guid groupGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var group = new GroupService( rockContext ).Get( groupGuid );

                // Authorize the current user.
                if ( group == null )
                {
                    // Don not return an error if the group is not found since we are disconnecting.
                    // Just return a 200 so the client knows they are not connected anymore.
                    return ActionOk();
                }

               await RealTimeHelper.GetTopicContext<IEntityUpdated>().Channels.RemoveFromChannelAsync( connectionId, EntityUpdatedTopic.GetAttendanceChannelForGroup( groupGuid ) );

                return ActionOk();
            }
        }

        [BlockAction( "MarkAttendance" )]
        public BlockActionResult MarkAttendance( GroupAttendanceDetailMarkAttendanceRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = GetOccurrenceDataClientService( rockContext );
                var searchParameters = clientService.GetAttendanceOccurrenceSearchParameters( attendanceOccurrenceGuidOverride: bag.AttendanceOccurrenceGuid );
                var occurrenceData = clientService.GetOccurrenceData( searchParameters );

                if ( !occurrenceData.IsValid )
                {
                    return ActionBadRequest( occurrenceData.ErrorMessage );
                }

                var attendance = occurrenceData.AttendanceOccurrence.Attendees.FirstOrDefault( a => a.PersonAlias.Person.Guid == bag.PersonGuid );

                if ( attendance == null )
                {
                    var personAliasId = new PersonService( rockContext ).Get( bag.PersonGuid )?.PrimaryAliasId;
                    var occurrenceLocationCampusId = new LocationService( rockContext ).GetCampusIdForLocation( occurrenceData.AttendanceOccurrence.LocationId );
                    var campusId = GetAttendanceCampusId( occurrenceLocationCampusId, occurrenceData.Group.CampusId, bag.CampusGuid );

                    DateTime startDateTime;

                    if ( occurrenceData.AttendanceOccurrence.Schedule != null
                        && occurrenceData.AttendanceOccurrence.Schedule.HasSchedule() )
                    {
                        startDateTime = occurrenceData.AttendanceOccurrence.OccurrenceDate.Date.Add(
                            occurrenceData.AttendanceOccurrence.Schedule.StartTimeOfDay );
                    }
                    else
                    {
                        startDateTime = occurrenceData.AttendanceOccurrence.OccurrenceDate;
                    }

                    occurrenceData.AttendanceOccurrence.Attendees.Add( CreateAttendance( personAliasId, campusId, startDateTime, bag.DidAttend ) );
                }
                else
                {
                    attendance.DidAttend = bag.DidAttend;
                }

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        [BlockAction( "UpdateDidNotOccur" )]
        public BlockActionResult UpdateDidNotOccur( GroupAttendanceDetailUpdateDidNotOccurRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = GetOccurrenceDataClientService( rockContext );
                var searchParameters = clientService.GetAttendanceOccurrenceSearchParameters( attendanceOccurrenceGuidOverride: bag.AttendanceOccurrenceGuid );
                var occurrenceData = clientService.GetOccurrenceData( searchParameters );

                if ( !occurrenceData.IsValid )
                {
                    return ActionBadRequest( occurrenceData.ErrorMessage );
                }

                // JMH Refactor this if needed. Check the existing save logic to see if this logic matches or if we can DRY things out.
                occurrenceData.AttendanceOccurrence.DidNotOccur = bag.DidNotOccur;

                if ( bag.DidNotOccur )
                {
                    // Clear the DidAttend flags if the occurrence did not occur.
                    foreach ( var attendee in occurrenceData.AttendanceOccurrence.Attendees )
                    {
                        attendee.DidAttend = null;
                    }
                }
                else
                {
                    var occurrenceLocationCampusId = new LocationService( rockContext ).GetCampusIdForLocation( occurrenceData.AttendanceOccurrence.LocationId );
                    var campusId = GetAttendanceCampusId( occurrenceLocationCampusId, occurrenceData.Group.CampusId, bag.CampusGuid );

                    foreach ( var attendee in GetRoster( rockContext, occurrenceData ).Where( a => a.PersonAliasId.HasValue ) )
                    {
                        var attendance = CreateAttendance(
                            attendee.PersonAliasId,
                            campusId,
                            occurrenceData.AttendanceOccurrence.Schedule != null && occurrenceData.AttendanceOccurrence.Schedule.HasSchedule() ? occurrenceData.AttendanceOccurrence.OccurrenceDate.Date.Add( occurrenceData.AttendanceOccurrence.Schedule.StartTimeOfDay ) : occurrenceData.AttendanceOccurrence.OccurrenceDate,
                            false );

                        if ( !attendance.IsValid )
                        {
                            occurrenceData.ErrorMessage = attendance.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                            return ActionBadRequest( occurrenceData.ErrorMessage );
                        }

                        occurrenceData.AttendanceOccurrence.Attendees.Add( attendance );
                    }
                } 

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        [BlockAction( "UpdateAttendanceOccurrenceType" )]
        public BlockActionResult UpdateAttendanceOccurrenceType( GroupAttendanceDetailUpdateAttendanceOccurrenceTypeRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var clientService = GetOccurrenceDataClientService( rockContext );
                var searchParameters = clientService.GetAttendanceOccurrenceSearchParameters( attendanceOccurrenceGuidOverride: bag.AttendanceOccurrenceGuid );
                var occurrenceData = clientService.GetOccurrenceData( searchParameters );

                if ( !occurrenceData.IsValid )
                {
                    return ActionBadRequest( occurrenceData.ErrorMessage );
                }
                // JMH Use the this.AttendanceOccurrenceTypes property to ensure only allowed types are set.
                occurrenceData.AttendanceOccurrence.AttendanceTypeValueId = DefinedValueCache.GetId( bag.AttendanceOccurrenceTypeGuid );

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion

        //[BlockAction("UpdateDidAttend")]
        //public BlockActionResult UpdateDidAttend( GroupAttendanceDetailUpdateDidAttendRequestBag bag )
        //{
        //    using ( var rockContext = new RockContext() )
        //    {
        //        var clientService = GetOccurrenceDataClientService( rockContext );
        //        var occurrenceData = clientService.GetOccurrenceData();

        //        if ( !occurrenceData.IsValid )
        //        {
        //            return ActionBadRequest( occurrenceData.ErrorMessage );
        //        }

        //        var attendanceService = new AttendanceService( rockContext );

        //        // Update the attendees.
        //        // JMH This will trigger updates in subscribers.
        //        var existingAttendees = occurrenceData.AttendanceOccurrence.Attendees.ToList();

        //        if ( bag.DidNotOccur )
        //        {
        //            // If did not meet was selected and this was a manually entered occurrence (not based on a schedule/location)
        //            // then just delete all the attendance records instead of tracking a 'did not meet' value
        //            if ( !occurrenceData.AttendanceOccurrence.ScheduleId.HasValue )
        //            {
        //                foreach ( var attendance in existingAttendees )
        //                {
        //                    attendanceService.Delete( attendance );
        //                }
        //            }
        //            // If the occurrence is based on a schedule and there are attendees,
        //            // then clear the did not meet flags on existing attendees.
        //            else if ( existingAttendees.Any() )
        //            {
        //                foreach ( var attendance in existingAttendees )
        //                {
        //                    attendance.DidAttend = null;
        //                }
        //            }
        //            // If the occurrence is based on a schedule and there are no attendees,
        //            // then add new attendees with did not meet flags set.
        //            else
        //            {
        //                var campusId = GetAttendanceCampusId( occurrenceData, bag );

        //                foreach ( var attendee in bag.Attendees.Where( a => a.PersonAliasId.HasValue ) )
        //                {
        //                    var attendance = new Attendance
        //                    {
        //                        PersonAliasId = attendee.PersonAliasId,
        //                        CampusId = campusId,
        //                        StartDateTime = occurrenceData.AttendanceOccurrence.Schedule != null && occurrenceData.AttendanceOccurrence.Schedule.HasSchedule() ? occurrenceData.AttendanceOccurrence.OccurrenceDate.Date.Add( occurrenceData.AttendanceOccurrence.Schedule.StartTimeOfDay ) : occurrenceData.AttendanceOccurrence.OccurrenceDate,
        //                        DidAttend = false
        //                    };

        //                    if ( !attendance.IsValid )
        //                    {
        //                        occurrenceData.ErrorMessage = attendance.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
        //                        return false;
        //                    }

        //                    occurrenceData.AttendanceOccurrence.Attendees.Add( attendance );
        //                }
        //            }
        //        }
        //        else
        //        {
        //            // Validate the AttendanceOccurrence before updating the Attendance records.
        //            if ( !occurrenceData.AttendanceOccurrence.IsValid )
        //            {
        //                occurrenceData.ErrorMessage = occurrenceData.AttendanceOccurrence.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
        //                return false;
        //            }

        //            var campusId = new Lazy<int?>( () => GetAttendanceCampusId( occurrenceData, bag ) );

        //            DateTime startDateTime;

        //            if ( occurrenceData.AttendanceOccurrence.Schedule != null
        //                && occurrenceData.AttendanceOccurrence.Schedule.HasSchedule() )
        //            {
        //                startDateTime = occurrenceData.AttendanceOccurrence.OccurrenceDate.Date.Add(
        //                    occurrenceData.AttendanceOccurrence.Schedule.StartTimeOfDay );
        //            }
        //            else
        //            {
        //                startDateTime = occurrenceData.AttendanceOccurrence.OccurrenceDate;
        //            }

        //            foreach ( var attendee in bag.Attendees )
        //            {
        //                var attendance = existingAttendees
        //                    .Where( a => a.PersonAliasId == attendee.PersonAliasId )
        //                    .FirstOrDefault();

        //                if ( attendance == null )
        //                {
        //                    if ( attendee.PersonAliasId.HasValue )
        //                    {
        //                        attendance = new Attendance
        //                        {
        //                            PersonAliasId = attendee.PersonAliasId,
        //                            CampusId = campusId.Value,
        //                            StartDateTime = startDateTime,
        //                            DidAttend = attendee.HasAttended
        //                        };

        //                        // Check that the attendance record is valid
        //                        if ( !attendance.IsValid )
        //                        {
        //                            occurrenceData.ErrorMessage = attendance.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
        //                            return false;
        //                        }

        //                        occurrenceData.AttendanceOccurrence.Attendees.Add( attendance );
        //                    }
        //                }
        //                else
        //                {
        //                    // Otherwise, only record that they attended -- don't change their attendance startDateTime.
        //                    attendance.DidAttend = attendee.HasAttended;
        //                }
        //            }
        //        }
        //    }
        //}

        #endregion

        #region Private Methods

        private string GetRedirectUrl( int groupId, int attendanceOccurrenceId )
        {
            var qryParams = new Dictionary<string, string>
            {
                { PageParameterKey.GroupId, groupId.ToString() },
                { PageParameterKey.OccurrenceId, attendanceOccurrenceId.ToString() }
            };

            var groupTypeIds = this.GroupTypeIdsPageParameter;
            if ( groupTypeIds.IsNotNullOrWhiteSpace() )
            {
                qryParams.Add( PageParameterKey.GroupTypeIds, groupTypeIds );
            }

            return this.GetCurrentPageUrl( qryParams );
        }

        /// <summary>
        /// Gets a user preference for the current user.
        /// </summary>
        /// <param name="key">The user preference key.</param>
        /// <returns>The user preference.</returns>
        private string GetCurrentUserPreference( string key )
        {
            return PersonService.GetUserPreference( this.GetCurrentPerson(), key );
        }

        /// <summary>
        /// Gets a user preference for the current user and block instance.
        /// </summary>
        /// <param name="key">The user preference key that will be converted to a block user preference key.</param>
        /// <returns>The user preference.</returns>
        private string GetCurrentUserPreferenceForBlock( string key )
        {
            return GetCurrentUserPreference( GetUserPreferenceKeyForBlock( key ) );
        }

        /// <summary>
        /// Sets a user preference for the current user.
        /// </summary>
        /// <param name="key">The user preference key.</param>
        /// <param name="value">The user preference value.</param>
        private void SetCurrentUserPreference( string key, string value )
        {
            PersonService.SaveUserPreference( this.GetCurrentPerson(), key, value );
        }

        /// <summary>
        /// Sets a user preference for the current user and block instance.
        /// </summary>
        /// <param name="key">The user preference key that will be converted to a block user preference key.</param>
        /// <param name="value">The user preference value.</param>
        private void SetCurrentUserPreferenceForBlock( string key, string value )
        {
            SetCurrentUserPreference( GetUserPreferenceKeyForBlock( key ), value );
        }

        /// <summary>
        /// Gets the user preference key for this block instance.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The block user preference key.</returns>
        private string GetUserPreferenceKeyForBlock( string key )
        {
            return $"{PersonService.GetBlockUserPreferenceKeyPrefix( this.BlockId )}{key}";
        }

        private OccurrenceDataClientService GetOccurrenceDataClientService( RockContext rockContext )
        {
            return new OccurrenceDataClientService(
                this,
                new GroupService( rockContext ),
                new AttendanceService( rockContext ),
                new AttendanceOccurrenceService( rockContext ),
                new LocationService( rockContext ),
                new ScheduleService( rockContext ) );
        }

        private void SetAddGroupMemberPageUrl( OccurrenceData occurrenceData, GroupAttendanceDetailInitializationBox box )
        {
            if ( this.AddGroupMemberPage.IsNullOrWhiteSpace() )
            {
                return;
            }

            var returnUrl = this.PageParameter( PageParameterKey.ReturnUrl );

            if ( returnUrl.IsNullOrWhiteSpace() )
            {
                returnUrl = this.RequestContext.RequestUri.AbsoluteUri;
            }

            var queryParams = new Dictionary<string, string>
                {
                    { PageParameterKey.GroupId, occurrenceData.Group.Id.ToString() },
                    { PageParameterKey.GroupName, occurrenceData.Group.Name },
                    { PageParameterKey.ReturnUrl, returnUrl }
                };

            box.AddGroupMemberPageUrl = this.GetLinkedPageUrl( AttributeKey.GroupMemberAddPage, queryParams );
        }

        private void SetErrorData( OccurrenceData occurrenceData, GroupAttendanceDetailInitializationBox box )
        {
            box.ErrorMessage = occurrenceData.ErrorMessage;
            box.IsGroupNotFoundError = occurrenceData.IsGroupNotFoundError;
            box.IsNotAuthorizedError = occurrenceData.IsNotAuthorizedError;
            box.IsNoAttendanceOccurrencesError = occurrenceData.IsNoAttendanceOccurrencesError;

            box.IsConfigError = box.ErrorMessage.IsNotNullOrWhiteSpace()
                || box.IsGroupNotFoundError
                || box.IsNotAuthorizedError
                || box.IsNoAttendanceOccurrencesError;
            // TODO JMH Add error when user trying to save new occurrence where one already exists (for date, location, schedule).
        }

        /// <summary>
        /// Gets the initialization box.
        /// </summary>
        private GroupAttendanceDetailInitializationBox SetInitializationBox( RockContext rockContext, OccurrenceData occurrenceData, GroupAttendanceDetailInitializationBox box )
        {
            box.AreAttendeesSortedByFirstName = this.AreAttendeesSortedByFirstName;
            box.CampusName = occurrenceData.Campus?.Name;
            box.CampusGuid = occurrenceData.Campus?.Guid;
            box.GroupGuid = occurrenceData.Group.Guid;
            box.GroupName = occurrenceData.Group.Name;
            box.IsCampusFilteringAllowed = this.IsCampusFilteringAllowed;
            box.IsFutureOccurrenceDateSelectionRestricted = this.IsFutureOccurrenceDateSelectionRestricted;
            box.IsNewAttendanceDateAdditionRestricted = this.IsNewAttendanceDateAdditionRestricted;
            box.IsNewAttendeeAdditionAllowed = this.IsNewAttendeeAdditionAllowed;
            box.IsNotesSectionHidden = this.IsNotesSectionHidden;
            box.NotesSectionLabel = this.NotesSectionLabel;
            box.AddPersonAs = this.AddPersonAs;
            box.AttendanceOccurrenceGuid = occurrenceData.IsNewOccurrence ? (Guid?)null : occurrenceData.AttendanceOccurrence.Guid;
            box.IsLongListDisabled = this.IsLongListDisabled;
            box.IsDidNotMeetDisabled = this.IsDidNotMeetDisabled;

            SetAddGroupMemberPageUrl( occurrenceData, box );
            SetOccurrenceDateOptions( occurrenceData, box );
            SetLocationOptions( rockContext, occurrenceData, box );
            SetScheduleOptions( occurrenceData, box );
            SetRosterOptions( occurrenceData, box );

            var allowedAttendanceTypeValues = this.AttendanceOccurrenceTypeValues;

            if ( allowedAttendanceTypeValues.Any() )
            {
                box.IsAttendanceOccurrenceTypesSectionShown = allowedAttendanceTypeValues.Count > 1;
                box.AttendanceOccurrenceTypesSectionLabel = this.AttendanceOccurrenceTypesLabel;
                box.AttendanceOccurrenceTypes = allowedAttendanceTypeValues
                    .Select( attendenceOccurrenceType => new ListItemBag
                    {
                        Text = attendenceOccurrenceType.Value,
                        Value = attendenceOccurrenceType.Guid.ToString(),
                    } )
                    .ToList();
                if ( box.AttendanceOccurrenceTypes.Count == 1 )
                {
                    box.SelectedAttendanceOccurrenceTypeValue = box.AttendanceOccurrenceTypes.First().Value;
                }
                else
                {
                    var attendanceOccurrenceTypeValue = allowedAttendanceTypeValues.Where( a => a.Id == occurrenceData.AttendanceOccurrence?.AttendanceTypeValueId ).Select( a => a.Guid.ToString() ).FirstOrDefault();
                    box.SelectedAttendanceOccurrenceTypeValue = box.AttendanceOccurrenceTypes
                        .FirstOrDefault( attendanceOccurrenceType => attendanceOccurrenceType.Value == attendanceOccurrenceTypeValue )?.Value;
                }
            }

            if ( occurrenceData.AttendanceOccurrence.Id > 0 )
            {
                box.Notes = occurrenceData.AttendanceOccurrence.Notes;
                box.IsDidNotMeetChecked = occurrenceData.AttendanceOccurrence.DidNotOccur ?? false;
            }

            box.Roster = GetRoster( rockContext, occurrenceData );

            return box;
        }

        private List<GroupAttendanceDetailAttendanceBag> GetRoster( RockContext rockContext, OccurrenceData occurrenceData )
        {
            // Load the attendance for the selected attendance occurrence.
            var attendedPersonIds = new List<int>();

            if ( occurrenceData.AttendanceOccurrence.Id > 0 )
            {
                // Get the list of people who attended.
                // These may or may not be group members.
                attendedPersonIds = occurrenceData.AttendanceOccurrence.Attendees
                    .Where( a =>
                        a.DidAttend.HasValue &&
                        a.DidAttend.Value &&
                        a.PersonAliasId.HasValue )
                    .Select( a => a.PersonAlias.PersonId )
                    .Distinct()
                    .ToList();
            }

            // Get the group members.
            var groupMemberService = new GroupMemberService( rockContext );

            // Add any group members not on that list.
            var unattendedPersonIds = groupMemberService
                .Queryable()
                .AsNoTracking()
                .Where( m =>
                    m.GroupId == occurrenceData.Group.Id
                    && m.GroupMemberStatus != GroupMemberStatus.Inactive
                    && !attendedPersonIds.Contains( m.PersonId ) )
                .Select( m => m.PersonId )
                .Distinct()
                .ToList();

            var lavaTemplate = this.ListItemDetailsTemplate;
            var mergeFields = this.RequestContext.GetCommonMergeFields( null );

            // Create the roster.
            return new PersonService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Include( p => p.Aliases )
                .Where( p => attendedPersonIds.Contains( p.Id ) || unattendedPersonIds.Contains( p.Id ) )
                .ToList()
                .Select( p => GetRosterAttendeeBag( p, attendedPersonIds.Contains( p.Id ), occurrenceData.Group.Members.FirstOrDefault( g => g.PersonId == p.Id ) ) )
                .ToList();
        }

        private GroupAttendanceDetailAttendanceBag GetRosterAttendeeBag( Person person, bool hasAttended, GroupMember groupMember )
        {
            var mergeFields = this.RequestContext.GetCommonMergeFields();
            mergeFields.Add( MergeFieldKeys.Person, person );
            mergeFields.Add( MergeFieldKeys.Attended, hasAttended );
            mergeFields.Add( MergeFieldKeys.GroupMember, groupMember );

            return new GroupAttendanceDetailAttendanceBag
            {
                PersonGuid = person.Guid,
                PersonAliasId = person.PrimaryAliasId,
                NickName = person.NickName,
                LastName = person.LastName,
                DidAttend = hasAttended,
                CampusGuid = person.PrimaryCampusId.HasValue ? person.PrimaryCampus.Guid : ( Guid? ) null,
                ItemTemplate = this.ListItemDetailsTemplate.ResolveMergeFields( mergeFields )
            };
        }

        private void SetRosterOptions( OccurrenceData occurrenceData, GroupAttendanceDetailInitializationBox box )
        {
            var groupMembersTerm = occurrenceData.Group.GroupType.GroupMemberTerm.Pluralize();
            box.GroupMembersSectionLabel = groupMembersTerm;
        }

        private void SetOccurrenceDateOptions( OccurrenceData occurrenceData, GroupAttendanceDetailInitializationBox box )
        {
            // Set occurrence date options.
            box.AttendanceOccurrenceDate = occurrenceData.AttendanceOccurrence.OccurrenceDate;
            // TODO JMH Figure out when to use GroupAttendanceDetailDateSelectionMode.ScheduledDatePicker.
            if ( occurrenceData.IsNewOccurrence )
            {
                box.AttendanceOccurrenceDateSelectionMode = GroupAttendanceDetailDateSelectionMode.DatePicker;
            }
            else
            {
                box.AttendanceOccurrenceDateSelectionMode = GroupAttendanceDetailDateSelectionMode.Specific;
            }
        }

        private void SetLocationOptions( RockContext rockContext, OccurrenceData occurrenceData, GroupAttendanceDetailInitializationBox box )
        {
            // Set location options.
            if ( occurrenceData.AttendanceOccurrence.LocationId.HasValue )
            {
                box.LocationGuid = occurrenceData.AttendanceOccurrence.Location.Guid;
                box.LocationSelectionMode = GroupAttendanceDetailLocationSelectionMode.Specific;
                box.LocationLabel = new LocationService( rockContext ).GetPath( occurrenceData.AttendanceOccurrence.LocationId.Value );
            }
            else
            {
                box.LocationGuid = null;
                box.LocationSelectionMode = GroupAttendanceDetailLocationSelectionMode.GroupLocationPicker;
                box.LocationLabel = null;
            }
        }

        private void SetScheduleOptions( OccurrenceData occurrenceData, GroupAttendanceDetailInitializationBox box )
        {
            if ( occurrenceData.AttendanceOccurrence.Location != null && occurrenceData.AttendanceOccurrence.Schedule != null )
            {
                box.ScheduleGuid = occurrenceData.AttendanceOccurrence.Schedule.Guid;
                box.ScheduleSelectionMode = GroupAttendanceDetailScheduleSelectionMode.Specific;
                box.ScheduleLabel = occurrenceData.AttendanceOccurrence.Schedule.Name;
            }
            else
            {
                box.ScheduleGuid = null;
                box.ScheduleSelectionMode = GroupAttendanceDetailScheduleSelectionMode.GroupLocationSchedulePicker;
                box.ScheduleLabel = null;
            }
        }

        /// <summary>
        /// Adds the person as group member but does not save changes.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="rockContext">The rock context.</param>
        private GroupMember AddPersonAsGroupMember( Group group, Person person, RockContext rockContext )
        {
            var groupMemberService = new GroupMemberService( rockContext );
            var role = new GroupTypeRoleService( rockContext ).Get( group.GroupType.DefaultGroupRoleId ?? 0 );

            var groupMember = new GroupMember
            {
                Id = 0,
                GroupId = group.Id
            };

            // Check to see if the person is already a member of the group/role.
            var existingGroupMember = groupMemberService.GetByGroupIdAndPersonIdAndGroupRoleId( group.Id, person.Id, group.GroupType.DefaultGroupRoleId ?? 0 );

            if ( existingGroupMember != null )
            {
                return existingGroupMember;
            }

            groupMember.PersonId = person.Id;
            groupMember.GroupRoleId = role.Id;
            groupMember.GroupMemberStatus = GroupMemberStatus.Active;

            if ( groupMember.Id.Equals( 0 ) )
            {
                groupMemberService.Add( groupMember );
            }

            return groupMember;
        }

        /// <summary>
        /// Method to email attendance summary.
        /// </summary>
        private void EmailAttendanceSummary( int attendanceOccurrenceId, int groupId )
        {
            try
            {
                using ( var rockContext = new RockContext() )
                {
                    // Get records from the database right before emailing to make sure we have fresh data.
                    var attendanceOccurrence = new AttendanceOccurrenceService( rockContext ).Get( attendanceOccurrenceId );
                    var group = new GroupService( rockContext ).Get( groupId );

                    var currentPerson = this.GetCurrentPerson();

                    var mergeObjects = this.RequestContext.GetCommonMergeFields( currentPerson );
                    mergeObjects.Add( MergeFieldKeys.Group, group );
                    mergeObjects.Add( MergeFieldKeys.AttendanceOccurrence, attendanceOccurrence );
                    mergeObjects.Add( MergeFieldKeys.AttendanceNoteLabel, this.AttendanceNoteLabel );

                    var recipients = new List<Person>();

                    foreach ( var summaryEmailRecipient in this.SummaryEmailRecipients )
                    {
                        switch ( summaryEmailRecipient )
                        {
                            case SendSummaryEmailType.GroupLeaders:
                                var leaders = new GroupMemberService( rockContext )
                                    .Queryable( "Person" )
                                    .AsNoTracking()
                                    .Where( m => m.GroupId == group.Id )
                                    .Where( m => m.IsArchived == false )
                                    .Where( m => m.GroupMemberStatus != GroupMemberStatus.Inactive )
                                    .Where( m => m.GroupRole.IsLeader );

                                recipients.AddRange( leaders.Where( a => !string.IsNullOrEmpty( a.Person.Email ) ).Select( a => a.Person ) );
                                break;

                            case SendSummaryEmailType.AllGroupMembers:
                                var allGroupMembers = new GroupMemberService( rockContext )
                                    .Queryable( "Person" )
                                    .AsNoTracking()
                                    .Where( m => m.GroupId == group.Id )
                                    .Where( m => m.IsArchived == false )
                                    .Where( m => m.GroupMemberStatus != GroupMemberStatus.Inactive );

                                recipients.AddRange( allGroupMembers.Where( a => !string.IsNullOrEmpty( a.Person.Email ) ).Select( a => a.Person ) );
                                break;

                            case SendSummaryEmailType.GroupAdministrator:
                                if ( group.GroupType.ShowAdministrator
                                     && group.GroupAdministratorPersonAliasId.HasValue
                                     && group.GroupAdministratorPersonAlias.Person.Email.IsNotNullOrWhiteSpace() )
                                {
                                    recipients.Add( group.GroupAdministratorPersonAlias.Person );
                                }

                                break;

                            case SendSummaryEmailType.ParentGroupLeaders:
                                if ( group.ParentGroupId.HasValue )
                                {
                                    var parentLeaders = new GroupMemberService( rockContext )
                                        .Queryable( "Person" )
                                        .AsNoTracking()
                                        .Where( m => m.GroupId == group.ParentGroupId.Value )
                                        .Where( m => m.IsArchived == false )
                                        .Where( m => m.GroupMemberStatus != GroupMemberStatus.Inactive )
                                        .Where( m => m.GroupRole.IsLeader );

                                    recipients.AddRange( parentLeaders.Where( a => !string.IsNullOrEmpty( a.Person.Email ) ).Select( a => a.Person ) );
                                }

                                break;

                            case SendSummaryEmailType.IndividualEnteringAttendance:
                                if ( !string.IsNullOrEmpty( currentPerson.Email ) )
                                {
                                    recipients.Add( currentPerson );
                                }

                                break;

                            default:
                                break;
                        }
                    }

                    foreach ( var recipient in recipients )
                    {
                        var emailMessage = new RockEmailMessage( this.AttendanceEmailTemplateGuid );
                        emailMessage.AddRecipient( new RockEmailMessageRecipient( recipient, mergeObjects ) );
                        emailMessage.CreateCommunicationRecord = false;
                        emailMessage.Send();
                    }
                }
            }
            catch ( SystemException ex )
            {
                RockLogger.Log.Error( RockLogDomains.Group, ex );
            }
        }

        private static Attendance CreateAttendance( int? personAliasId, int? campusId, DateTime startDateTime, bool? didAttend )
        {
            return new Attendance
            {
                PersonAliasId = personAliasId,
                CampusId = campusId,
                StartDateTime = startDateTime,
                DidAttend = didAttend
            };
        }

        /// <summary>
        /// Gets the Campus ID from the AttendanceOccurrence Location.
        /// <para>If not present, then try the Group's Campus.</para>
        /// <para>Finally, if not set there, get the Campus from the Campus filter, if present in the request.</para>
        /// </summary>
        private int? GetAttendanceCampusId( int? occurrenceLocationCampusId, int? groupLocationCampusId, Guid? campusGuidOverride )
        {
            var campusId = occurrenceLocationCampusId ?? groupLocationCampusId;

            if ( !campusId.HasValue && this.IsCampusFilteringAllowed && campusGuidOverride.HasValue )
            {
                campusId = CampusCache.GetId( campusGuidOverride.Value );
            }

            return campusId;
        }

        #endregion

        #region Helper Classes and Enums

        private class OccurrenceDataClientService
        {
            private readonly AttendanceOccurrenceService _attendanceOccurrenceService;
            private readonly AttendanceService _attendanceService;
            private readonly GroupAttendanceDetail _block;
            private readonly GroupService _groupService;
            private readonly LocationService _locationService;
            private readonly ScheduleService _scheduleService;

            internal OccurrenceDataClientService( GroupAttendanceDetail block, GroupService groupService, AttendanceService attendanceService, AttendanceOccurrenceService attendanceOccurrenceService, LocationService locationService, ScheduleService scheduleService )
            {
                _block = block ?? throw new ArgumentNullException( nameof( block ) );
                _groupService = groupService ?? throw new ArgumentNullException( nameof( groupService ) );
                _attendanceService = attendanceService ?? throw new ArgumentNullException( nameof( attendanceService ) );
                _attendanceOccurrenceService = attendanceOccurrenceService ?? throw new ArgumentNullException( nameof( attendanceOccurrenceService ) );
                _locationService = locationService ?? throw new ArgumentNullException( nameof( locationService ) );
                _scheduleService = scheduleService ?? throw new ArgumentNullException( nameof( scheduleService ) );
            }

            internal OccurrenceData GetOccurrenceData( AttendanceOccurrenceSearchParameters occurrenceDataSearchParameters = null, bool asNoTracking = false )
            {
                var occurrenceData = new OccurrenceData();

                if ( occurrenceDataSearchParameters == null )
                {
                    occurrenceDataSearchParameters = GetAttendanceOccurrenceSearchParameters();
                }

                if ( !TrySetGroup( occurrenceData, occurrenceDataSearchParameters, asNoTracking ) )
                {
                    return occurrenceData;
                }

                if ( !TrySetAttendanceOccurrence( occurrenceData, occurrenceDataSearchParameters, asNoTracking ) )
                {
                    return occurrenceData;
                }

                occurrenceData.Campus = GetCampus( occurrenceDataSearchParameters );

                return occurrenceData;
            }

            internal AttendanceOccurrenceSearchParameters GetAttendanceOccurrenceSearchParameters( Guid? attendanceOccurrenceGuidOverride = null, DateTime? attendanceOccurrenceDateOverride = null, Guid? locationGuidOverride = null, Guid? scheduleGuidOverride = null, int? campusIdOverride = null )
            {
                // Get defaults.
                var occurrenceDataSearchParameters = new AttendanceOccurrenceSearchParameters
                {
                    AttendanceOccurrenceDate = _block.DatePageParameter ?? _block.OccurrencePageParameter,
                    AttendanceOccurrenceId = _block.OccurrenceIdPageParameter,
                    GroupId = _block.GroupIdPageParameter,
                    LocationId = _block.LocationIdPageParameter,
                    ScheduleId = _block.ScheduleIdPageParameter,
                };

                // If AttendanceOccurrenceId is set, then assume a specific AttendanceOccurrence is being requested.
                if ( attendanceOccurrenceGuidOverride.HasValue )
                {
                    occurrenceDataSearchParameters.AttendanceOccurrenceId = _attendanceOccurrenceService.GetId( attendanceOccurrenceGuidOverride.Value );
                }

                // If Group ID page parameter not found, then use the Group Schedule.
                if ( !occurrenceDataSearchParameters.ScheduleId.HasValue && occurrenceDataSearchParameters.GroupId.HasValue )
                {
                    occurrenceDataSearchParameters.ScheduleId = GetGroup( occurrenceDataSearchParameters.GroupId.Value, asNoTracking: true )?.ScheduleId;
                }

                // If overrides are allowed, then use the overrides.
                if ( !_block.IsNewAttendanceDateAdditionRestricted )
                {
                    if ( locationGuidOverride.HasValue )
                    {
                        occurrenceDataSearchParameters.LocationId = _locationService.GetId( locationGuidOverride.Value );
                    }

                    if ( attendanceOccurrenceDateOverride.HasValue )
                    {
                        occurrenceDataSearchParameters.AttendanceOccurrenceDate = attendanceOccurrenceDateOverride.Value;
                    }

                    if ( scheduleGuidOverride.HasValue )
                    {
                        occurrenceDataSearchParameters.ScheduleId = _scheduleService.GetId( scheduleGuidOverride.Value );
                    }

                    if ( _block.IsCampusFilteringAllowed )
                    {
                        // Set the search parameter.
                        occurrenceDataSearchParameters.CampusId = campusIdOverride;

                        // Update the user preference.
                        var campusIdUserPreference = _block.CampusIdBlockUserPreference;
                        if ( campusIdUserPreference != campusIdOverride )
                        {
                            _block.CampusIdBlockUserPreference = campusIdOverride;
                        }
                    }
                }

                return occurrenceDataSearchParameters;
            }

            internal AttendanceOccurrenceSearchParameters GetAttendanceOccurrenceSearchParameters( GroupAttendanceDetailSaveAttendanceOccurrenceRequestBag bag )
            {
                return GetAttendanceOccurrenceSearchParameters( bag.AttendanceOccurrenceGuid, bag.AttendanceOccurrenceDate, bag.LocationGuid, bag.ScheduleGuid, bag.CampusGuid.HasValue ? CampusCache.GetId( bag.CampusGuid.Value ) : null );
            }

            internal bool Save( OccurrenceData occurrenceData, GroupAttendanceDetailSaveAttendanceOccurrenceRequestBag bag )
            {
                if ( occurrenceData.IsValid != true )
                {
                    return false;
                }

                if ( occurrenceData.IsReadOnly )
                {
                    // If an occurrence already exists but not by the supplied OccurrenceID,
                    // then return an error to the client.
                    occurrenceData.ErrorMessage = "An occurrence already exists for this group for the selected date, location, and schedule that you've selected. Please return to the list and select that occurrence to update it's attendance.";
                    return false;
                }

                if ( occurrenceData.IsNewOccurrence )
                {
                    _attendanceOccurrenceService.Add( occurrenceData.AttendanceOccurrence );
                }
                else
                {
                    _attendanceOccurrenceService.Attach( occurrenceData.AttendanceOccurrence );
                }

                occurrenceData.AttendanceOccurrence.Notes = !_block.IsNotesSectionHidden ? bag.Notes : string.Empty;
                occurrenceData.AttendanceOccurrence.DidNotOccur = bag.DidNotOccur;

                // Set the attendance type.
                if ( bag.AttendanceTypeGuid.HasValue )
                {
                    var allowedAttendanceTypes = _block.AttendanceOccurrenceTypeValues;
                    var attendanceTypeDefinedValue = allowedAttendanceTypes.FirstOrDefault( a => a.Guid == bag.AttendanceTypeGuid.Value );
                    occurrenceData.AttendanceOccurrence.AttendanceTypeValueId = attendanceTypeDefinedValue?.Id;
                }

                // Update the attendees.
                // JMH This will trigger updates in subscribers.
                var existingAttendees = occurrenceData.AttendanceOccurrence.Attendees.ToList();

                var campusId = new Lazy<int?>( () =>
                {
                    var occurrenceLocationCampusId = _locationService.GetCampusIdForLocation( occurrenceData.AttendanceOccurrence.LocationId );
                    return _block.GetAttendanceCampusId( occurrenceLocationCampusId, occurrenceData.Group.CampusId, bag.CampusGuid );
                } );

                if ( bag.DidNotOccur )
                {
                    // If did not meet was selected and this was a manually entered occurrence (not based on a schedule/location)
                    // then just delete all the attendance records instead of tracking a 'did not meet' value
                    if ( !occurrenceData.AttendanceOccurrence.ScheduleId.HasValue )
                    {
                        foreach ( var attendance in existingAttendees )
                        {
                            _attendanceService.Delete( attendance );
                        }
                    }
                    // If the occurrence is based on a schedule and there are attendees,
                    // then set the did not meet flags on existing attendees.
                    else if ( existingAttendees.Any() )
                    {
                        foreach ( var attendance in existingAttendees )
                        {
                            attendance.DidAttend = null;
                        }
                    }
                    // If the occurrence is based on a schedule and there are no attendees,
                    // then add new attendees with did not meet flags set.
                    else
                    {
                        foreach ( var attendee in bag.Attendees.Where( a => a.PersonAliasId.HasValue ) )
                        {
                            var attendance = CreateAttendance(
                                attendee.PersonAliasId,
                                campusId.Value,
                                occurrenceData.AttendanceOccurrence.Schedule != null && occurrenceData.AttendanceOccurrence.Schedule.HasSchedule() ? occurrenceData.AttendanceOccurrence.OccurrenceDate.Date.Add( occurrenceData.AttendanceOccurrence.Schedule.StartTimeOfDay ) : occurrenceData.AttendanceOccurrence.OccurrenceDate,
                                false );

                            if ( !attendance.IsValid )
                            {
                                occurrenceData.ErrorMessage = attendance.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                                return false;
                            }

                            occurrenceData.AttendanceOccurrence.Attendees.Add( attendance );
                        }
                    }
                }
                else
                {
                    // Validate the AttendanceOccurrence before updating the Attendance records.
                    if ( !occurrenceData.AttendanceOccurrence.IsValid )
                    {
                        occurrenceData.ErrorMessage = occurrenceData.AttendanceOccurrence.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                        return false;
                    }

                    DateTime startDateTime;

                    if ( occurrenceData.AttendanceOccurrence.Schedule != null
                        && occurrenceData.AttendanceOccurrence.Schedule.HasSchedule() )
                    {
                        startDateTime = occurrenceData.AttendanceOccurrence.OccurrenceDate.Date.Add(
                            occurrenceData.AttendanceOccurrence.Schedule.StartTimeOfDay );
                    }
                    else
                    {
                        startDateTime = occurrenceData.AttendanceOccurrence.OccurrenceDate;
                    }

                    foreach ( var attendee in bag.Attendees )
                    {
                        var attendance = existingAttendees
                            .Where( a => a.PersonAliasId == attendee.PersonAliasId )
                            .FirstOrDefault();

                        if ( attendance == null )
                        {
                            if ( attendee.PersonAliasId.HasValue )
                            {
                                attendance = CreateAttendance(
                                    attendee.PersonAliasId,
                                    campusId.Value,
                                    startDateTime,
                                    attendee.DidAttend );

                                // Check that the attendance record is valid
                                if ( !attendance.IsValid )
                                {
                                    occurrenceData.ErrorMessage = attendance.ValidationResults.Select( a => a.ErrorMessage ).ToList().AsDelimited( "<br />" );
                                    return false;
                                }

                                occurrenceData.AttendanceOccurrence.Attendees.Add( attendance );
                            }
                        }
                        else
                        {
                            // Otherwise, only record that they attended -- don't change their attendance startDateTime.
                            attendance.DidAttend = attendee.DidAttend;
                        }
                    }
                }

                return true;
            }

            internal bool TrySetGroup( OccurrenceData occurrenceData, AttendanceOccurrenceSearchParameters attendanceOccurrenceSearchParameters, bool asNoTracking )
            {
                // Get Group.
                if ( attendanceOccurrenceSearchParameters.GroupId.HasValue )
                {
                    occurrenceData.Group = GetGroup( attendanceOccurrenceSearchParameters.GroupId.Value, asNoTracking );
                }
                else
                {
                    occurrenceData.Group = null;
                    occurrenceData.ErrorMessage = "Group ID was not provided.";
                    return false;
                }

                if ( occurrenceData.Group == null )
                {
                    occurrenceData.IsGroupNotFoundError = true;
                    return false;
                }

                // Authorize Group.
                var currentPerson = _block.GetCurrentPerson();

                if ( !occurrenceData.Group.IsAuthorized( Authorization.MANAGE_MEMBERS, currentPerson )
                    && !occurrenceData.Group.IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    occurrenceData.IsNotAuthorizedError = true;
                    return false;
                }

                return true;
            }

            internal bool TrySetAttendanceOccurrence( OccurrenceData occurrenceData, AttendanceOccurrenceSearchParameters occurrenceDataSearchParameters, bool asNoTracking = false )
            {
                (var isReadOnly, var attendanceOccurrence) = GetExistingAttendanceOccurrence( occurrenceDataSearchParameters, asNoTracking );

                occurrenceData.IsReadOnly = isReadOnly;
                occurrenceData.AttendanceOccurrence = attendanceOccurrence;

                if ( occurrenceData.AttendanceOccurrence == null )
                {
                    if ( _block.IsNewAttendanceDateAdditionRestricted )
                    {
                        occurrenceData.IsNoAttendanceOccurrencesError = true;
                        return false;
                    }

                    occurrenceData.AttendanceOccurrence = GetNewAttendanceOccurrence( occurrenceDataSearchParameters );
                }

                return occurrenceData.AttendanceOccurrence != null;
            }

            private CampusCache GetCampus( AttendanceOccurrenceSearchParameters occurrenceDataSearchParameters )
            {
                var campusId = occurrenceDataSearchParameters.CampusId;

                if ( campusId.HasValue )
                {
                    return CampusCache.Get( campusId.Value );
                }

                return null;
            }

            private Group GetGroup( int groupId, bool asNoTracking )
            {
                // Get Group.
                var query = _groupService
                    .AsNoFilter()
                    .Include( g => g.GroupType )
                    .Include( g => g.Schedule )
                    .Where( g => g.Id == groupId );

                if ( asNoTracking )
                {
                    query = query.AsNoTracking();
                }

                return query.FirstOrDefault();
            }

            private (bool IsReadOnly, AttendanceOccurrence AttendanceOccurrence) GetExistingAttendanceOccurrence( AttendanceOccurrenceSearchParameters occurrenceDataSearchParameters, bool asNoTracking )
            {
                // Try to set the AttendanceOccurrence from Attendance Occurrence ID.
                if ( occurrenceDataSearchParameters.AttendanceOccurrenceId.HasValue && occurrenceDataSearchParameters.AttendanceOccurrenceId.Value > 0 )
                {
                    var query = _attendanceOccurrenceService
                        .AsNoFilter()
                        .Include( a => a.Schedule )
                        .Include( a => a.Location )
                        .Include( a => a.Attendees )
                        .Where( a => a.Id == occurrenceDataSearchParameters.AttendanceOccurrenceId.Value );

                    if ( asNoTracking )
                    {
                        query = query.AsNoTracking();
                    }

                    var attendanceOccurrence = query.FirstOrDefault();

                    if ( attendanceOccurrence != null )
                    {
                        return (false, attendanceOccurrence);
                    }
                }

                // If no specific Attendance Occurrence ID was specified, try to find a matching occurrence from Date, GroupId, Location, ScheduleId.
                if ( occurrenceDataSearchParameters.AttendanceOccurrenceDate.HasValue )
                {
                    var attendanceOccurrence = _attendanceOccurrenceService.Get( occurrenceDataSearchParameters.AttendanceOccurrenceDate.Value.Date, occurrenceDataSearchParameters.GroupId, occurrenceDataSearchParameters.LocationId, occurrenceDataSearchParameters.ScheduleId, "Location,Schedule" );
                    return ( attendanceOccurrence != null ? true : false, attendanceOccurrence );
                }

                return ( false, null );
            }

            private AttendanceOccurrence GetNewAttendanceOccurrence( AttendanceOccurrenceSearchParameters attendanceOccurrenceSearchParameters )
            {
                var attendanceOccurrence = new AttendanceOccurrence
                {
                    GroupId = attendanceOccurrenceSearchParameters.GroupId,
                    LocationId = attendanceOccurrenceSearchParameters.LocationId,
                    OccurrenceDate = attendanceOccurrenceSearchParameters.AttendanceOccurrenceDate ?? RockDateTime.Today,
                    ScheduleId = attendanceOccurrenceSearchParameters.ScheduleId
                };

                if ( attendanceOccurrenceSearchParameters.LocationId.HasValue )
                {
                    attendanceOccurrence.Location = _locationService.Get( attendanceOccurrenceSearchParameters.LocationId.Value );
                }

                if ( attendanceOccurrenceSearchParameters.ScheduleId.HasValue )
                {
                    attendanceOccurrence.Schedule = _scheduleService.Get( attendanceOccurrenceSearchParameters.ScheduleId.Value );
                }

                return attendanceOccurrence;
            }
        }

        private class OccurrenceData
        {
            public bool IsValid => ErrorMessage.IsNullOrWhiteSpace() && !IsGroupNotFoundError && !IsNoAttendanceOccurrencesError && !IsNotAuthorizedError && Group != null && AttendanceOccurrence != null;

            public bool IsNewOccurrence => AttendanceOccurrence?.Id == 0;

            public string ErrorMessage { get; set; }

            public Group Group { get; internal set; }

            public AttendanceOccurrence AttendanceOccurrence { get; internal set; }

            public CampusCache Campus { get; internal set; }

            public bool IsGroupNotFoundError { get; internal set; }

            public bool IsNotAuthorizedError { get; internal set; }

            public bool IsNoAttendanceOccurrencesError { get; internal set; }
            public bool IsReadOnly { get; internal set; }
        }

        private class AttendanceOccurrenceSearchParameters
        {
            /// <summary>
            /// Gets or sets the attendance occurrence identifier.
            /// <para>If set to an existing Attendance Occurrence ID, then no other search parameters are necessary.</para>
            /// </summary>
            public int? AttendanceOccurrenceId { get; internal set; }

            public int? GroupId { get; set; }

            public DateTime? AttendanceOccurrenceDate { get; set; }

            public int? LocationId { get; set; }

            public int? ScheduleId { get; set; }

            public int? CampusId { get; set; }
        }

        /// <summary>
        /// Represents an attendance summary email recipient type.
        /// </summary>
        private enum SendSummaryEmailType
        {
            /// <summary>
            /// Group Leaders
            /// </summary>
            GroupLeaders = 1,

            /// <summary>
            /// All Group Members (note: all active group members)
            /// </summary>
            AllGroupMembers = 2,

            /// <summary>
            /// Parent Group Leaders
            /// </summary>
            ParentGroupLeaders = 3,

            /// <summary>
            /// Individual Entering Attendance
            /// </summary>
            IndividualEnteringAttendance = 4,

            /// <summary>
            /// Group Administrator
            /// </summary>
            GroupAdministrator = 5
        }

        #endregion
    }
}
