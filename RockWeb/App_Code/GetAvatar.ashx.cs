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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing.Processors.Drawing;
using CacheManager.Core;
using DocumentFormat.OpenXml.Drawing.Charts;
using SixLabors.ImageSharp.Processing;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Data.Entity.Core.Metadata.Edm;

namespace RockWeb
{
    /// <summary>
    /// Handles retrieving file data from storage
    /// </summary>
    public class GetAvatar : IHttpAsyncHandler
    {
        // Implemented this as an IHttpAsyncHandler instead of IHttpHandler to improve performance
        // https://stackoverflow.com/questions/48528773/ihttphandler-versus-httptaskasynchandler-performance
        // Good overview on how to implement an IHttpAsyncHandler
        // https://www.madskristensen.net/blog/how-to-use-the-ihttpasynchandler-in-aspnet/


        private AsyncProcessorDelegate _Delegate;

        protected delegate void AsyncProcessorDelegate( HttpContext context );

        /// <summary>
        /// Called to initialize an asynchronous call to the HTTP handler. 
        /// </summary>
        /// <param name="context">An HttpContext that provides references to intrinsic server objects used to service HTTP requests.</param>
        /// <param name="cb">The AsyncCallback to call when the asynchronous method call is complete.</param>
        /// <param name="extraData">Any state data needed to process the request.</param>
        /// <returns>An IAsyncResult that contains information about the status of the process.</returns>
        public IAsyncResult BeginProcessRequest( HttpContext context, AsyncCallback cb, object extraData )
        {
            _Delegate = new AsyncProcessorDelegate( ProcessRequest );

            return _Delegate.BeginInvoke( context, cb, extraData );
        }

