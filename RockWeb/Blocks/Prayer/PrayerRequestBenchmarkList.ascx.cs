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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Prayer
{
    [DisplayName( "Prayer Request Benchmark List" )]
    [Category( "Prayer" )]
    [Description( "Lists prayer requests in a standard way for benchmarking purposes." )]

    [Rock.SystemGuid.BlockTypeGuid( "664bb067-ad59-4674-955c-a46854801aed" )]
    public partial class PrayerRequestBenchmarkList : RockBlock
    {
        #region Properties

        /// <summary>
        /// Gets or sets the available attributes.
        /// </summary>
        /// <value>
        /// The available attributes.
        /// </value>
        public List<AttributeCache> AvailableAttributes { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            if ( ViewState["AvailableAttributeIds"] != null )
            {
                AvailableAttributes = ( ViewState["AvailableAttributeIds"] as int[] ).Select( a => AttributeCache.Get( a ) ).ToList();
            }

            AddDynamicControls();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindAttributes();
                AddDynamicControls();
                BindGrid();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["AvailableAttributeIds"] = AvailableAttributes == null ? null : AvailableAttributes.Select( a => a.Id ).ToArray();

            return base.SaveViewState();
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the GridRebind event of the gPrayerRequests control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gPrayerRequests_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the attributes.
        /// </summary>
        private void BindAttributes()
        {
            // Parse the attribute filters
            AvailableAttributes = new List<AttributeCache>();

            var rockContext = new RockContext();
            int entityTypeId = EntityTypeCache.GetId<PrayerRequest>().Value;

            var attributes = new AttributeService( rockContext ).GetByEntityTypeQualifier( entityTypeId, string.Empty, string.Empty, true )
                .Where( a => a.IsGridColumn )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToAttributeCacheList();

            foreach ( var attribute in attributes )
            {
                if ( attribute.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    AvailableAttributes.Add( attribute );
                }
            }
        }

        /// <summary>
        /// Adds the attribute columns.
        /// </summary>
        private void AddDynamicControls()
        {
            // Clear dynamic controls so we can re-add them
            RemoveAttributeAndButtonColumns();

            if ( AvailableAttributes != null )
            {
                foreach ( var attribute in AvailableAttributes )
                {
                    bool columnExists = gPrayerRequests.Columns.OfType<AttributeField>().FirstOrDefault( a => a.AttributeId == attribute.Id ) != null;
                    if ( !columnExists )
                    {
                        AttributeField boundField = new AttributeField();
                        boundField.DataField = attribute.Key;
                        boundField.AttributeId = attribute.Id;
                        boundField.HeaderText = attribute.Name;
                        boundField.ItemStyle.HorizontalAlign = HorizontalAlign.Left;

                        gPrayerRequests.Columns.Add( boundField );
                    }
                }
            }
        }

        private void RemoveAttributeAndButtonColumns()
        {
            // Remove attribute columns
            foreach ( var column in gPrayerRequests.Columns.OfType<AttributeField>().ToList() )
            {
                gPrayerRequests.Columns.Remove( column );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var prayerRequestService = new PrayerRequestService( rockContext );

            SortProperty sortProperty = gPrayerRequests.SortProperty;
            if ( sortProperty == null )
            {
                sortProperty = new SortProperty( new GridViewSortEventArgs( nameof( PrayerRequest.EnteredDateTime ), SortDirection.Descending ) );
            }

            var qryPrayerRequests = prayerRequestService.Queryable()
                .Sort( sortProperty );

            var prayerRequestList = qryPrayerRequests.ToList();

            gPrayerRequests.DataSource = prayerRequestList;
            gPrayerRequests.EntityTypeId = EntityTypeCache.Get<PrayerRequest>().Id;
            gPrayerRequests.DataBind();
        }

        #endregion
    }
}