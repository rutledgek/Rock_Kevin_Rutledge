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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web.Http;
using System.Web.Http.Controllers;

using Rock.Data;
using Rock.Logging;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.RestControllerService"/> entity objects.
    /// </summary>
    public partial class RestControllerService
    {
        private class DiscoveredControllerFromReflection
        {
            public string Name { get; set; }
            public string ClassName { get; set; }
            public Guid? ReflectedGuid { get; set; }
            public List<DiscoveredRestAction> DiscoveredRestActions { get; set; } = new List<DiscoveredRestAction>();
            public override string ToString()
            {
                return ClassName;
            }
        }

        private class DiscoveredRestAction
        {
            public string ApiId { get; set; }
            public string Method { get; set; }
            public string Path { get; set; }
            public Guid? ReflectedGuid { get; set; }
            public override string ToString()
            {
                return ApiId;
            }
        }

        /// <summary>
        /// Registers the controllers.
        /// </summary>
        public static void RegisterControllers()
        {
            /*
             * 05/13/2022 MDP/DMV
             * 
             * In addition to the 12/19/2019 BJW note, we also added a RockGuid attribute to 
             * controllers and methods (except for inherited methods). This will prevent
             * loosing security on methods that have changed their signature. 
             * 
             * 
             * 12/19/2019 BJW
             *
             * There was an issue with the SecuredAttribute not calculating API ID the same as was being calculated here.
             * This caused the secured attribute to sometimes not find the RestAction record and thus not find the
             * appropriate permissions (Auth table). The new method "GetApiId" is used in both places as a standardized
             * API ID generator to ensure that this does not occur. The following code has also been modified to gracefully
             * update any old style API IDs and update them to the new format without losing any foreign key associations, such
             * as permissions.
             *
             * See task for detailed background: https://app.asana.com/0/474497188512037/1150703513867003/f
             */

            WriteLog( "Initializing..." );

            // Controller Class Name => New Format Id => Old Format Id
            var controllerApiIdMap = new Dictionary<string, Dictionary<string, string>>();

            var rockContext = new RockContext();
            var restControllerService = new RestControllerService( rockContext );
            var discoveredControllers = new List<DiscoveredControllerFromReflection>();

            var config = GlobalConfiguration.Configuration;

            // For Debug Only: Check for a caching error.
            var assemblies1 = Reflection.GetPluginAssemblies( refreshCache: false )
                .Select(a => a.GetName().Name );
            var assemblies2 = Reflection.GetPluginAssemblies( refreshCache: true )
                .Select( a => a.GetName().Name );

            var addedAssemblies = assemblies2.Except( assemblies1 ).ToList();
            if ( addedAssemblies.Any() )
            {
                WriteLog( $"WARNING - Forced cache refresh found {addedAssemblies.Count} additional assemblies. [Assemblies={addedAssemblies.AsDelimited( ", " )}]", RockLogLevel.Debug );
            }

            // Identify the target assemblies.
            var resolver = config.Services.GetAssembliesResolver();
            var targetAssemblyNames = resolver.GetAssemblies()
                .Select( a => a.GetName().Name )
                .ToList();

            WriteLog( $"Discovered {targetAssemblyNames.Count} target assemblies. [Resolver={resolver.GetType().Name}, Assemblies={targetAssemblyNames.AsDelimited(", ")}]", RockLogLevel.Debug );

            // Extract API methods from target assemblies.
            var explorer = config.Services.GetApiExplorer();
            WriteLog( $"Discovered {explorer.ApiDescriptions.Count} API methods in target assemblies." );

            if ( !explorer.ApiDescriptions.Any() )
            {
                // Just in case ApiDescriptions wasn't populated, exit and don't do anything
                return;
            }

            foreach ( var apiDescription in explorer.ApiDescriptions )
            {
                var reflectedHttpActionDescriptor = ( ReflectedHttpActionDescriptor ) apiDescription.ActionDescriptor;
                var action = apiDescription.ActionDescriptor;
                var name = action.ControllerDescriptor.ControllerName;
                var method = apiDescription.HttpMethod.Method;

                var controller = discoveredControllers.Where( c => c.Name == name ).FirstOrDefault();
                if ( controller == null )
                {
                    var controllerRockGuid = action.ControllerDescriptor.ControllerType.GetCustomAttribute<Rock.SystemGuid.RestControllerGuidAttribute>( inherit: false )?.Guid;

                    controller = new DiscoveredControllerFromReflection
                    {
                        Name = name,
                        ClassName = action.ControllerDescriptor.ControllerType.FullName
                    };

                    if ( controllerRockGuid.HasValue )
                    {
                        controller.ReflectedGuid = controllerRockGuid.Value;
                    }
                    else
                    {
                        controller.ReflectedGuid = null;
                    }

                    discoveredControllers.Add( controller );
                    controllerApiIdMap[controller.ClassName] = new Dictionary<string, string>();
                }

                var apiIdMap = controllerApiIdMap[controller.ClassName];
                var apiId = GetApiId( reflectedHttpActionDescriptor.MethodInfo, method, controller.Name, out Guid? restActionGuid );

                // Because we changed the format of the stored ApiId, it is possible some RestAction records will have the old
                // style Id, which is apiDescription.ID
                apiIdMap[apiId] = apiDescription.ID;

                var restAction = new DiscoveredRestAction
                {
                    ApiId = apiId,
                    Method = method,
                    Path = apiDescription.RelativePath
                };

                if ( restActionGuid.HasValue )
                {
                    restAction.ReflectedGuid = restActionGuid.Value;
                }
                else
                {
                    restAction.ReflectedGuid = null;
                }

                controller.DiscoveredRestActions.Add( restAction );
            }

            WriteLog( $"Discovered {discoveredControllers.Count()} controllers in target assemblies." );

            if ( !discoveredControllers.Any() )
            {
                // Just in case discoveredControllers somehow is empty, exit and don't do anything
                return;
            }

            var discoveredControllerNames = discoveredControllers.Select( d => d.Name ).OrderBy( n => n ).ToList();

            WriteLog( $"Identified controllers to be processed. [Controllers={discoveredControllerNames.AsDelimited( ", " ) }]", RockLogLevel.Debug );

            bool addController;
            bool updateController;

            int controllersAdded = 0;
            int controllersRemoved = 0;
            int controllersUpdated = 0;

            var actionService = new RestActionService( rockContext );
            foreach ( var discoveredController in discoveredControllers )
            {
                addController = false;
                updateController = false;

                var apiIdMap = controllerApiIdMap[discoveredController.ClassName];

                var controller = restControllerService.Queryable( "Actions" )
                    .Where( c => c.Name == discoveredController.Name ).FirstOrDefault();
                if ( controller == null )
                {
                    controller = new RestController
                    {
                        Name = discoveredController.Name,
                    };

                    restControllerService.Add( controller );
                    addController = true;
                }

                controller.ClassName = discoveredController.ClassName;

                if ( discoveredController.ReflectedGuid.HasValue )
                {
                    if ( controller.Guid != discoveredController.ReflectedGuid.Value )
                    {
                        controller.Guid = discoveredController.ReflectedGuid.Value;
                        updateController = true;
                    }
                }

                foreach ( var discoveredAction in discoveredController.DiscoveredRestActions )
                {
                    var newFormatId = discoveredAction.ApiId;
                    var oldFormatId = apiIdMap[newFormatId];

                    var action = controller.Actions.Where( a =>
                        a.ApiId == newFormatId
                        || a.ApiId == oldFormatId
                        || ( discoveredAction.ReflectedGuid.HasValue && a.Guid == discoveredAction.ReflectedGuid.Value ) ).FirstOrDefault();

                    if ( action == null )
                    {
                        action = new RestAction { ApiId = newFormatId };
                        controller.Actions.Add( action );
                        updateController = true;
                    }

                    action.Method = discoveredAction.Method;
                    action.Path = discoveredAction.Path;

                    if ( action.ApiId != newFormatId )
                    {
                        // Update the ID to the new format
                        // This will also take care of method signature changes
                        WriteLog( $"Updated Action \"{action.ApiId}\" ApiId. [NewValue={newFormatId}]", RockLogLevel.Debug );

                        action.ApiId = newFormatId;
                        updateController = true;
                    }

                    if ( discoveredAction.ReflectedGuid.HasValue )
                    {
                        if ( action.Guid != discoveredAction.ReflectedGuid.Value )
                        {
                            WriteLog ( $"Updated Action \"{action.ApiId}\" Guid. [NewValue={discoveredAction.ReflectedGuid.Value}]", RockLogLevel.Debug );

                            action.Guid = discoveredAction.ReflectedGuid.Value;
                            updateController = true;
                        }
                    }
                }

                var actions = discoveredController.DiscoveredRestActions.Select( d => d.ApiId ).ToList();
                foreach ( var action in controller.Actions.Where( a => !actions.Contains( a.ApiId ) ).ToList() )
                {
                    WriteLog( $"Removed Action \"{action.ApiId}\".", RockLogLevel.Debug );

                    actionService.Delete( action );
                    controller.Actions.Remove( action );
                    updateController = true;
                }

                var registerAction = string.Empty;
                if ( addController )
                {
                    registerAction = "Added";
                    controllersAdded++;
                }
                else if (updateController)
                {
                    registerAction = "Updated";
                    controllersUpdated++;
                }
                if ( !string.IsNullOrEmpty( registerAction ) )
                {
                    WriteLog( $"{registerAction} controller \"{controller.Name}\" ({actions.Count} actions)", RockLogLevel.Debug );
                }
            }

            // Remove Controllers that are no longer discoverable.
            var undiscoveredControllers = restControllerService.Queryable().Where( c => !discoveredControllerNames.Contains( c.Name ) ).ToList();
            foreach ( var controller in undiscoveredControllers )
            {
                restControllerService.Delete( controller );
                WriteLog( $"Removed controller \"{controller.Name}\".", RockLogLevel.Debug );
                controllersRemoved++;
            }

            try
            {
                rockContext.SaveChanges();

                WriteLog( $"Registration complete. ({controllersAdded} added, {controllersUpdated} updated, {controllersRemoved} removed)" );
            }
            catch ( Exception thrownException )
            {
                WriteLog( $"Registration failed. Check the Exception Log for additional information.", RockLogLevel.Error );

                // if the exception was due to a duplicate Guid, throw as a duplicateGuidException. That'll make it easier to troubleshoot.
                var duplicateGuidException = Rock.SystemGuid.DuplicateSystemGuidException.CatchDuplicateSystemGuidException( thrownException, null );
                if ( duplicateGuidException != null )
                {
                    throw duplicateGuidException;
                }
                else
                {
                    throw;
                }
            }
        }

        private static void WriteLog( string message, RockLogLevel logLevel = RockLogLevel.Info, [CallerMemberName] string processName = "" )
        {
            RockLogger.Log.WriteToLog( logLevel, RockLogDomains.Core, $"{nameof( RestControllerService )} ({processName}): {message}" );
        }
    }
}
