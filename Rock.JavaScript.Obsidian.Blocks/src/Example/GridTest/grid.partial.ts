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

import { asFormattedString } from "@Obsidian/Utility/numberUtils";
import { pluralConditional } from "@Obsidian/Utility/stringUtils";
import { computed, defineComponent, PropType, ref, watch } from "vue";
import GridActionGroup from "./gridActionGroup.partial";
import GridColumnHeaderRow from "./gridColumnHeaderRow.partial";
import GridData from "./gridData.partial";
import GridFilterHeaderRow from "./gridFilterHeaderRow.partial";
import GridPagerRow from "./gridPagerRow.partial";
import { GridAction } from "./types";

export default defineComponent({
    name: "Grid",

    components: {
        GridActionGroup,
        GridColumnHeaderRow,
        GridData,
        GridFilterHeaderRow,
        GridPagerRow
    },

    props: {
        rows: {
            type: [Array,Function] as PropType<Record<string, unknown>[] | (() => Record<string, unknown>[] | Promise<Record<string, unknown>[]>)>,
            default: []
        }
    },

    setup(props) {
        const visibleColumnCount = ref(2);
        const gridActions = ref<GridAction[]>([]);
        const rows = ref<Record<string, unknown>[]>([]);
        const currentPage = ref(1);
        const pageSize = ref(10);
        const pageSizes = [10, 50, 250];
        const quickFilterValue = ref("");

        if (Array.isArray(props.rows)) {
            rows.value = props.rows;
        }

        const filteredRows = computed((): Record<string, unknown>[] => {
            if (!quickFilterValue) {
                return rows.value;
            }

            return rows.value.filter(v => {
                for (const key of Object.keys(v)) {
                    const keyValue = v[key];

                    if (typeof keyValue === "string") {
                        if (keyValue.toLowerCase().includes(quickFilterValue.value.toLowerCase())) {
                            return true;
                        }
                    }
                }

                return false;
            });
        });

        const visibleRows = computed((): Record<string, unknown>[] => {
            const startIndex = (currentPage.value - 1) * pageSize.value;

            return filteredRows.value.slice(startIndex, startIndex + pageSize.value);
        });

        const pageCount = computed((): number => {
            return Math.ceil(filteredRows.value.length / pageSize.value);
        });

        const pagerMessage = computed((): string => {
            return `${asFormattedString(filteredRows.value.length)} ${pluralConditional(rows.value.length, "Group", "Groups")}`;
        });

        const onActionClick = (): Promise<void> => {
            return new Promise(resolve => setTimeout(resolve, 2000));
        };

        watch(quickFilterValue, () => {
            currentPage.value = 1;
        });

        gridActions.value.push({
            executing: false,
            handler: onActionClick,
            iconCssClass: "fa fa-question",
            tooltip: "This gives you help."
        });

        gridActions.value.push({
            executing: false,
            handler: onActionClick,
            iconCssClass: "fa fa-plus",
            tooltip: "Add a new item."
        });

        return {
            currentPage,
            filteredRows,
            gridActions,
            pageCount,
            pageSize,
            pageSizes,
            pagerMessage,
            quickFilterValue,
            visibleColumnCount,
            visibleRows
        };
    },

    template: `
<v-style>
    table.table-obsidian td.grid-paging {
    }

    table.table-obsidian td.grid-paging .grid-page-sizes {
        flex: 1 0;
        justify-content: start;
        align-items: center;
    }

    table.table-obsidian td.grid-paging .grid-page-picker {
        flex: 1 0;
        display: flex;
        justify-content: center;
        align-items: center;
    }

    table.table-obsidian td.grid-paging .grid-page-picker > button {
        border: 0px;
        background-color: transparent;
        margin: 0px 5px;
        padding: 0px 6px;
    }

    table.table-obsidian td.grid-paging .grid-page-picker > button.disabled {
        opacity: 0.5;
        pointer-events: none;
    }

    table.table-obsidian td.grid-paging .grid-page-picker > button.active {
        border: 1px solid #b7b7b7;
        background-color: #f7f7f7;
    }

    table.table-obsidian td.grid-paging .grid-actions {
        flex: 1 0;
        display: flex;
        justify-content: end;
        align-items: center;
    }

    table.table-obsidian td.grid-filter {
        padding: 8px;
    }

    table.table-obsidian td.grid-filter .grid-quick-filter {
        flex: 1 0;
        max-width: 480px;
    }

    table.table-obsidian td.grid-filter .grid-actions {
        flex: 1 0;
        display: flex;
        justify-content: end;
        background-color: initial;
    }
</v-style>

<table class="grid-table table table-bordered table-striped table-hover table-obsidian">
    <thead>
        <GridFilterHeaderRow v-model:filterValue="quickFilterValue" :gridActions="gridActions" :visibleColumnCount="visibleColumnCount" />

        <GridColumnHeaderRow />
    </thead>

    <tbody>
        <GridData :rows="visibleRows" />
    </tbody>

    <tfoot>
        <GridPagerRow v-model:pageSize="pageSize" v-model:currentPage="currentPage" :pageCount="pageCount" :pageSizes="pageSizes" :message="pagerMessage" :visibleColumnCount="visibleColumnCount" :gridActions="gridActions" />
    </tfoot>
</table>
`
});
