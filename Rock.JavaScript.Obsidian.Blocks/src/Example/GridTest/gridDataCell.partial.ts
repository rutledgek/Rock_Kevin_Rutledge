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

import { escapeHtml } from "@Obsidian/Utility/stringUtils";
import { computed, defineComponent, PropType } from "vue";
import { GridColumnDefinition } from "./types";

export default defineComponent({
    name: "GridDataRow",

    components: {
    },

    props: {
        column: {
            type: Object as PropType<GridColumnDefinition>,
            required: true
        },

        data: {
            type: Object as PropType<unknown>,
            required: true
        }
    },

    setup(props) {
        const htmlContent = computed((): string => {
            if (props.column.format) {
                return props.column.format(props.data);
            }

            if (!props.data) {
                return "";
            }

            return escapeHtml(new String(props.data).toString());
        });

        return {
            htmlContent
        };
    },

    template: `
<td v-html="htmlContent">
</td>
`
});
