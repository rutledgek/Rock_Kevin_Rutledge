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
import { defineComponent, PropType } from "vue";
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
        }
    },

    emits: {
        "update:columnFilters": (_value: Record<string, unknown | undefined>) => true
    },

    setup(props, { emit }) {
        const columnFilters = useVModelPassthrough(props, "columnFilters", emit);

        const onUpdateFilterValue = (columnName: string, filterValue: unknown | undefined): void => {
            const newFilters = {...columnFilters.value};

            newFilters[columnName] = filterValue;

            columnFilters.value = newFilters;
        };
        
        return {
            columnFilters,
            onUpdateFilterValue
        };
    },

    template: `
<tr>
    <GridColumnHeaderCell v-for="column in columns" :column="column" :filterValue="columnFilters[column.name]" @update:filterValue="onUpdateFilterValue(column.name, $event)" />
</tr>
`
});
