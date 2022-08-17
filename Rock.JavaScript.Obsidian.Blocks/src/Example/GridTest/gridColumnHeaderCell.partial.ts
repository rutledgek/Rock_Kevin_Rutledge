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
import { computed, defineComponent, nextTick, PropType, ref, watch } from "vue";
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

        /** -1 means descending, 1 means ascending, 0 means no sort applied. */
        sortDirection: {
            type: Number as PropType<number>,
            default: 0
        },

        column: {
            type: Object as PropType<GridColumnDefinition>,
            required: true
        }
    },

    emits: {
        "update:filterValue": (_value: unknown | undefined) => true,
        "update:sortDirection": (_value: number) => true
    },

    setup(props, { emit }) {
        const filterValue = useVModelPassthrough(props, "filterValue", emit);
        const sortDirection = useVModelPassthrough(props, "sortDirection", emit);
        const internalFilterValue = ref(filterValue.value ?? "");
        const isFilterVisible = ref(false);
        const filterElement = ref<HTMLElement | null>(null);
        const filterButtonElement = ref<HTMLElement | null>(null);
        const resizeHandleElement = ref<HTMLElement | null>(null);
        const columnHeaderElement = ref<HTMLElement | null>(null);
        let resizePositionX = 0;
        let resizeWidth = 0;

        const hasFilter = computed((): boolean => props.column.filter !== undefined);

        const buttonCssClass = computed((): string => {
            return filterValue.value !== undefined ? "btn-grid-column-filter active" : "btn-grid-column-filter";
        });
        
        const isSortAscending = computed((): boolean => sortDirection.value > 0);
        const isSortDescending = computed((): boolean => sortDirection.value < 0);

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

        const onFilterClearClick = (): void => {
            isFilterVisible.value = false;
            filterValue.value = undefined;
        };

        const onFilterCloseClick = (): void => {
            isFilterVisible.value = false;
        };

        const onResizeMouseDown = (e: MouseEvent): void => {
            if (!columnHeaderElement.value) {
                return;
            }

            resizePositionX = e.pageX;
            resizeWidth = columnHeaderElement.value.clientWidth;

            document.body.addEventListener("mousemove", onResizeMouseMove);
            document.body.addEventListener("mouseup", onResizeMouseUp);
        };

        const onResizeMouseUp = (): void => {
            document.body.removeEventListener("mousemove", onResizeMouseMove);
            document.body.removeEventListener("mouseup", onResizeMouseUp);
        };

        const onResizeMouseMove = (e: MouseEvent): void => {
            if (!columnHeaderElement.value) {
                return;
            }

            const changeAmount = resizePositionX - e.pageX;

            columnHeaderElement.value.style.width = `${resizeWidth - changeAmount}px`;
        };

        const onSortClick = (): void => {
            if (sortDirection.value > 0) {
                sortDirection.value = -1;
            }
            else {
                sortDirection.value = 1;
            }
        };

        watch(filterValue, () => internalFilterValue.value = filterValue.value ?? "");
        watch (internalFilterValue, () => {
            // Temporary hack until we have actual filter components.
            if (internalFilterValue.value === "") {
                filterValue.value = undefined;
            }
            else {
                filterValue.value = internalFilterValue.value;
            }
        });

        watch(isFilterVisible, () => {
            if (isFilterVisible.value) {
                document.addEventListener("mousedown", onMouseDownEvent);

                nextTick(() => {
                    // Hack until this is handled by a filter component.
                    filterElement.value?.querySelector("input")?.focus();
                });
            }
            else {
                document.removeEventListener("mousedown", onMouseDownEvent);
            }
        });

        watch(resizeHandleElement, () => {
            if (!resizeHandleElement.value) {
                return;
            }

            resizeHandleElement.value.addEventListener("mousedown", onResizeMouseDown);
        });

        return {
            buttonCssClass,
            columnHeaderElement,
            filterButtonElement,
            filterElement,
            internalFilterValue,
            isSortAscending,
            isSortDescending,
            hasFilter,
            isFilterVisible,
            onFilterClearClick,
            onFilterClick,
            onFilterCloseClick,
            onSortClick,
            resizeHandleElement
        };
    },

    template: `
<th ref="columnHeaderElement" class="grid-column-header">
    <span class="grid-column-title clickable" @click.prevent="onSortClick">{{ column.title ?? column.name }}</span>

    <span v-if="isSortAscending" class="grid-sort-direction"><i class="fa fa-sort-alpha-up"></i></span>
    <span v-else-if="isSortDescending" class="grid-sort-direction"><i class="fa fa-sort-alpha-down"></i></span>

    <button v-if="hasFilter"
        ref="filterButtonElement"
        :class="buttonCssClass"
        @click.prevent="onFilterClick">
        <i class="fa fa-filter "></i>
    </button>

    <div ref="resizeHandleElement" class="resize-handle"></div>

    <div v-if="isFilterVisible" ref="filterElement" class="grid-filter-popup">
        <TextBox v-model="internalFilterValue" />

        <div class="actions">
            <button class="btn btn-primary btn-xs" @click.prevent="onFilterCloseClick">Close</button>
            <button class="btn btn-link btn-xs" @click.prevent="onFilterClearClick">Clear</button>
        </div>
    </div>
</th>
`
});
