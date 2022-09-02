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
using System.Linq;
using Rock.Data;
using Rock.Model;

namespace Rock.Chart
{
    /// <summary>
    /// A factory that produces datasets for use with Rock charts.
    /// </summary>
    /// <remarks>
    /// This factory replaces the data functions of the ChartJsTimeSeriesDataFactory and
    /// ChartJsTimeSeriesDataFactory components. The ChartJs-specific presentation elements have been
    /// decoupled for greater flexibility. 
    /// </remarks>
    public static class ChartDataFactory
    {
        /// <summary>
        /// Gets the recommended time interval for quantizing a set of datapoints in a time series into categories.
        /// </summary>
        /// <param name="dataset"></param>
        /// <returns></returns>
        public static ChartJsTimeSeriesTimeScaleSpecifier GetRecommendedCategoryIntervalForTimeSeries( ChartJsTimeSeriesDataset dataset )
        {
            if ( dataset?.DataPoints == null || !dataset.DataPoints.Any() )
            {
                return ChartJsTimeSeriesTimeScaleSpecifier.Auto;
            }

            var minDate = dataset.DataPoints.Min( dp => dp.DateTime );
            var maxDate = dataset.DataPoints.Max( dp => dp.DateTime );

            var interval = GetRecommendedCategoryIntervalForTimeSeries( new List<DateTime> { minDate, maxDate } );
            return interval;
        }

        /// <summary>
        /// Gets the recommended time interval for quantizing a set of datapoints in a time series into categories.
        /// </summary>
        /// <param name="dataPointDateTimeValues"></param>
        /// <returns></returns>
        public static ChartJsTimeSeriesTimeScaleSpecifier GetRecommendedCategoryIntervalForTimeSeries( List<DateTime> dataPointDateTimeValues )
        {
            if ( dataPointDateTimeValues == null || !dataPointDateTimeValues.Any() )
            {
                return ChartJsTimeSeriesTimeScaleSpecifier.Auto;
            }

            var minDate = dataPointDateTimeValues.Min();
            var maxDate = dataPointDateTimeValues.Max();

            if ( minDate.Year != maxDate.Year )
            {
                return ChartJsTimeSeriesTimeScaleSpecifier.Year;
            }
            else if ( minDate.Month != maxDate.Month )
            {
                return ChartJsTimeSeriesTimeScaleSpecifier.Month;
            }
            else if ( minDate.Day != minDate.Day )
            {
                return ChartJsTimeSeriesTimeScaleSpecifier.Day;
            }
            return ChartJsTimeSeriesTimeScaleSpecifier.Auto;
        }

