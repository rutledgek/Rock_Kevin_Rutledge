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

import { useVModelPassthrough } from "@Obsidian/Utility/component";
import { defineComponent, PropType, ref } from "vue";
import GridColumnHeaderCell from "./gridColumnHeaderCell.partial";
import { GridColumnDefinition } from "./types";

export default defineComponent({
    name: "GridColumnHeaderRow",

    components: {
        GridColumnHeaderCell
    },

    props: {
        columns: {
            type: Array as PropType<GridColumnDefinition[]>,
            default: []
        },

        columnFilters: {
            type: Object as PropType<Record<string, unknown | undefined>>,
            default: {}
        },

        columnSort: {
            type: Object as PropType<{ column: string, isDescending: boolean }>,
            required: false
        }
    },

    emits: {
        "update:columnFilters": (_value: Record<string, unknown | undefined>) => true,
        "update:columnSort": (_value: { column: string, isDescending: boolean } | undefined) => true
    },

    setup(props, { emit }) {
        const columnFilters = useVModelPassthrough(props, "columnFilters", emit);
        const columnSort = useVModelPassthrough(props, "columnSort", emit);
        const columnSortLookup = ref<Record<string, number>>({});

        const updateSortLookup = (): void => {
            const lookup: Record<string, number> = {};

            for (const column of props.columns) {
                if (columnSort.value && columnSort.value.column === column.name) {
                    lookup[column.name] = columnSort.value.isDescending ? -1 : 1;
                }
                else {
                    lookup[column.name] = 0;
                }
            }

            columnSortLookup.value = lookup;
        };

        const onUpdateFilterValue = (columnName: string, filterValue: unknown | undefined): void => {
            const newFilters = { ...columnFilters.value };

            newFilters[columnName] = filterValue;

            columnFilters.value = newFilters;
        };

        const onUpdateSortDirection = (columnName: string, direction: number): void => {
            if (direction !== 0) {
                columnSort.value = {
                    column: columnName,
                    isDescending: direction < 0
                };

                updateSortLookup();
            }
        };

        updateSortLookup();

        return {
            columnFilters,
            columnSortLookup,
            onUpdateFilterValue,
            onUpdateSortDirection
        };
    },

    template: `
<tr>
    <GridColumnHeaderCell v-for="column in columns"
        :column="column"
        :filterValue="columnFilters[column.name]"
        :sortDirection="columnSortLookup[column.name]"
        @update:filterValue="onUpdateFilterValue(column.name, $event)"
        @update:sortDirection="onUpdateSortDirection(column.name, $event)" />
</tr>
`
});
