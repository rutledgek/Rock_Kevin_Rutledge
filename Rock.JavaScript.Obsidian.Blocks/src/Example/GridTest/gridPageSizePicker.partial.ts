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

export default defineComponent({
    name: "GridPageSizePicker",

    components: {
    },

    props: {
        modelValue: {
            type: Number as PropType<number>,
            required: true
        },

        pageSizes: {
            type: Array as PropType<number[]>,
            default: []
        }
    },

    emits: {
        "update:modelValue": (_value: number) => true,
    },

    setup(props, { emit }) {
        const pageSize = useVModelPassthrough(props, "modelValue", emit);

        const getPageSizeClass = (size: number): string | undefined => {
            return pageSize.value === size ? "active" : undefined;
        };

        const onPageSizeClick = (size: number): void => {
            pageSize.value = size;
        };

        return {
            getPageSizeClass,
            onPageSizeClick
        };
    },

    template: `
<ul v-if="pageSizes" class="grid-pagesize pagination pagination-sm">
    <li v-for="size in pageSizes" :class="getPageSizeClass(size)">
        <a href="#" @click.prevent="onPageSizeClick(size)">{{ size }}</a>
    </li>
</ul>
`
});
