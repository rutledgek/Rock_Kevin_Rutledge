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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.Crm.Following
{
    /// <summary>
    /// Tests for the Following feature.
    /// </summary>
    [TestClass]
    public class FollowingTests
    {
        /// <summary>
        /// Verify that Followings added for either a Person entity or Person Alias entity.
        /// </summary>
        /// <remarks>
        /// This test verifies a fix for GitHub Issue #3012. (https://github.com/SparkDevNetwork/Rock/issues/3012)
        /// </remarks>
        [TestMethod]
        public void Following_GetFollowedPeople_ReturnsFollowedPersonAndPersonAliasEntities()
        {
            RemovePersonFollowings();

            var rockContext = new RockContext();
            var followingService = new FollowingService( rockContext );
            var personService = new PersonService( rockContext );

            var personEntityTypeId = EntityTypeCache.Get<Person>().Id;
            var personAliasEntityTypeId = EntityTypeCache.Get<PersonAlias>().Id;
            var tedDecker = personService.Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var billMarble = personService.Get( TestGuids.TestPeople.BillMarble.AsGuid() );
            var alishaMarble = personService.Get( TestGuids.TestPeople.AlishaMarble.AsGuid() );

            AddPersonFollowings();

            // Verify that the Followed Items requested for the Person entity include both the Person and PersonAlias entities.
            var followedItemsForPersonEntity = followingService.GetFollowedItems( personEntityTypeId, tedDecker.Id )
                .Cast<Person>()
                .ToList();

            Assert.IsNotNull( followedItemsForPersonEntity.FirstOrDefault( x => x.Id == billMarble.Id ) );
            Assert.IsNotNull( followedItemsForPersonEntity.FirstOrDefault( x => x.Id == alishaMarble.Id ) );

            // Verify that the Followed Items requested for the PersonAlias entity include both the Person and PersonAlias entities.
            var followedItemsForPersonAliasEntity = followingService.GetFollowedItems( personAliasEntityTypeId, tedDecker.Id )
                .Cast<PersonAlias>()
                .ToList();

            Assert.IsNotNull( followedItemsForPersonAliasEntity.FirstOrDefault( x => x.Person.Id == billMarble.Id ) );
            Assert.IsNotNull( followedItemsForPersonAliasEntity.FirstOrDefault( x => x.Person.Id == alishaMarble.Id ) );

            RemovePersonFollowings();
        }

        private void RemovePersonFollowings()
        {
            var rockContext = new RockContext();
            var followingService = new FollowingService( rockContext );

            var personEntityTypeId = EntityTypeCache.Get<Person>().Id;
            var personAliasEntityTypeId = EntityTypeCache.Get<PersonAlias>().Id;

            // Remove all Person followings for Ted Decker.
            var tedDeckerGuid = TestGuids.TestPeople.TedDecker.AsGuid();
            var followedPeopleTedDeckerQuery = followingService.Queryable()
                .Where( f => f.PersonAlias.Person.Guid == tedDeckerGuid // Id == tedDecker.PrimaryAliasId
                && ( f.EntityTypeId == personEntityTypeId || f.EntityTypeId == personAliasEntityTypeId ) );

            followingService.DeleteRange( followedPeopleTedDeckerQuery.ToList() );

            rockContext.SaveChanges();
        }

        private void AddPersonFollowings()
        {
            var rockContext = new RockContext();
            var followingService = new FollowingService( rockContext );
            var personService = new PersonService( rockContext );

            var personEntityTypeId = EntityTypeCache.Get<Rock.Model.Person>().Id;
            var personAliasEntityTypeId = EntityTypeCache.Get<Rock.Model.PersonAlias>().Id;
            var tedDecker = personService.Get( TestGuids.TestPeople.TedDecker.AsGuid() );
            var billMarble = personService.Get( TestGuids.TestPeople.BillMarble.AsGuid() );
            var alishaMarble = personService.Get( TestGuids.TestPeople.AlishaMarble.AsGuid() );

            // Ted Decker --> Bill Marble (via Person entity)
            Rock.Model.Following following;
            following = new Rock.Model.Following();
            following.EntityTypeId = personEntityTypeId;
            following.EntityId = billMarble.Id;
            following.PersonAliasId = tedDecker.PrimaryAliasId.Value;
            followingService.Add( following );

            // Ted Decker --> Alisha Marble (via PersonAlias entity)
            following = new Rock.Model.Following();
            following.EntityTypeId = personAliasEntityTypeId;
            following.EntityId = alishaMarble.PrimaryAliasId.Value;
            following.PersonAliasId = tedDecker.PrimaryAliasId.Value;
            followingService.Add( following );

            rockContext.SaveChanges();
        }
    }
}
