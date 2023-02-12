using System;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using Rock.Web.Cache;

namespace Rock.Tests.Integration
{
    public static class TestHelper
    {
        static TestHelper()
        {
            Trace.Listeners.Add( new System.Diagnostics.TextWriterTraceListener( Console.Out ) );
        }

        //private static stopWatch = new System.Diagnostics.Stopwatch();

        public static Guid StartTask( string testName )
        {
            //var stopWatch = new System.Diagnostics.Stopwatch();
            //stopWatch.Start();

            return Guid.NewGuid();
        }

        public static void StopTask( Guid taskGuid )
        {
            Trace.WriteLine( "Task completed" );

            //Trace.WriteLine( string.Format( "Test AlphaNumericCodesShouldSkipBadCodes took {0} ms.", stopWatch.ElapsedMilliseconds ) );
        }
        public static void Log( string message )
        {
            var timestamp = DateTime.Now.ToString( "HH:mm:ss.fff" );
            Trace.WriteLine( $"[{timestamp}] {message}" );
        }

    }
}