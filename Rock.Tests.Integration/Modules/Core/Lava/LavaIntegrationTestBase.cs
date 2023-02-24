﻿// <copyright>
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
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Lava;
using Rock.Tests.Shared;

namespace Rock.Tests.Integration.Core.Lava
{
    [TestClass]
    public class LavaIntegrationTestBase
    {
        public static LavaIntegrationTestHelper TestHelper
        {
            get
            {
                return LavaIntegrationTestHelper.CurrentInstance;
            }
        }

        [ClassInitialize]
        public static void ClassInitialize( TestContext context )
        {
            LogHelper.SetTestContext( context );
        }
    }
}
