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

import { defineComponent, PropType } from "vue";
import GridDataCell from "./gridDataCell.partial";
import { GridColumnDefinition } from "./types";

export default defineComponent({
    name: "GridDataRow",

    components: {
        GridDataCell
    },

    props: {
        columns: {
            type: Array as PropType<GridColumnDefinition[]>,
            default: []
        },

        data: {
            type: Object as PropType<Record<string, unknown>>,
            required: true
        }
    },

    setup(props) {
        return {
        };
    },

    template: `
<tr>
    <GridDataCell v-for="column in columns" :column="column" :data="data[column.name]" />
</tr>
`
});
