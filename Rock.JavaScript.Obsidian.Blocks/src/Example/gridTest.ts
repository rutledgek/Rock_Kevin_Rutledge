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

import textBox from "@Obsidian/Controls/textBox";
import { Guid } from "@Obsidian/Types";
import { useConfigurationValues, useInvokeBlockAction } from "@Obsidian/Utility/block";
import { escapeHtml } from "@Obsidian/Utility/stringUtils";
import { defineComponent } from "vue";
import { RockDateTime } from "@Obsidian/Utility/rockDateTime";
import Grid from "./GridTest/grid.partial";
import { GridData } from "./GridTest/types";

export default defineComponent({
    name: "Example.GridTest",

    components: {
        Grid
    },

    setup() {
        const configuration = useConfigurationValues();
        const invokeBlockAction = useInvokeBlockAction();

        const loadGridData = async (): Promise<GridData> => {
            const result = await invokeBlockAction<GridData>("GetGridData");

            if (result.isSuccess && result.data) {
                console.log(result.data.rows);
                return {
                    columns: [
                        {
                            name: "name",
                            title: "Name",
                            format: (value: unknown) => {
                                if (value && typeof value === "object") {
                                    return `${value["firstName"]} ${value["lastName"]}`;
                                }
                                else {
                                    return "";
                                }
                            },
                            sortValue: (value: unknown) => {
                                if (value && typeof value === "object") {
                                    return `${value["lastName"]} ${value["firstName"]}`;
                                }
                                else {
                                    return "";
                                }
                            }
                        },
                        {
                            name: "email",
                            title: "Email"
                        },
                        {
                            name: "enteredDateTime",
                            title: "Entered On",
                            format: (value: unknown) => {
                                if (!(typeof value === "string")) {
                                    return "";
                                }

                                const dt = RockDateTime.parseISO(value);

                                return dt?.toASPString("g") ?? "";
                            }
                        },
                        {
                            name: "expirationDateTime",
                            title: "Expires",
                            format: (value: unknown) => {
                                if (!(typeof value === "string")) {
                                    return "";
                                }

                                const dt = RockDateTime.parseISO(value);

                                return dt?.toASPString("g") ?? "";
                            }
                        },
                        {
                            name: "isUrgent",
                            title: "Urgent",
                            format: (value: unknown) => {
                                return value ? `<i class="fa fa-check"></i>` : "";
                            },
                            sortValue: (value: unknown) => {
                                return value ? 1 : 0;
                            }
                        },
                        {
                            name: "isPublic",
                            title: "Public",
                            format: (value: unknown) => {
                                return value ? `<i class="fa fa-check"></i>` : "";
                            },
                            sortValue: (value: unknown) => {
                                return value ? 1 : 0;
                            }
                        },
                        {
                            name: "attr_Person1",
                            title: "Person 1",
                            format: (value: unknown) => {
                                if (!(typeof value === "string")) {
                                    return "";
                                }

                                return value;
                            }
                        },
                        {
                            name: "attr_TextValue1",
                            title: "Text Value 1"
                        },
                        {
                            name: "attr_SingleSelect1",
                            title: "Single Select 1"
                        },
                        {
                            name: "attr_Group1",
                            title: "Group 1",
                            format: (value: unknown) => {
                                if (!(typeof value === "string")) {
                                    return "";
                                }

                                return value;
                            }
                        },
                        {
                            name: "attr_GroupTypes",
                            title: "Group Types"
                        }
                    ],
                    rows: result.data.rows
                };
            }
            else {
                throw new Error(result.errorMessage ?? "Unknown error while trying to load grid data.");
            }
        };

        return {
            gridData: loadGridData
        };
    },

    template: `
<Grid :data="gridData" />
`
});