        /// <summary>
        /// Provides an end method for an asynchronous process. 
        /// </summary>
        /// <param name="result">An IAsyncResult that contains information about the status of the process.</param>
        public void EndProcessRequest( IAsyncResult result )
        {
            _Delegate.EndInvoke( result );
        }


        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void ProcessRequest( HttpContext context )
        {
            // Read query string parms
            var settings = ReadSettingsFromRequest( context.Request );

            // Make a copy of the mask so we don't resize the shared original
            var maskTemplate = AvatarHelper.AdultMaleMask.Clone( o => o.Crop( settings.Size, settings.Size ) );
            var test = AvatarHelper.AdultMaleMask.CloneAs<Rgba32>();
            test.Mutate( o => o.Resize( settings.Size, settings.Size ) );

            var mask = AvatarHelper.CreateIconMask( AvatarHelper.AdultMaleMask, RockColor.FromHex( settings.AvatarColors.ForegroundColor ) );

            var filePath = System.Web.Hosting.HostingEnvironment.MapPath( "~\\Content\\avatar.png" );


            mask.Save( filePath );
            maskTemplate.Save( System.Web.Hosting.HostingEnvironment.MapPath( "~\\Content\\avatar-temp.png" ) );
            AvatarHelper.AdultMaleMask.Save( System.Web.Hosting.HostingEnvironment.MapPath( "~\\Content\\avatar-temp2.png" ) );
            test.Save( System.Web.Hosting.HostingEnvironment.MapPath( "~\\Content\\avatar-temp3.png" ) );

            // var routeTable = RockCache.GetOrAddExisting( RoutesCacheKey, BuildDeepLinkRoutes ) as Dictionary<string, List<DeepLinkRoute>>;
            var cacheKey = GenerateCacheKey( settings );

            //System.IO.File.SetLastWriteTimeUtc(fileName, DateTime.UtcNow);

            Stream fileContent = null;
            try
            {
                string cachedFilePath = context.Request.MapPath( $"~/App_Data/Cache/{cacheKey}.{settings.AvatarFileExtension}" );

                if ( File.Exists( cachedFilePath ) )
                {
                    fileContent = FetchFromCache( cachedFilePath );
                }

                // The file is not in the cache so we'll create it
                if ( fileContent == null )
                {
                    fileContent = CreateAvatar( settings );
                }

                // Something has gone really wrong so we'll send an error message
                if ( fileContent == null )
                {
                    context.Response.StatusCode = System.Net.HttpStatusCode.InternalServerError.ConvertToInt();
                    context.Response.StatusDescription = "The requested avatar could not be created.";
                    context.ApplicationInstance.CompleteRequest();
                    return;
                }

                // Add cache validation headers
                context.Response.AddHeader( "Last-Modified", DateTime.Now.ToUniversalTime().ToString( "R" ) );
                context.Response.AddHeader( "ETag", DateTime.Now.ToString().XxHash() );

                // Return the avatar mime-type
                if ( settings.AvatarFormat == AvatarFormat.Png )
                {
                    context.Response.ContentType = "image/png";
                }
                else
                {
                    context.Response.ContentType = "image/svg+xml";
                }

                // Stream the contents of the file to the response
                using ( var responseStream = fileContent )
                {
                    context.Response.AddHeader( "content-disposition", "inline;filename=" + $"{cacheKey}.{settings.AvatarFileExtension}".UrlEncode() );
                    if ( responseStream.CanSeek )
                    {
                        responseStream.Seek( 0, SeekOrigin.Begin );
                    }
                    responseStream.CopyTo( context.Response.OutputStream );
                    context.Response.Flush();
                }
            }
            finally
            {
                if ( fileContent != null )
                {
                    fileContent.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler" /> instance.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler" /> instance is reusable; otherwise, false.</returns>
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Reads the settings from request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>RockWeb.AvatarSettings.</returns>
        private AvatarSettings ReadSettingsFromRequest( HttpRequest request )
        {
            var settings = new AvatarSettings();

            settings.AvatarColors.BackgroundColor = ( request.QueryString["BackgroundColor"] ?? "" ).ToString();
            settings.AvatarColors.ForegroundColor = ( request.QueryString["ForegroundColor"] ?? "" ).ToString();
            settings.AvatarColors.GenerateMissingColors();

            if ( request.QueryString["Size"] != null )
            {
                settings.Size = request.QueryString["Size"].AsInteger();
            }

            // Format (PNG is the default
            if ( request.QueryString["Format"] != null )
            {
                if ( request.QueryString["Format"].ToLower() == "svg" )
                {
                    settings.AvatarFormat = AvatarFormat.Svg;
                }
            }

            // Style
            if ( request.QueryString["Style"] != null )
            {
                if ( request.QueryString["Style"].ToLower() == "initials" )
                {
                    settings.AvatarStyle = AvatarStyle.Initials;
                }
            }

            // Gender
            if ( request.QueryString["Gender"] != null )
            {
                settings.Gender = (Gender) Enum.Parse( typeof(Gender), request.QueryString["Gender"], true );
            }

            // Text
            if ( request.QueryString["Text"] != null )
            {
                settings.Text = request.QueryString["Text"];
            }

            // Photo Id
            if ( request.QueryString["PhotoId"] != null )
            {
                settings.PhotoId = request.QueryString["PhotoId"].AsIntegerOrNull();
            }

            // Record Type Guid
            if ( request.QueryString["RecordTypeGuid"] != null )
            {
                settings.RecordTypeGuid = request.QueryString["RecordTypeGuid"].AsGuidOrNull();
            }

            // Person Guid
            if ( request.QueryString["PersonGuid"] != null )
            {
                settings.PersonGuid = request.QueryString["PersonGuid"].AsGuidOrNull();
            }

            // Person Id
            if ( request.QueryString["PersonId"] != null )
            {
                settings.PersonId = request.QueryString["PersonId"].AsIntegerOrNull();
            }

            // Bold
            if ( request.QueryString["Bold"] != null )
            {
                settings.IsBold = request.QueryString["Bold"].AsBoolean();
            }

            // Corner Radius + Circle
            if ( request.QueryString["Radius"] != null )
            {
                var radius = request.QueryString["Radius"];

                if ( radius.ToLower() == "circle" )
                {
                    settings.IsCircle = true;
                }
                else
                {
                    settings.CornerRadius = radius.AsInteger();
                }
            }

            // Prefers Light
            if ( request.QueryString["PrefersLight"] != null )
            {
                settings.PrefersLight = request.QueryString["PrefersLight"].AsBoolean();
            }

            return settings;
        }

        private Stream CreateAvatar( AvatarSettings settings )
        {
            return null;
        }

        private string GenerateCacheKey( AvatarSettings settings )
        {
            return $"{settings.Size}-{settings.Text}-{settings.AvatarColors.ForegroundColor}_{settings.AvatarColors.BackgroundColor}-{settings.CornerRadius}-{settings.AvatarFormat}-{settings.AvatarStyle}-{settings.Gender}-{settings.IsBold.ToString().Truncate(1)}-{settings.PersonGuid}{settings.PersonId}-{settings.PhotoId}-{settings.PrefersLight.ToString().Truncate( 1 )}-{settings.RecordTypeGuid}";
        }

        /// <summary>
        /// Attempts to retrieve the file from cache.
        /// </summary>
        /// <param name="physicalPath">The physical path.</param>
        /// <returns>Stream.</returns>
        private Stream FetchFromCache( string physicalPath )
        {
            try
            {
                return File.Open( physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read );
            }
            catch
            {
                // if it fails, return null, which will result in fetching it from the database instead
                return null;
            }
        }
    }

    public static class AvatarHelper
    {
        // Reuseable masks
        public static Image AdultMaleMask { get; } = new Image<Rgba32>( 1, 1 );
        public static Image AdultFemaleMask { get; } = new Image<Rgba32>( 1, 1 );
        public static Image ChildMaleMask { get; } = new Image<Rgba32>( 1, 1 );
        public static Image ChildFemaleMask { get; } = new Image<Rgba32>( 1, 1 );
        public static Image UnknownGenderMask { get; } = new Image<Rgba32>( 1, 1 );
        public static Image BusinessMask { get; } = new Image<Rgba32>( 1, 1 );

        static AvatarHelper() {

            // Load icon masks. 
            var folderPath = System.Web.Hosting.HostingEnvironment.MapPath( "~\\App_Data\\Avatar\\Masks\\" );

            // Adult male
            try
            {
                AdultMaleMask = Image.Load<RgbaVector>( folderPath + "adult-male.png" );
                AdultFemaleMask = Image.Load<RgbaVector>( folderPath + "adult-female.png" );
                ChildMaleMask = Image.Load<RgbaVector>( folderPath + "child-male.png" );
                ChildFemaleMask = Image.Load<RgbaVector>( folderPath + "child-female.png" );
                UnknownGenderMask = Image.Load<RgbaVector>( folderPath + "unknown-gender.png" );
                BusinessMask = Image.Load<RgbaVector>( folderPath + "business.png" );
            }
            catch {}
        }


        /// <summary>
        /// Creates an icon mask with the provided color.
        /// </summary>
        /// <param name="mask">The mask.</param>
        /// <param name="color">The color.</param>
        /// <returns>Image.</returns>
        public static Image CreateIconMask( Image mask, RockColor color )
        {
            // Logic for this comes from: https://stackoverflow.com/questions/52875516/how-to-compose-two-images-using-source-in-composition

            // Convert the RGB to floats
            var red = (float) color.R / 100;
            var green = ( float ) color.G / 100;
            var blue = ( float ) color.B / 100;

            // Create the "background" which is the solid color that we want our mask  to cut out
            var background = new Image<RgbaVector>( mask.Width, mask.Height, new RgbaVector( red, green, blue, 1 ) );  
            
            var processorCreator = new DrawImageProcessor(
                mask,
                Point.Empty,
                PixelColorBlendingMode.Normal,
                PixelAlphaCompositionMode.DestIn, // The destination where the destination and source overlap.
                1f
            );

            var pxProcessor = processorCreator.CreatePixelSpecificProcessor(
                Configuration.Default,
                background,
                mask.Bounds() );

            pxProcessor.Execute(); 

            return background;
        }
    }

    /// <summary>
    /// Class for storing the settings needed to create an avatar.
    /// </summary>
    public class AvatarSettings
    {
        /// <summary>
        /// Gets or sets the size (width / height) of the avatar image.
        /// </summary>
        /// <value>The size.</value>
        public int Size { get; set; } = 128;

        /// <summary>
        /// Gets or sets the avatar format.
        /// </summary>
        /// <value>The avatar format.</value>
        public AvatarFormat AvatarFormat { get; set; } = AvatarFormat.Png;

        /// <summary>
        /// Gets or sets the gender for the avatar icon.
        /// </summary>
        /// <value>The gender.</value>
        public Gender Gender { get; set; } = Gender.Unknown;

        /// <summary>
        /// Gets or sets the text for the initials.
        /// </summary>
        /// <value>The text.</value>
        public string Text {
            get
            {
                return _text;
            }
            set
            {
                _text = value.Truncate( 2 );
            }
        }
        private string _text = string.Empty;

        /// <summary>
        /// Gets or sets the photo identifier.
        /// </summary>
        /// <value>The photo identifier.</value>
        public int? PhotoId { get; set; }

        /// <summary>
        /// Gets or sets the record type unique identifier.
        /// </summary>
        /// <value>The record type unique identifier.</value>
        public Guid? RecordTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the person unique identifier.
        /// </summary>
        /// <value>The person unique identifier.</value>
        public Guid? PersonGuid { get; set; }

        /// <summary>
        /// Gets or sets the person identifier.
        /// </summary>
        /// <value>The person identifier.</value>
        public int? PersonId { get; set; }


        /// <summary>
        /// Gets the avatar file extension from the format.
        /// </summary>
        /// <value>The avatar file extension.</value>
        public string AvatarFileExtension
        {
            get
            {
                return AvatarFormat.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the avatar colors (foreground / background).
        /// </summary>
        /// <value>The avatar colors.</value>
        public AvatarColors AvatarColors { get; set; } = new AvatarColors();

        /// <summary>
        /// Gets or sets the avatar style.
        /// </summary>
        /// <value>The avatar style.</value>
        public AvatarStyle AvatarStyle { get; set; } = AvatarStyle.Icon;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is bold.
        /// </summary>
        /// <value><c>true</c> if this instance is bold; otherwise, <c>false</c>.</value>
        public bool IsBold { get; set; } = false;

        /// <summary>
        /// Gets or sets the corner radius.
        /// </summary>
        /// <value>The corner radius.</value>
        public int CornerRadius { get; set; } = 0;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is circle.
        /// </summary>
        /// <value><c>true</c> if this instance is circle; otherwise, <c>false</c>.</value>
        public bool IsCircle { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether [prefers light].
        /// </summary>
        /// <value><c>true</c> if [prefers light]; otherwise, <c>false</c>.</value>
        public bool PrefersLight { get; set; } = true;
    }

    public enum AvatarFormat
    {
        Png = 0,
        Svg = 1
    }

    public enum AvatarStyle
    {
        Icon = 0,
        Initials = 1
    }

    /// <summary>
    /// Class AvatarColors.
    /// </summary>
    public class AvatarColors
    {
        /// <summary>
        /// Gets or sets the foreground color for the avatar.
        /// </summary>
        /// <value>The color of the foreground.</value>
        public string ForegroundColor { get; set; }

        /// <summary>
        /// Gets or sets the background color for the avatar.
        /// </summary>
        /// <value>The color of the background.</value>

        public string BackgroundColor { get; set; }


        /// <summary>
        /// Generates any missing colors.
        /// </summary>
        public void GenerateMissingColors()
        {
            // If we were provided both a foreground color and background color then we don't need
            // to do anything
            if ( this.ForegroundColor.IsNotNullOrWhiteSpace() && this.BackgroundColor.IsNotNullOrWhiteSpace() )
            {
                return;
            }

            // If no color was provided find a random color
            if ( this.ForegroundColor.IsNullOrWhiteSpace() && this.BackgroundColor.IsNullOrWhiteSpace() )
            {
                var random = new Random();
                var randomIndex = random.Next( 0, colorValues.Length );
                this.ForegroundColor = colorValues[randomIndex];
            }

            // At this point we only have one color so generate the missing one
            if ( this.BackgroundColor.IsNullOrWhiteSpace() )
            {
                this.BackgroundColor = GetContrastColor( this.ForegroundColor );
            }

            if ( this.ForegroundColor.IsNullOrWhiteSpace() )
            {
                this.ForegroundColor = GetContrastColor( this.BackgroundColor );
            }
        }

        public string GetContrastColor( string source )
        {
            var sourceColor = RockColor.FromHex( source );

            // Create dark contrast using the Practical UI darkest recipe
            var darkColor = RockColor.FromHex( source );
            darkColor.Saturation = .60;
            darkColor.Luminosity = .20;

            // Create light contrast using the Practical UI ligest recipe
            var lightColor = RockColor.FromHex( source );
            lightColor.Saturation = .88;
            lightColor.Luminosity = .87;

            // Return the color with the greatest contrast
            var darkContrast = RockColor.CalculateContrastRatio( sourceColor, darkColor );
            var lightContrast = RockColor.CalculateContrastRatio( sourceColor, lightColor );

            if ( darkContrast > lightContrast )
            {
                return darkColor.ToHex();
            }

            return lightColor.ToHex();
        }

        // List of random colors to use when no colors were provided
        // From: https://github.com/LasseRafn/ui-avatars/blob/master/Utils/Input.php
        private string[] colorValues = new string[] {
            "#5e35b1",
            "#512da8",
            "#4527a0",
            "#311b92",
            "#8e24aa",
            "#7b1fa2",
            "#6a1b9a",
            "#4a148c",
            "#3949ab",
            "#303f9f",
            "#283593",
            "#1a237e",
            "#1e88e5",
            "#1976d2",
            "#1565c0",
            "#0d47a1",
            "#039be5",
            "#0288d1",
            "#0277bd",
            "#01579b",
            "#00acc1",
            "#0097a7",
            "#00838f",
            "#006064",
            "#00897b",
            "#00796b",
            "#00695c",
            "#004d40",
            "#43a047",
            "#388e3c",
            "#2e7d32",
            "#1b5e20",
            "#7cb342",
            "#689f38",
            "#558b2f",
            "#33691e",
            "#c0ca33",
            "#afb42b",
            "#9e9d24",
            "#827717",
            "#fdd835",
            "#fbc02d",
            "#f9a825",
            "#f57f17",
            "#ffb300",
            "#ffa000",
            "#ff8f00",
            "#ff6f00",
            "#fb8c00",
            "#f57c00",
            "#ef6c00",
            "#e65100",
            "#f4511e",
            "#e64a19",
            "#d84315",
            "#bf360c",
            "#6d4c41",
            "#5d4037",
            "#4e342e",
            "#3e2723",
            "#546e7a",
            "#455a64",
            "#37474f",
            "#263238",
            "#F44336",
            "#E53935",
            "#D32F2F",
            "#C62828"
        };
    }
}