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
using System.Diagnostics;
using Rock.Checkr.Constants;
using Color = SixLabors.ImageSharp.Color;
using System.Threading;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using Humanizer;
using Parlot.Fluent;
using static Lucene.Net.Store.Lock;
using Mono.CSharp;
using Enum = System.Enum;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SixLabors.Fonts;
using FontFamily = SixLabors.Fonts.FontFamily;
using DocumentFormat.OpenXml.Drawing;
using OpenXmlPowerTools;
using Point = SixLabors.ImageSharp.Point;

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

        // Delegate setup variables
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

            string cacheFolder = context.Request.MapPath( $"~/App_Data/Avatar/Cache/" );
            string cachedFilePath = $"{cacheFolder}{settings.CacheKey}.png";

            // Process any cache refresh request for single item
            if ( context.Request.QueryString["RefreshItemCache"] != null && context.Request.QueryString["RefreshItemCache"].AsBoolean() )
            {
                RefreshItemCache( cachedFilePath );
            }

            // Process any cache refresh for all items
            if ( context.Request.QueryString["RefreshCache"] != null && context.Request.QueryString["RefreshCache"].AsBoolean() )
            {
                RefreshCache( cacheFolder );
            }

            Stream fileContent = null;
            try
            {
                fileContent = FetchFromCache( cachedFilePath );

                // The file is not in the cache so we'll create it
                if ( fileContent == null )
                {
                    fileContent = AvatarHelper.CreateAvatar( settings );
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

                // Configure client to cache image locally for 1 week
                //context.Response.Cache.SetCacheability( HttpCacheability.Public ); REMOVE BEFORE FLIGHT
                //context.Response.Cache.SetMaxAge( new TimeSpan( 7, 0, 0, 0, 0 ) ); REMOVE BEFORE FLIGHT

                context.Response.ContentType = "image/png";

                // Stream the contents of the file to the response
                using ( var responseStream = fileContent )
                {
                    context.Response.AddHeader( "content-disposition", "inline;filename=" + $"{settings.CacheKey}.png".UrlEncode() );
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
        /// Refreshs the cache for a specific item
        /// </summary>
        /// <param name="filePath"></param>
        private void RefreshItemCache( string filePath )
        {
            // Ensure the person is allowed to refresh the cache
            if ( !IsPersonAllowedRefeshCache() )
            {
                return;
            }

            // Delete the file if it exists
            if ( File.Exists( filePath ) )
            {
                File.Delete( filePath );
            }
        }

        /// <summary>
        /// Refreshes the cache on all cached avatars
        /// </summary>
        /// <param name="cacheFolder"></param>
        private void RefreshCache( string cacheFolder )
        {
            // Ensure the person is allowed to refresh the cache
            if ( !IsPersonAllowedRefeshCache() )
            {
                return;
            }

            // Delete all files
            foreach ( string sFile in System.IO.Directory.GetFiles( cacheFolder, "*.png" ) )
            {
                File.Delete( sFile );
            }
        }

        /// <summary>
        /// Determines if the person is in a role that allows them to refresh cache
        /// </summary>
        /// <returns></returns>
        private bool IsPersonAllowedRefeshCache()
        {
            var rockContext = new RockContext();
            var currentUser = new UserLoginService( rockContext ).GetByUserName( UserLogin.GetCurrentUserName() );
            var currentPerson = currentUser != null ? currentUser.Person : null;

            return RoleCache.AllRoles()
                        .Where( r =>
                            AvatarHelper.RoleGuidsAuthorizedToRefreshCache.Contains( r.Guid )
                            && r.IsPersonInRole( currentPerson.Guid )
                        )
                        .Any();
        }

        /// <summary>
        /// Reads the settings from request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>RockWeb.AvatarSettings.</returns>
        private AvatarSettings ReadSettingsFromRequest( HttpRequest request )
        {
            var settings = new AvatarSettings();

            // Calculate the physical path to store the cached files to
            settings.CachePath = request.MapPath( $"~/App_Data/Avatar/Cache/");

            // Colors
            settings.AvatarColors.BackgroundColor = ( request.QueryString["BackgroundColor"] ?? "" ).ToString();
            settings.AvatarColors.ForegroundColor = ( request.QueryString["ForegroundColor"] ?? "" ).ToString();
            settings.AvatarColors.GenerateMissingColors();

            // Size
            if ( request.QueryString["Size"] != null )
            {
                settings.Size = request.QueryString["Size"].AsInteger();
            }

            // Style
            if ( request.QueryString["Style"] != null )
            {
                if ( request.QueryString["Style"].ToLower() == "initials" )
                {
                    settings.AvatarStyle = AvatarStyle.Initials;
                }
            }

            // Age Classification
            if ( request.QueryString["AgeClassification"] != null )
            {
                settings.AgeClassification = ( AgeClassification ) Enum.Parse( typeof( AgeClassification ), request.QueryString["AgeClassification"], true );
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
            if ( request.QueryString["RecordTypeId"] != null )
            {
                settings.RecordTypeId = request.QueryString["RecordTypeId"].AsIntegerOrNull();
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

            // Logic for loading from Person Objects
            // ----------------------------------------

            Person person = null;
            // Person Guid
            if ( request.QueryString["PersonGuid"] != null )
            {
                settings.PersonGuid = request.QueryString["PersonGuid"].AsGuidOrNull();

                if ( settings.PersonGuid.HasValue)
                {
                    person = new PersonService( new RockContext() ).Get( settings.PersonGuid.Value );
                }
            }

            // Person Id
            if ( request.QueryString["PersonId"] != null )
            {
                settings.PersonId = request.QueryString["PersonId"].AsIntegerOrNull();

                if ( settings.PersonId.HasValue )
                {
                    person = new PersonService( new RockContext() ).Get( settings.PersonId.Value );
                }
            }

            // Load configuration from the person object
            if (person != null )
            {
                settings.RecordTypeId = person.RecordStatusValueId;
                settings.Gender = person.Gender;
                settings.Text = person.Initials;
                settings.AgeClassification = person.AgeClassification;
                settings.PhotoId = person.PhotoId;
            }

            return settings;
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
                if ( File.Exists( physicalPath ) )
                {
                    // Touch the file to update the last modified date
                    File.SetLastWriteTimeUtc( physicalPath, DateTime.UtcNow );

                    return File.Open( physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read );
                }

                return null;
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

        public static int RecordTypeIdBusiness { get; } = 0;

        public static int RecordTypeIdNamelessPerson { get; } = 0;

        public static int RecordTypeIdPerson { get; } = 0;

        public static List<Guid> RoleGuidsAuthorizedToRefreshCache { get; } = new List<Guid>();

        // Fonts for avatars
        private static FontCollection _fontCollection = null;

        static AvatarHelper() {

            // Load icon masks. 
            var folderPath = System.Web.Hosting.HostingEnvironment.MapPath( "~\\App_Data\\Avatar\\" );

            // Load role guids that are allowed to refresh the cache
            RoleGuidsAuthorizedToRefreshCache.Add( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() );
            RoleGuidsAuthorizedToRefreshCache.Add( Rock.SystemGuid.Group.GROUP_WEB_ADMINISTRATORS.AsGuid() );

            try
            {
                AdultMaleMask = Image.Load<RgbaVector>( folderPath + "\\Masks\\adult-male.png" );
                AdultFemaleMask = Image.Load<RgbaVector>( folderPath + "\\Masks\\adult-female.png" );
                ChildMaleMask = Image.Load<RgbaVector>( folderPath + "\\Masks\\child-male.png" );
                ChildFemaleMask = Image.Load<RgbaVector>( folderPath + "\\Masks\\child-female.png" );
                UnknownGenderMask = Image.Load<RgbaVector>( folderPath + "\\Masks\\unknown-gender.png" );
                BusinessMask = Image.Load<RgbaVector>( folderPath + "\\Masks\\business.png" );

                // Load Fonts
                _fontCollection = new FontCollection();
                _fontCollection.Add( folderPath + "\\Fonts\\Inter-Regular.ttf" );
                _fontCollection.Add( folderPath + "\\Fonts\\Inter-ExtraBold.ttf" );
                _fontCollection.Add( folderPath + "\\Fonts\\Inter-Bold.ttf" );
                _fontCollection.Add( folderPath + "\\Fonts\\Inter-Thin.ttf" );
            }
            catch { }

            // Load cache of record types
            RecordTypeIdBusiness = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;
            RecordTypeIdNamelessPerson = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_NAMELESS.AsGuid() ).Id;
            RecordTypeIdPerson = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
        }


        /// <summary>
        /// Creates an icon mask with the provided color.
        /// </summary>
        /// <param name="mask">The mask.</param>
        /// <param name="color">The color.</param>
        /// <returns>Image.</returns>
        

        /// <summary>
        /// Creates the avatar based on the provided settings
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public static Stream CreateAvatar( AvatarSettings settings )
        {
            Image avatar = null;

            // If there is a photo in the settings we will always use that
            if ( settings.PhotoId.HasValue )
            {
                // Get image from the binary file
                avatar = RockImage.GetImageFromBinaryFileService( settings.PhotoId.Value );

                // Resize image
                avatar.CropResize( settings.Size, settings.Size );
            }

            // If not photo was provided then we'll create a fallback
            if ( avatar == null )
            {
                switch ( settings.AvatarStyle )
                {
                    case AvatarStyle.Icon:
                        {
                            avatar = CreateIconAvatar( settings );
                            break;
                        }
                    case AvatarStyle.Initials:
                        {
                            avatar = CreateInitialsAvatar( settings );
                            break;
                        }
                }
            }

            // We should always have a image, but just in case
            if ( avatar == null )
            {
                return null;
            }

            // Apply the requested styling
            if ( settings.CornerRadius != 0 )
            {
                avatar.Mutate( o => o.ApplyRoundedCorners( settings.CornerRadius ) );
            }

            if ( settings.IsCircle )
            {
                avatar.Mutate( o => o.ApplyCircleCorners() );
            }

            // Cache the image to the file system
            //avatar.SaveAsPng( $"{settings.CachePath}{settings.CacheKey}.png" );  TODO: REMOVE BEFORE FLIGHT

            var outputStream = new MemoryStream();
            avatar.SaveAsPng( outputStream );

            outputStream.Position = 0;

            return outputStream;
        }

        private static Image CreateIconAvatar( AvatarSettings settings )
        {
            // Get the top layer which is the icon
            var topLayer = CreateIconAvatarTopLayer( settings );

            // Get the background layer which is solid
            var bottomLayer = RockImage.CreateSolidImage( settings.Size, settings.Size, settings.AvatarColors.BackgroundColor );

            // Return the two images merged
            bottomLayer.Mutate( i => i
                        .DrawImage( topLayer, new Point( 0, 0 ), 1f )
                        .DrawImage( bottomLayer, new Point( 0, 0 ), 1f ) );

            return bottomLayer;
        }

        /// <summary>
        /// Gets the top layer for the icon avatar
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private static Image CreateIconAvatarTopLayer( AvatarSettings settings )
        {
            // Get the correct mask to use based off the settings
            var mask = GetIconMask( settings );

            // Resize the mask
            mask.Mutate( o => o.Resize( settings.Size, settings.Size, KnownResamplers.Lanczos3 ) );

            var topLayer = RockImage.CreateMaskImage( mask, settings.AvatarColors.ForegroundColor );

            return topLayer;
        }

        /// <summary>
        /// Gets the correct icon mask based on the settings.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private static Image GetIconMask( AvatarSettings settings )
        {
            // Business
            if ( settings.RecordTypeId == AvatarHelper.RecordTypeIdBusiness )
            {
                return AvatarHelper.BusinessMask.CloneAs<Rgba32>();
            }

            // Nameless person
            if ( settings.RecordTypeId == AvatarHelper.RecordTypeIdNamelessPerson )
            {
                return AvatarHelper.UnknownGenderMask.CloneAs<Rgba32>();
            }

            // Child male
            if ( settings.Gender == Gender.Male && settings.AgeClassification == AgeClassification.Child )
            {
                return AvatarHelper.ChildMaleMask.CloneAs<Rgba32>();
            }

            // Child female
            if ( settings.Gender == Gender.Female && settings.AgeClassification == AgeClassification.Child )
            {
                return AvatarHelper.ChildFemaleMask.CloneAs<Rgba32>();
            }

            // Male
            if ( settings.Gender == Gender.Male )
            {
                return AvatarHelper.AdultMaleMask.CloneAs<Rgba32>();
            }

            // Female
            if ( settings.Gender == Gender.Female )
            {
                return AvatarHelper.AdultFemaleMask.CloneAs<Rgba32>();
            }

            // Default is unknown
            return AvatarHelper.UnknownGenderMask.CloneAs<Rgba32>();
        }

        private static Image CreateInitialsAvatar( AvatarSettings settings )
        {
            // Create the background
            var backgroundImage = new Image<Rgba32>( settings.Size, settings.Size, Color.ParseHex( settings.AvatarColors.BackgroundColor ) );

            // Calculate spacing
            var fontSize = ( settings.Size / 3 ); // font size is 1/3 of the height
            
            // Get font
            var fontWeight = FontStyle.Regular;

            if ( settings.IsBold )
            {
                fontWeight = FontStyle.Bold;
            }

            var fontFamily = _fontCollection.Get( "Inter" );
            var font = fontFamily.CreateFont( ( float ) fontSize, fontWeight );

            // Write text
            // https://github.com/SixLabors/ImageSharp/issues/185
            // https://www.adamrussell.com/adding-image-watermark-text-in-c-with-imagesh
            var textOptions = new TextOptions( font )
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                WrappingLength = settings.Size,
                Dpi = (float) backgroundImage.Metadata.VerticalResolution
            };

            // Measure the size of the text
            var textSize = TextMeasurer.Measure( settings.Text, textOptions );

            // Calculate margins
            var leftMargin = (settings.Size - textSize.Width ) / 2;
            var topMargin = ( settings.Size - textSize.Height ) / 2;

            textOptions.Origin = new System.Numerics.Vector2(topMargin, leftMargin);

            backgroundImage.Mutate( o => o.DrawText( textOptions, settings.Text, Color.ParseHex( settings.AvatarColors.ForegroundColor) ) );
            //backgroundImage.Mutate( o => o.DrawText( settings.Text, font, Color.ParseHex( settings.AvatarColors.ForegroundColor ), new PointF( topMargin, leftMargin ) ) );

            return backgroundImage;
        }

        
    }

    public static class RockImage
    {
        /// <summary>
        /// Creates a image from a mask using the fill color provided.
        /// </summary>
        /// <param name="mask"></param>
        /// <param name="fillColor"></param>
        /// <returns></returns>
        public static Image CreateMaskImage( Image mask, string fillColor )
        {
            // Logic for this comes from: https://stackoverflow.com/questions/52875516/how-to-compose-two-images-using-source-in-composition

            // Create the "background" which is the solid color that we want our mask  to cut out
            var maskColor = RgbaVector.FromHex( fillColor );
            var background = new Image<RgbaVector>( mask.Width, mask.Height, maskColor );

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

        public static Image CreateSolidImage( int width, int height, string color )
        {
            return new Image<Rgba32>( width, height, Color.ParseHex( color)  );
        }

        /// <summary>
        /// Returns a Image Sharp image from a binary file 
        /// </summary>
        /// <param name="photoId"></param>
        /// <returns></returns>
        public static Image GetImageFromBinaryFileService( int photoId )
        {
            var binaryFileData = new BinaryFileDataService( new RockContext() ).Get( photoId );

            // If the file was not found return a blank image
            if ( binaryFileData == null )
            {
                return new Image<Rgba32>( 1, 1 );
            }

            // Return the binary file's content as a new image
            try
            {
                return Image.Load<Rgba32>( binaryFileData.Content );
            }
            catch { }

            // There was a problem with the content in the binary file so return a blank image
            return new Image<Rgba32>( 1, 1 );
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
        /// Gets or sets the age classification
        /// </summary>
        public AgeClassification AgeClassification { get; set; }

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
                _text = value.Truncate( 2, false );
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
        public int? RecordTypeId { get; set; }

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

        /// <summary>
        /// The physical path to store the cached avatars
        /// </summary>
        public string CachePath { get; set; }

        /// <summary>
        /// Creates a cache key based on the settings
        /// </summary>
        public string CacheKey
        {
            // Written as is to ensure the key is calculated only once.
            // Note we can't include the person id or guid in the cache key has their profile picture could change and the cache key is
            // used to cache the image both on the server (which could be clustered) and the client.
            get
            {
                if ( _cacheKey.IsNullOrWhiteSpace() )
                {
                    _cacheKey = $"{Size}-{Text}-{AvatarColors.ForegroundColor.Replace("#", "")}_{AvatarColors.BackgroundColor.Replace( "#", "" )}-{CornerRadius}-{AvatarStyle}-{AgeClassification}-{Gender}-{IsBold.ToString().Truncate( 1, false )}-{PhotoId}-{PrefersLight.ToString().Truncate( 1, false )}-{RecordTypeId}";
                }

                return _cacheKey;
            }
        }
        private string _cacheKey = string.Empty;
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
        /// Reusable random generator (ensures colors are truely random). 
        /// </summary>
        private static Random _random= new Random();

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
                var randomIndex = _random.Next( 0, colorValues.Length );
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
            "#ef4444",
            "#f97316",
            "#71717a",
            "#f59e0b",
            "#eab308",
            "#84cc16",
            "#22c55e",
            "#10b981",
            "#14b8a6",
            "#06b6d4",
            "#0ea5e9",
            "#3b82f6",
            "#6366f1",
            "#8b5cf6",
            "#a855f7",
            "#d946ef",
            "#ec4899",
            "#f43f5e",
            "#22c55e"
        };
    }

    public static partial class ExtensionMethods
    {
        /// <summary>
        /// Applies rounded corners to an image.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="cornerRadius"></param>
        /// <returns></returns>
        public static IImageProcessingContext ApplyRoundedCorners( this IImageProcessingContext ctx, float cornerRadius )
        {
            // Source: https://github.com/SixLabors/Samples/blob/main/ImageSharp/AvatarWithRoundedCorner/Program.cs

            // First create a square
            var rect = new RectangularPolygon( -0.5f, -0.5f, cornerRadius, cornerRadius );

            // Then cut out of the square a circle so we are left with a corner
            IPath cornerTopLeft = rect.Clip( new EllipsePolygon( cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius ) );

            var size = ctx.GetCurrentSize();

            float rightPos = size.Width - cornerTopLeft.Bounds.Width + 1;
            float bottomPos = size.Height - cornerTopLeft.Bounds.Height + 1;

            // Move it across the width of the image - the width of the shape
            IPath cornerTopRight = cornerTopLeft.RotateDegree( 90 ).Translate( rightPos, 0 );
            IPath cornerBottomLeft = cornerTopLeft.RotateDegree( -90 ).Translate( 0, bottomPos );
            IPath cornerBottomRight = cornerTopLeft.RotateDegree( 180 ).Translate( rightPos, bottomPos );

            var corners = new PathCollection( cornerTopLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight );

            ctx.SetGraphicsOptions( new GraphicsOptions()
            {
                Antialias = true,
                AlphaCompositionMode = PixelAlphaCompositionMode.DestOut // Enforces that any part of this shape that has color is punched out of the background
            } );

            // mutating in here as we already have a cloned original
            // use any color (not Transparent), so the corners will be clipped
            foreach ( var c in corners )
            {
                ctx = ctx.Fill( Color.Red, c );
            }
            return ctx;
        }

        /// <summary>
        /// Gives the corners a circle effect in a way where the image looks round on the edges (a full circle if the image is square).
        /// </summary>
        /// <param name="ctx"></param>
        /// <returns></returns>
        public static IImageProcessingContext ApplyCircleCorners( this IImageProcessingContext ctx )
        {
            var size = ctx.GetCurrentSize();

            // 1/2 of the height + 2 px (to make up for the internal logic of rounded corners)
            ctx.ApplyRoundedCorners( ( size.Height / 2 ) + 2 );

            return ctx;
        }

        /// <summary>
        /// Resizes the source image. Crops the resized image to fit the bounds of its container.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Image CropResize( this Image source, int width, int height )
        {
            var resizeOptions = new ResizeOptions() { Mode = ResizeMode.Crop, Size = new SixLabors.ImageSharp.Size( width, height ), Sampler = KnownResamplers.Lanczos3 };
            source.Mutate( i => i.Resize( resizeOptions ) );

            return source;
        }
    }
}