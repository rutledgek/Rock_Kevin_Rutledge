using System.ComponentModel;
using System.Data.Entity;
using System.Drawing.Text;
using System.Linq;

using Rock.Data;
using Rock.Mobile;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v15 to create a 'Mobile Application Users' rest security group, and add existing mobile application users into that group.
    /// </summary>
    [DisplayName( "Rock Update Helper v15.0 - Update Mobile Application Rest User Security." )]
    [Description( "This job will create (if doesn't exist) a new 'Mobile Application Users' security group, and add all current mobile application rest users to that group." )]
    public class PostV15DataMigrationsMobileApplicationUserRestGroup : RockJob
    {
        /// <inheritdoc />
        public override void Execute()
        {
            using( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                var groupService = new GroupService( rockContext );
                var userLoginService = new UserLoginService( rockContext );

                var mobileAppUsersGuid = SystemGuid.Group.GROUP_MOBILE_APPLICATION_USERS.AsGuid();
                var group = groupService
                    .Queryable()
                    .Include( g => g.Members )
                    .Where( g => g.Guid == mobileAppUsersGuid )
                    .SingleOrDefault();

                // Create the rest security group if it doesn't exist.
                if ( group == null )
                {
                    group = new Group
                    {
                        Guid = SystemGuid.Group.GROUP_MOBILE_APPLICATION_USERS.AsGuid(),
                        IsSystem = true,
                        ParentGroupId = null,
                        GroupTypeId = 1,
                        CampusId = null,
                        Name = "RSR - Mobile Application Users",
                        Description = "Group of mobile application people to use for endpoint authorization.",
                        IsSecurityRole = true,
                        IsActive = true,
                        ElevatedSecurityLevel = Utility.Enums.ElevatedSecurityLevel.Extreme,
                        IsPublic = true
                    };

                    groupService.Add( group );
                    rockContext.SaveChanges();
                }

                // All of our mobile application sites.
                var mobileSiteUserIds = SiteCache.GetAllActiveSites()
                    .Where( s => s.SiteType == Rock.Model.SiteType.Mobile )
                    .Select( s => s.AdditionalSettings.FromJsonOrNull<AdditionalSiteSettings>()?.ApiKeyId )
                    .Where( id => id.HasValue )
                    .ToList();

                // Get all of the user logins associated with a mobile site.
                var mobileSitePeopleIds = userLoginService.Queryable()
                    .Where( ul => mobileSiteUserIds.Contains( ul.Id ) && ul.PersonId.HasValue )
                    .Select( ul => ul.PersonId )
                    .ToList();

                var groupType = GroupTypeCache.Get( group.GroupTypeId );

                // Add each mobile application user into the group.
                foreach ( var personId in mobileSitePeopleIds )
                {
                    // Ensure there are no duplicates.
                    if( !group.Members.Any( gm => gm.PersonId == personId ) )
                    {
                        
                        GroupMember groupMember = new GroupMember();

                        // We explicitly set the group here because if we don't,
                        // the group will later be loaded from the database via
                        // a pre-save hook.
                        groupMember.Group = group;
                        groupMember.GroupId = group.Id;
                        groupMember.PersonId = personId.Value;
                        groupMember.GroupRoleId = groupType?.DefaultGroupRoleId ?? 0;
                        groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                        group.Members.Add( groupMember );
                    }
                }

                rockContext.SaveChanges();
            }
        }
    }
}
