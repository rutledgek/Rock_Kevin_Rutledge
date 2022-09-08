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

import { isPromise } from "@Obsidian/Utility/promiseUtils";
import { computed, defineComponent, PropType } from "vue";
import { GridAction } from "./types";

const gridActionButton = defineComponent({
    name: "GridActionButton",

    props: {
        action: {
            type: Object as PropType<GridAction>,
            required: true
        }
    },

    setup(props) {
        const actionTooltip = computed((): string | undefined => {
            return props.action.tooltip;
        });
        
        const buttonCssClass = computed((): string => {
            let classes = "btn btn-grid-action";

            if (props.action.executing || props.action.disabled) {
                classes += " disabled";
            }

            return classes;
        });

        const iconCssClass = computed((): string => {
            return props.action.iconCssClass || "fa fa-square";
        });

        const onActionClick = async (event: Event): Promise<void> => {
            if (props.action.handler && !props.action.disabled && !props.action.executing) {
                // This is a special case where we modify the original object.
                // It is only used internally by the Grid so this is safe.
                props.action.executing = true;

                try {
                    const result = props.action.handler(event);

                    if (isPromise(result)) {
                        await result;
                    }
                }
                finally {
                    props.action.executing = false;
                }
            }
        };

        return {
            actionTooltip,
            buttonCssClass,
            iconCssClass,
            onActionClick
        };
    },

    template: `
<button :class="buttonCssClass"
    :title="actionTooltip"
    @click.prevent.stop="onActionClick">
    <i :class="iconCssClass"></i>
</button>
`
});

export default defineComponent({
    name: "GridActionGroup",

    components: {
        GridActionButton: gridActionButton
    },

    props: {
        gridActions: {
            type: Array as PropType<GridAction[]>,
            default: []
        }
    },

    setup() {
        return {
        };
    },

    template: `
<div class="grid-actions">
    <GridActionButton v-for="action in gridActions" :action="action" />
</div>
`
});