        /// <summary>
        /// Convert a collection of time series datasets to category-value datasets, where the categories represent discrete periods in the specified time scale.
        /// </summary>
        /// <remarks>
        /// Quantizing the data points in this way will substantially improve the performance of Chart.js for large data sets.
        /// </remarks>
        /// <param name="dataset"></param>
        /// <param name="timeScale">
        /// The timescale used to separate the datapoints into categories.
        /// If not specified, the scale is determined as the best fit for the date range of the datapoints.
        /// </param>
        /// <returns></returns>
        public static ChartJsCategorySeriesDataset GetCategorySeriesFromTimeSeries( ChartJsTimeSeriesDataset dataset, ChartJsTimeSeriesTimeScaleSpecifier? timeScale = null )
        {
            const string DateFormatStringMonthYear = "MMM yyyy";
            const string DateFormatStringDayMonthYear = "d";

            var datapoints = dataset.DataPoints;

            var datasetQuantized = new ChartJsCategorySeriesDataset();

            datasetQuantized.Name = dataset.Name;
            datasetQuantized.BorderColor = dataset.BorderColor;
            datasetQuantized.FillColor = dataset.FillColor;

            if ( timeScale == ChartJsTimeSeriesTimeScaleSpecifier.Auto )
            {
                timeScale = GetRecommendedCategoryIntervalForTimeSeries( dataset );
            }

            if ( timeScale == ChartJsTimeSeriesTimeScaleSpecifier.Day )
            {
                var quantizedDataPoints = datapoints
                    .GroupBy( x => new { Day = x.DateTime } )
                    .Select( x => new ChartJsCategorySeriesDataPoint
                    {
                        Category = x.Key.Day.ToString( DateFormatStringDayMonthYear ),
                        Value = x.Sum( y => y.Value ),
                        SortKey = x.Key.Day.ToString( "yyyyMMdd" ),
                    } )
                    .OrderBy( x => x.SortKey )
                    .ToList();

                datasetQuantized.DataPoints = quantizedDataPoints.Cast<IChartJsCategorySeriesDataPoint>().ToList();
            }
            else if ( timeScale == ChartJsTimeSeriesTimeScaleSpecifier.Month )
            {
                var quantizedDataPoints = datapoints
                    .GroupBy( x => new { Month = new DateTime( x.DateTime.Year, x.DateTime.Month, 1 ) } )
                    .Select( x => new ChartJsCategorySeriesDataPoint
                    {
                        Category = x.Key.Month.ToString( DateFormatStringMonthYear ),
                        Value = x.Sum( y => y.Value ),
                        SortKey = x.Key.Month.ToString( "yyyyMM" ),
                    } )
                    .OrderBy( x => x.SortKey )
                    .ToList();

                datasetQuantized.DataPoints = quantizedDataPoints.Cast<IChartJsCategorySeriesDataPoint>().ToList();
            }
            else if ( timeScale == ChartJsTimeSeriesTimeScaleSpecifier.Year )
            {
                var quantizedDataPoints = datapoints
                    .GroupBy( x => new { Year = new DateTime( x.DateTime.Year, 1, 1 ) } )
                    .Select( x => new ChartJsCategorySeriesDataPoint
                    {
                        Category = x.Key.Year.ToString( "yyyy" ),
                        Value = x.Sum( y => y.Value ),
                        SortKey = x.Key.Year.ToString( "yyyy" ),
                    } )
                    .OrderBy( x => x.SortKey )
                    .ToList();

                datasetQuantized.DataPoints = quantizedDataPoints.Cast<IChartJsCategorySeriesDataPoint>().ToList();
            }
            else
            {
                // Get the sum of all datapoints.
                var quantizedDataPoints = datapoints
                    .Select( x => new ChartJsCategorySeriesDataPoint
                    {
                        Category = dataset.Name,
                        Value = datapoints.Sum( y => y.Value ),
                        SortKey = dataset.Name
                    } )
                    .ToList();

                datasetQuantized.DataPoints = quantizedDataPoints.Cast<IChartJsCategorySeriesDataPoint>().ToList();
            }

            return datasetQuantized;
        }

        /// <summary>
        /// Create a category series of data values from a collection of Rock Chart Data Items.
        /// </summary>
        /// <param name="chartDataItems"></param>
        /// <param name="defaultSeriesName"></param>
        /// <returns></returns>
        public static List<ChartJsCategorySeriesDataset> GetCategorySeriesFromChartData( IEnumerable<IChartData> chartDataItems, string defaultSeriesName = null )
        {
            if ( string.IsNullOrWhiteSpace( defaultSeriesName ) )
            {
                defaultSeriesName = "(unknown)";
            }

            var itemsBySeries = chartDataItems.GroupBy( k => k.SeriesName, v => v );

            var firstItem = chartDataItems.FirstOrDefault();
            var isChartJsDataPoint = firstItem is IChartJsCategorySeriesDataPoint;
            var datasets = new List<ChartJsCategorySeriesDataset>();
            foreach ( var series in itemsBySeries )
            {
                var categoryName = string.IsNullOrWhiteSpace( series.Key ) ? defaultSeriesName : series.Key;
                List<IChartJsCategorySeriesDataPoint> dataPoints;
                if ( isChartJsDataPoint )
                {
                    dataPoints = chartDataItems.Cast<IChartJsCategorySeriesDataPoint>().ToList();
                }
                else
                {
                    dataPoints = chartDataItems.Where( x => x.SeriesName == series.Key )
                        .Select( x => ( IChartJsCategorySeriesDataPoint ) new ChartJsCategorySeriesDataPoint
                        {
                            Category = categoryName,
                            Value = x.YValue ?? 0,
                        } )
                        .ToList();
                }
                var dataset = new ChartJsCategorySeriesDataset
                {
                    Name = categoryName,
                    DataPoints = dataPoints
                };

                datasets.Add( dataset );
            }

            return datasets;
        }

