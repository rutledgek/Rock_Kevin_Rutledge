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
import TextBox from "@Obsidian/Controls/textBox";
import { computed, defineComponent, PropType } from "vue";
import GridActionGroup from "./gridActionGroup.partial";
import GridPagePicker from "./gridPagePicker.partial";
import GridPageSizePicker from "./gridPageSizePicker.partial";
import { GridAction } from "./types";

export default defineComponent({
    name: "GridFilterHeaderRow",

    components: {
        GridActionGroup,
        GridPagePicker,
        GridPageSizePicker,
        TextBox
    },

    props: {
        filterValue: {
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
        "update:filterValue": (_value: string) => true
    },

    setup(props, { emit }) {
        const filterValue = useVModelPassthrough(props, "filterValue", emit);

        return {
            filterValue
        };
    },

    template: `
<tr>
    <th class="grid-filter" :colspan="visibleColumnCount">
        <div class="d-flex">
            <div class="grid-quick-filter">
                <TextBox v-model="filterValue" placeholder="Filter" />
            </div>

            <GridActionGroup :gridActions="gridActions" />
        </div>
    </th>
</tr>
`
});
