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

import TextBox from "@Obsidian/Controls/textBox";
import { useVModelPassthrough } from "@Obsidian/Utility/component";
import { computed, defineComponent, PropType, ref, watch } from "vue";
import { GridColumnDefinition } from "./types";

export default defineComponent({
    name: "GridColumnHeaderCell",

    components: {
        TextBox
    },

    props: {
        filterValue: {
            type: Object as PropType<unknown>,
            required: false
        },

        column: {
            type: Object as PropType<GridColumnDefinition>,
            required: true
        }
    },

    emits: {
        "update:filterValue": (_value: unknown | undefined) => true
    },

    setup(props, { emit }) {
        const filterValue = useVModelPassthrough(props, "filterValue", emit);
        const isFilterVisible = ref(false);
        const filterElement = ref<HTMLElement | null>(null);
        const filterButtonElement = ref<HTMLElement | null>(null);

        const hasFilter = computed((): boolean => props.column.filter !== undefined);

        const onMouseDownEvent = (ev: MouseEvent): void => {
            if (ev.target && ev.target instanceof HTMLElement) {
                if (filterElement.value && filterElement.value.contains(ev.target)) {
                    return;
                }

                if (filterButtonElement.value && filterButtonElement.value.contains(ev.target)) {
                    return;
                }
            }

            isFilterVisible.value = false;
        };

        const onFilterClick = (): void => {
            isFilterVisible.value = !isFilterVisible.value;
        };

        watch(isFilterVisible, () => {
            if (isFilterVisible.value) {
                document.addEventListener("mousedown", onMouseDownEvent);
            }
            else {
                document.removeEventListener("mousedown", onMouseDownEvent);
            }
        });

        return {
            filterButtonElement,
            filterElement,
            filterValue,
            hasFilter,
            isFilterVisible,
            onFilterClick
        };
    },

    template: `
<th class="grid-column-header">
    <span class="grid-column-title">{{ column.title ?? column.name }}</span>

    <button v-if="hasFilter"
        ref="filterButtonElement"
        class="btn-grid-column-filter"
        @click.prevent="onFilterClick">
        <i class="fa fa-filter "></i>
    </button>

    <div v-if="isFilterVisible" ref="filterElement" class="grid-filter-popup">
        <TextBox v-model="filterValue" />
    </div>
</th>
`
});