        /// <summary>
        /// Create a time series of data values from a collection of Rock Chart Data Items.
        /// </summary>
        /// <param name="chartDataItems"></param>
        /// <param name="defaultSeriesName"></param>
        /// <returns></returns>
        public static List<ChartJsTimeSeriesDataset> GetTimeSeriesFromChartData( IEnumerable<IChartData> chartDataItems, string defaultSeriesName )
        {
            var firstItem = chartDataItems.FirstOrDefault();
            var isChartJsDataPoint = firstItem is IChartJsTimeSeriesDataPoint;

            var itemsBySeries = chartDataItems.GroupBy( k => k.SeriesName, v => v );
            var timeDatasets = new List<ChartJsTimeSeriesDataset>();
            foreach ( var series in itemsBySeries )
            {
                List<IChartJsTimeSeriesDataPoint> dataPoints;
                if ( isChartJsDataPoint )
                {
                    dataPoints = chartDataItems.Cast<IChartJsTimeSeriesDataPoint>().ToList();
                }
                else
                {
                    dataPoints = chartDataItems.Where( x => x.SeriesName == series.Key )
                        .Select( x => ( IChartJsTimeSeriesDataPoint ) new ChartJsTimeSeriesDataPoint
                        {
                            DateTime = GetDateTimeFromJavascriptMilliseconds( x.DateTimeStamp ),
                            Value = x.YValue ?? 0
                        } )
                        .ToList();
                }
                var dataset = new ChartJsTimeSeriesDataset
                {
                    Name = string.IsNullOrWhiteSpace( series.Key ) ? defaultSeriesName : series.Key,
                    DataPoints = dataPoints
                };

                timeDatasets.Add( dataset );
            }

            return timeDatasets;
        }

        /// <summary>
        /// Convert a collection of Metrics into a collection of datasets suitable for plotting as a Category vs Value chart.
        /// Each Metric represents a Category in the dataset, and the Value of the category is the sum of the metric values.
        /// </summary>
        /// <param name="values"></param>
        /// <param name="valueType"></param>
        /// <param name="defaultSeriesName"></param>
        /// <returns></returns>
        public static ChartJsCategorySeriesDataset GetCategoryDatasetFromMetrics( IEnumerable<MetricValue> values, MetricValueType valueType, string defaultSeriesName )
        {
            if ( string.IsNullOrWhiteSpace( defaultSeriesName ) )
            {
                defaultSeriesName = "(unknown)";
            }

            var itemsByMetricId = values.Where( v => v.MetricValueType == valueType )
                .GroupBy( k => k.MetricId, v => v );

            var rockContext = new RockContext();
            var metricService = new MetricService( rockContext );

            var dataset = new ChartJsCategorySeriesDataset();
            dataset.DataPoints = new List<IChartJsCategorySeriesDataPoint>();
            foreach ( var metricIdGroup in itemsByMetricId )
            {
                var metric = metricService.Get( metricIdGroup.Key );

                var categoryName = string.IsNullOrWhiteSpace( metric.Title ) ? defaultSeriesName : metric.Title;
                if ( valueType == Rock.Model.MetricValueType.Goal )
                {
                    categoryName += " Goal";
                }
                var datapoint = new ChartJsCategorySeriesDataPoint
                {
                    Category = categoryName,
                    Value = metricIdGroup.Sum( v => v.YValue ?? 0 )
                };

                dataset.DataPoints.Add( datapoint );
            }

            return dataset;
        }

        #region Internal methods

        internal static DateTime GetDateTimeFromJavascriptMilliseconds( long millisecondsAfterEpoch )
        {
            return new DateTime( 1970, 1, 1 ).AddTicks( millisecondsAfterEpoch * 10000 );
        }

        #endregion
    }
}