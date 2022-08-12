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
import Alert from "@Obsidian/Controls/alert";
import LoadingIndicator from "@Obsidian/Controls/loadingIndicator";
import { computed, defineComponent, PropType, ref, watch } from "vue";
import GridActionGroup from "./gridActionGroup.partial";
import GridColumnHeaderRow from "./gridColumnHeaderRow.partial";
import GridDataRows from "./gridDataRows.partial";
import GridFilterHeaderRow from "./gridFilterHeaderRow.partial";
import GridPagerRow from "./gridPagerRow.partial";
import { GridAction, GridColumnDefinition, GridData } from "./types";

export default defineComponent({
    name: "Grid",

    components: {
        Alert,
        GridActionGroup,
        GridColumnHeaderRow,
        GridDataRows,
        GridFilterHeaderRow,
        GridPagerRow,
        LoadingIndicator
    },

    props: {
        data: {
            type: [Object,Function] as PropType<GridData | (() => GridData | Promise<GridData>)>,
            default: []
        }
    },

    setup(props) {
        const gridActions = ref<GridAction[]>([]);
        const currentPage = ref(1);
        const pageSize = ref(10);
        const pageSizes = [10, 50, 250];
        const quickFilterValue = ref("");
        const loadingData = ref(false);
        const gridErrorMessage = ref("");
        const gridData = ref<GridData | null>(null);
        const columnFilterValues = ref<Record<string, unknown | undefined>>({});

        const columnDefinitions = computed((): GridColumnDefinition[] => {
            return gridData.value?.columns ?? [];
        });

        const visibleColumnDefinitions = computed((): GridColumnDefinition[] => {
            return columnDefinitions.value;
        });

        const visibleColumnCount = computed((): number => {
            return visibleColumnDefinitions.value.length;
        });

        const rows = computed((): Record<string, unknown>[] => {
            return gridData.value?.rows ?? [];
        });

        const filteredRows = computed((): Record<string, unknown>[] => {
            const filterValue = quickFilterValue.value.toLowerCase();

            return rows.value.filter(v => {
                const quickFilterMatch = !filterValue || visibleColumnDefinitions.value.some(column => {
                    const value = v[column.name];

                    if (column.quickFilter && column.quickFilter(filterValue, value)) {
                        return true;
                    }

                    let textValue: string;

                    if (column.format) {
                        textValue = column.format(value);
                    }
                    else if (typeof value === "string") {
                        textValue = value;
                    }
                    else {
                        textValue = new String(value).toString();
                    }

                    if (textValue.toLowerCase().includes(filterValue)) {
                        return true;
                    }
                });

                const filtersMatch = visibleColumnDefinitions.value.every(column => {
                    if (!column.filter) {
                        return true;
                    }

                    const columnFilterValue = columnFilterValues.value[column.name];

                    if (columnFilterValue === undefined) {
                        return true;
                    }

                    return column.filter(columnFilterValue, v[column.name]);
                });

                return quickFilterMatch && filtersMatch;
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

        const updateGridData = async (): Promise<void> => {
            loadingData.value = true;

            if (typeof props.data === "object") {
                gridData.value = props.data;
            }
            else if (typeof props.data === "function") {
                try {
                    gridData.value = await props.data();
                }
                catch (error) {
                    gridErrorMessage.value = error instanceof Error ? error.message : new String(error).toString();
                }
            }

            loadingData.value = false;
        };
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

        updateGridData();

        return {
            currentPage,
            filteredRows,
            columnFilterValues,
            gridActions,
            gridErrorMessage,
            loadingData,
            pageCount,
            pageSize,
            pageSizes,
            pagerMessage,
            quickFilterValue,
            visibleColumnCount,
            visibleColumnDefinitions,
            visibleRows
        };
    },

    template: `
<v-style>
    table.table-obsidian th.grid-column-header {
        position: relative;
    }
        table.table-obsidian th.grid-column-header .btn-grid-column-filter {
            border: 0px;
            background-color: transparent;
            margin-left: 4px;
            color: #e7e7e7;
            opacity: 0.5;
            transition-duration: 250ms;
            transition-property: opacity;
        }

            table.table-obsidian th.grid-column-header .btn-grid-column-filter.active {
                color: var(--brand-success);
            }

        table.table-obsidian th.grid-column-header:hover .btn-grid-column-filter {
            opacity: 1;
        }

        table.table-obsidian th.grid-column-header .grid-filter-popup {
            position: absolute;
            padding: 12px;
            margin-top: 6px;
            min-width: 120px;
            max-width: 320px;
            background-color: white;
            border: 1px solid #c7c7c7;
            border-radius: 0px;
            border-bottom-right-radius: 6px;
            border-bottom-left-radius: 6px;
            box-shadow: 2px 2px 4px rgba(0,0,0,0.2);
            font-weight: initial;
        }

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

    table.table-obsidian th.grid-filter {
        padding: 8px;
    }

        table.table-obsidian th.grid-filter .grid-quick-filter {
            flex: 1 0;
            max-width: 480px;
        }

        table.table-obsidian th.grid-filter .grid-actions {
            flex: 1 0;
            display: flex;
            justify-content: end;
            background-color: initial;
        }

    table.table-obsidian th.grid-error {
        padding: 0px;
    }
        table.table-obsidian th.grid-error > .alert {
            margin-bottom: 0px;
        }
</v-style>

<table class="grid-table table table-bordered table-striped table-hover table-obsidian">
    <thead>
        <GridFilterHeaderRow v-model:filterValue="quickFilterValue"
            :gridActions="gridActions"
            :visibleColumnCount="visibleColumnCount" />

        <GridColumnHeaderRow :columns="visibleColumnDefinitions" v-model:columnFilters="columnFilterValues" />
    </thead>

    <tbody>
        <tr v-if="loadingData">
            <td class="grid-error" :colspan="visibleColumnCount">
                <LoadingIndicator />
            </td>
        </tr>

        <tr v-else-if="gridErrorMessage">
            <td class="grid-error" :colspan="visibleColumnCount">
                <Alert alertType="warning">{{ gridErrorMessage }}</Alert>
            </td>
        </tr>

        <GridDataRows v-else
            :columns="visibleColumnDefinitions"
            :rows="visibleRows" />
    </tbody>

    <tfoot>
        <GridPagerRow v-model:pageSize="pageSize"
            v-model:currentPage="currentPage"
            :pageCount="pageCount"
            :pageSizes="pageSizes"
            :message="pagerMessage"
            :visibleColumnCount="visibleColumnCount"
            :gridActions="gridActions" />
    </tfoot>
</table>
`
});
