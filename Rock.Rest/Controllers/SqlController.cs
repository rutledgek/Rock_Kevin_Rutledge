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
using System.Data.SqlClient;
using System.Web.Http;

using Rock.Data;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Controller to handle SQL interface.
    /// </summary>
    [Rock.SystemGuid.RestControllerGuid( "21d96f82-1fd9-427a-b9cf-fa4214153a61" )]
    public class SqlController : ApiController
    {
        [System.Web.Http.Route( "api/Sql/ExecuteQuery" )]
        [Rock.SystemGuid.RestActionGuid( "95cfb019-ee3b-44f3-9a60-e6d67498e773" )]
        [HttpPost]
        public IHttpActionResult PostExecuteQuery( [FromBody] ExecuteQueryRequest request )
        {
            var messages = new List<QueryMessage>();
            var resultSets = new List<QueryResultSet>();

            if ( request.Query.IsNotNullOrWhiteSpace() )
            {
                using ( var rockContext = new RockContext() )
                {
                    var connection = rockContext.Database.Connection as SqlConnection ?? throw new Exception( "Invalid connection type." );

                    connection.FireInfoMessageEventOnUserErrors = true;
                    connection.InfoMessage += ( s, e ) =>
                    {
                        foreach ( SqlError error in e.Errors )
                        {
                            messages.Add( new QueryMessage
                            {
                                Message = error.Message,
                                Code = error.Number,
                                Level = error.Class,
                                State = error.State,
                                LineNumber = error.LineNumber
                            } );
                        }
                    };

                    connection.Open();

                    using ( var command = connection.CreateCommand() )
                    {
                        command.CommandText = request.Query;
                        command.StatementCompleted += ( s, e ) =>
                        {
                            messages.Add( new QueryMessage
                            {
                                Message = $"({e.RecordCount} {( e.RecordCount == 1 ? "row" : "rows" )} affected)"
                            } );
                        };

                        using ( var reader = command.ExecuteReader() )
                        {
                            if ( reader.HasRows )
                            {
                                resultSets.Add( ReadResultSet( reader ) );
                            }

                            while ( reader.NextResult() )
                            {
                                resultSets.Add( ReadResultSet( reader ) );
                            }
                        }
                    }
                }
            }

            var result = new ExecuteQueryResult
            {
                Messages = messages,
                ResultSets = resultSets
            };

            return Ok( result );
        }

        private QueryResultSet ReadResultSet( SqlDataReader reader )
        {
            var resultSet = new QueryResultSet
            {
                Columns = new List<QueryColumn>(),
                Rows = new List<List<object>>()
            };

            for ( int i = 0; i < reader.FieldCount; i++ )
            {
                resultSet.Columns.Add( new QueryColumn
                {
                    Name = reader.GetName( i ),
                    Type = QueryColumnType.String
                } );
            }

            if ( reader.HasRows )
            {
                while ( reader.Read() )
                {
                    var row = new List<object>();

                    for ( int i = 0; i < reader.FieldCount; i++ )
                    {
                        if ( reader.IsDBNull( i ) )
                        {
                            row.Add( null );
                        }
                        else
                        {
                            row.Add( reader.GetValue( i ).ToString() );
                        }
                    }

                    resultSet.Rows.Add( row );
                }
            }

            return resultSet;
        }

        public class ExecuteQueryRequest
        {
            public string Query { get; set; }
        }

        public class ExecuteQueryResult
        {
            public List<QueryMessage> Messages { get; set; }

            public List<QueryResultSet> ResultSets { get; set; }
        }

        public class QueryResultSet
        {
            public List<QueryColumn> Columns { get; set; }

            public List<List<object>> Rows { get; set; }
        }

        public class QueryColumn
        {
            public string Name { get; set; }

            public QueryColumnType Type { get; set; }
        }

        public enum QueryColumnType
        {
            String = 0
        }

        public class QueryMessage
        {
            public string Message { get; set; }

            public int? Code { get; set; }

            public int? Level { get; set; }

            public int? State { get; set; }

            public int? LineNumber { get; set; }
        }
    }
}