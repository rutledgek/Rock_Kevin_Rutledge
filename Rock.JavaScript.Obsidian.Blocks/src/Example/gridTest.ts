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

import { Guid } from "@Obsidian/Types";
import { useConfigurationValues, useInvokeBlockAction } from "@Obsidian/Utility/block";
import { escapeHtml } from "@Obsidian/Utility/stringUtils";
import { defineComponent } from "vue";
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
                            title: "Name"
                        },
                        {
                            name: "description"
                        },
                        {
                            name: "attr_Group1",
                            title: "Group 1",
                            format: (value: unknown) => {
                                if (typeof value === "object") {
                                    const linkValue = value as { guid?: Guid | null, text?: string | null };

                                    if (!linkValue.guid || !linkValue.text) {
                                        return "";
                                    }

                                    return `<a href="/Group/${linkValue.guid}">${escapeHtml(linkValue.text)}</a>`;
                                }

                                return "";
                            },
                            quickFilter: (needle, haystack) => {
                                if (typeof haystack === "object") {
                                    const linkValue = haystack as { guid: string, text: string };
                                    return linkValue.text.toLowerCase().includes(needle);
                                }

                                return false;
                            },
                            filter: (needle, haystack) => {
                                if (typeof needle === "string" && typeof haystack === "object") {
                                    const linkValue = haystack as { guid: string, text: string };
                                    return linkValue.text.toLowerCase().includes(needle.toLowerCase());
                                }

                                return false;
                            },
                            sort: (a, b) => {
                                if (typeof a === "object" && typeof b === "object") {
                                    const linkA = a as { guid: string, text: string };
                                    const linkB = b as { guid: string, text: string };

                                    if (linkA.text < linkB.text) {
                                        return -1;
                                    }
                                    else if (linkA.text > linkB.text) {
                                        return 1;
                                    }
                                }

                                return 0;
                            }
                        },
                        {
                            name: "attr_CheckList",
                            title: "Check List"
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
