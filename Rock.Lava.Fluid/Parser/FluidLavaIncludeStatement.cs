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
using System.IO;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid;
using Fluid.Ast;

namespace Rock.Lava.Fluid
{
    public class FluidLavaIncludeStatement : IncludeStatement
    {
        #region Constructors

        private static FieldInfo _cachedTemplateFieldInfo;

        public FluidLavaIncludeStatement( FluidParser parser, Expression path, Expression with = null, Expression @for = null, string alias = null, IList<AssignStatement> assignStatements = null )
            : base( parser, path, with, @for, alias, assignStatements )
        {
        }

        static FluidLavaIncludeStatement()
        {
            // Get a reference to the internal field that is used to store the cached template.
            // This field exists in Fluid v2.3.1, but it may be removed or renamed in subsequent releases of the Fluid library.
            // If that happens, an exception will be thrown.
            _cachedTemplateFieldInfo = typeof( IncludeStatement ).GetField( "_cachedTemplate",
                BindingFlags.NonPublic | BindingFlags.Instance );

            if ( _cachedTemplateFieldInfo == null )
            {
                throw new LavaException( "Lava Include Tag implementation is not compatible with the current version of the Fluid library." );
            }
        }

        #endregion

        public override ValueTask<Completion> WriteToAsync( TextWriter writer, TextEncoder encoder, TemplateContext context )
        {
            // Reset the Fluid internal template cache variable to ensure that the include file content is reloaded.
            // This allows updates to the included file to be displayed immediately.
            _cachedTemplateFieldInfo.SetValue( this, null );

            return base.WriteToAsync( writer, encoder, context );
        }
    }
}