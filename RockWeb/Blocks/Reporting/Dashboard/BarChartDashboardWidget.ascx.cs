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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Rock.Data;
using Rock.Model;
using Rock.Reporting.Dashboard;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Reporting.Dashboard
{
    [DisplayName( "Bar Chart" )]
    [Category( "Reporting > Dashboard" )]
    [Description( "Bar Chart Dashboard Widget" )]
    [Rock.SystemGuid.BlockTypeGuid( "4E3A95C6-AB63-4920-9EA6-FA5F882B13AD" )]
    public partial class BarChartDashboardWidget : MetricChartDashboardWidget
    {
        #region Overrides

        protected override IRockChart GetChartControl()
        {
            return metricChart;
        }

        protected override void OnLoadChart()
        {
            var rockContext = new RockContext();
            var metricService = new MetricService( rockContext );
            string metricName = null;

            // Configure for Metric
            var metric = metricService.Get( this.MetricId.GetValueOrDefault( 0 ) );

            if ( metric != null )
            {
                metricName = metric.Title;

                if ( string.IsNullOrWhiteSpace( metricChart.XAxisLabel ) )
                {
                    // if XAxisLabel hasn't been set, and this is a metric, automatically set it to the metric.XAxisLabel
                    metricChart.XAxisLabel = metric.XAxisLabel;
                }

                if ( string.IsNullOrWhiteSpace( metricChart.YAxisLabel ) )
                {
                    // if YAxisLabel hasn't been set, and this is a metric, automatically set it to the metric.YAxisLabel
                    metricChart.YAxisLabel = metric.YAxisLabel;
                }
            }

            // Add the Metric data.
            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( this.GetAttributeValue( "SlidingDateRange" ) ?? string.Empty );
            var metricValueType = this.MetricValueType;
            var entityPartitionValues = this.GetPartitionEntityIdentifiers();
            var metricIdList = new List<int> { this.MetricId ?? 0 };

            var qryMetric = metricService.GetMetricValuesQuery( metricIdList,
                metricValueType,
                dateRange.Start,
                dateRange.End,
                entityPartitionValues );

            var metricItems = qryMetric.ToList();

            metricChart.SetChartDataItems( metricItems, metricName );

            nbMetricWarning.Visible = !metricItems.Any();
        }

        #endregion
    }
}