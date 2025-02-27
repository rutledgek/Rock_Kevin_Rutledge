//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
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

/** A bag that contains all data for the Volunteer Generosity Analysis block. */
export type VolunteerGenerositySetupBag = {
    /** Gets or sets the estimated refresh time of the persisted dataset.  */
    estimatedRefreshTime: number;

    /** Gets or sets the last updated date time.  */
    lastUpdated?: string | null;

    /** Gets or sets the bool that shows/hides the campus filter. */
    showCampusFilter: boolean;

    /** Gets or sets the list of unique campuses. */
    uniqueCampuses?: string[] | null;

    /** Gets or sets the list of unique groups  */
    uniqueGroups?: string[] | null;
};
