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
import { computed, defineComponent, PropType } from "vue";
import GridActionGroup from "./gridActionGroup.partial";
import GridPagePicker from "./gridPagePicker.partial";
import GridPageSizePicker from "./gridPageSizePicker.partial";
import { GridAction } from "./types";

export default defineComponent({
    name: "GridPagerRow",

    components: {
        GridActionGroup,
        GridPagePicker,
        GridPageSizePicker
    },

    props: {
        pageSize: {
            type: Number as PropType<number>,
            required: true
        },

        pageSizes: {
            type: Array as PropType<number[]>,
            default: []
        },

        currentPage: {
            type: Number as PropType<number>,
            default: 1
        },

        pageCount: {
            type: Number as PropType<number>,
            default: 1
        },

        message: {
            type: String as PropType<string>,
            default: ""
        },

        gridActions: {
            type: Array as PropType<GridAction[]>,
            default: []
        },

        visibleColumnCount: {
            type: Number as PropType<number>,
            default: 0
        }
    },

    emits: {
        "update:pageSize": (_value: number) => true,
        "update:currentPage": (_value: number) => true
    },

    setup(props, { emit }) {
        const pageSize = useVModelPassthrough(props, "pageSize", emit);
        const currentPage = useVModelPassthrough(props, "currentPage", emit);

        const hasMultiplePages = computed((): boolean => {
            return props.pageCount > 1;
        });

        const getPageSizeClass = (size: number): string | undefined => {
            return pageSize.value === size ? "active" : undefined;
        };

        const onPageSizeClick = (size: number): void => {
            pageSize.value = size;
        };

        return {
            currentPage,
            getPageSizeClass,
            hasMultiplePages,
            pageSize,
            onPageSizeClick
        };
    },

    template: `
<tr>
    <td class="grid-paging" :colspan="visibleColumnCount">
        <div class="d-flex">
            <div class="grid-page-sizes">
                <GridPageSizePicker v-model="pageSize" :pageSizes="pageSizes" />
        
                <div v-if="message" class="grid-itemcount">{{ message }}</div>
            </div>

            <GridPagePicker v-model="currentPage" :pageCount="pageCount" />

            <GridActionGroup :gridActions="gridActions" />
        </div>
    </td>
</tr>
`
});
