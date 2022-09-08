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
import Alert from "@Obsidian/Controls/alert.vue";
import LoadingIndicator from "@Obsidian/Controls/loadingIndicator";
import { computed, defineComponent, PropType, ref, toRaw, watch } from "vue";
import GridActionGroup from "./gridActionGroup.partial";
import GridColumnHeaderRow from "./gridColumnHeaderRow.partial";
import GridDataRows from "./gridDataRows.partial";
import GridFilterHeaderRow from "./gridFilterHeaderRow.partial";
import GridPagerRow from "./gridPagerRow.partial";
import { GridAction, GridColumnDefinition, GridData } from "./types";

/*
 * 8/17/2022 - DSH
 * 
 * The grid uses a number of non-ref instances with calculations via function call.
 * This is because the normal wrapped references that Vue uses dramatically slow
 * down our filtering and sorting processes. For example, filtering over wrapped
 * references of 100,000 rows takes around 600ms. The same 100,000 rows using an
 * unwrapped raw array takes about 40ms.
 */

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
            type: [Object, Function] as PropType<GridData | (() => GridData | Promise<GridData>)>,
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
        const columnFilterValues = ref<Record<string, unknown | undefined>>({});
        const columnSortDirection = ref<{ column: string, isDescending: boolean } | undefined>();
        const visibleRows = ref<Record<string, unknown>[]>([]);
        const pageCount = ref(0);
        const pagerMessage = ref("");
        const visibleColumnDefinitions = ref<GridColumnDefinition[]>([]);

        let gridData: GridData | null = null;
        let filteredRows: Record<string, unknown>[] = [];
        let sortedRows: Record<string, unknown>[] = [];

        const visibleColumnCount = computed((): number => {
            return visibleColumnDefinitions.value.length;
        });

        const updateFilteredRows = (): void => {
            const start = Date.now();
            if (gridData) {
                const columns = toRaw(visibleColumnDefinitions.value);
                const filterValue = quickFilterValue.value.toLowerCase();

                const result = gridData.rows.filter(v => {
                    const quickFilterMatch = !filterValue || columns.some((column): boolean => {
                        const columnValue = v[column.name];
                        let value: string | undefined;

                        if (column.quickFilterValue) {
                            value = column.quickFilterValue(value);

                            if (value === undefined) {
                                return false;
                            }

                            value = value.toLowerCase();
                        }
                        else if (typeof columnValue === "string") {
                            value = columnValue;
                        }
                        else if (typeof columnValue === "number") {
                            value = columnValue.toString();
                        }
                        else {
                            value = undefined;
                        }

                        if (value === undefined) {
                            return false;
                        }

                        return value.toLowerCase().includes(filterValue);
                    });

                    const filtersMatch = columns.every(column => {
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

                filteredRows = result;
            }
            else {
                filteredRows = [];
            }
            console.log(`Filtering took ${Date.now() - start}ms.`);

            updateSortedRows();
            updatePageCount();
            updatePagerMessage();
        };

        const updateSortedRows = (): void => {
            const start = Date.now();
            const sortDirection = columnSortDirection.value;

            if (sortDirection) {
                const column = visibleColumnDefinitions.value.find(c => c.name === sortDirection.column);
                const order = sortDirection.isDescending ? -1 : 1;

                if (!column) {
                    throw new Error("Invalid sort definition");
                }

                const columnName = column.name;
                const sortValue = column.sortValue;
                console.log(columnName, sortValue, column);

                const rows = [...filteredRows];

                rows.sort((a, b) => {
                    const columnValueA = a[columnName];
                    const columnValueB = b[columnName];
                    let valueA: string | number | undefined;
                    let valueB: string | number | undefined;

                    if (sortValue) {
                        valueA = sortValue(columnValueA);
                        valueB = sortValue(columnValueB);
                    }
                    else {
                        if (typeof columnValueA === "string" || typeof columnValueA === "number") {
                            valueA = columnValueA;
                        }
                        else {
                            valueA = undefined;
                        }

                        if (typeof columnValueB === "string" || typeof columnValueB === "number") {
                            valueB = columnValueB;
                        }
                        else {
                            valueB = undefined;
                        }
                    }

                    if (valueA === undefined) {
                        return -order;
                    }
                    else if (valueB === undefined) {
                        return order;
                    }
                    else if (valueA < valueB) {
                        return -order;
                    }
                    else if (valueA > valueB) {
                        return order;
                    }
                    else {
                        return 0;
                    }
                });

                sortedRows = rows;
            }
            else {
                sortedRows = filteredRows;
            }
            console.log(`sortedRows took ${Date.now() - start}ms.`);

            updateVisibleRows();
        };

        const updateVisibleRows = (): void => {
            const startIndex = (currentPage.value - 1) * pageSize.value;

            visibleRows.value = sortedRows.slice(startIndex, startIndex + pageSize.value);
        };

        const updatePageCount = (): void => {
            pageCount.value = Math.ceil(filteredRows.length / pageSize.value);
        };

        const updatePagerMessage = (): void => {
            pagerMessage.value = `${asFormattedString(filteredRows.length)} ${pluralConditional(filteredRows.length, "Group", "Groups")}`;
        };

        const updateGridData = async (): Promise<void> => {
            loadingData.value = true;

            if (typeof props.data === "object") {
                gridData = props.data;
            }
            else if (typeof props.data === "function") {
                try {
                    gridData = await props.data();
                }
                catch (error) {
                    gridErrorMessage.value = error instanceof Error ? error.message : new String(error).toString();
                }
            }

            visibleColumnDefinitions.value = gridData?.columns ?? [];
            updateFilteredRows();

            loadingData.value = false;
        };

        const onActionClick = (): Promise<void> => {
            return new Promise(resolve => setTimeout(resolve, 2000));
        };

        watch([quickFilterValue, columnFilterValues], () => {
            currentPage.value = 1;
            updateFilteredRows();
        });

        watch(columnSortDirection, () => {
            updateSortedRows();
        });

        watch([currentPage, pageSize], () => {
            updateVisibleRows();
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
            columnFilterValues,
            columnSortDirection,
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
            transition-property: opacity, color;
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
    
        table.table-obsidian th.grid-column-header .grid-filter-popup .actions {
            border-top: 1px solid #eee;
            margin: 18px -12px 0px -12px;
            padding: 12px 12px 0px 12px;
        }

        table.table-obsidian th.grid-column-header .resize-handle {
            position: absolute;
            right: 0px;
            top: 0px;
            bottom: 0px;
            width: 2px;
            height: 100%;
            cursor: w-resize;
        }
        table.table-obsidian th.grid-column-header:hover .resize-handle {
            background-color: #eee;
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
        font-weight: initial;
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

        <GridColumnHeaderRow :columns="visibleColumnDefinitions" v-model:columnFilters="columnFilterValues" v-model:columnSort="columnSortDirection" />
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
