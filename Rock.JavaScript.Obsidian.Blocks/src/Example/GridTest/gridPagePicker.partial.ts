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

export default defineComponent({
    name: "GridPagePicker",

    components: {
    },

    props: {
        modelValue: {
            type: Number as PropType<number>,
            required: true
        },

        pageCount: {
            type: Number as PropType<number>,
            default: 1
        }
    },

    emits: {
        "update:modelValue": (_value: number) => true,
    },

    setup(props, { emit }) {
        const currentPage = useVModelPassthrough(props, "modelValue", emit);

        const pageButtons = computed((): number[] => {
            const firstNumber = Math.max(1, currentPage.value - 3);
            const lastNumber = Math.min(props.pageCount, currentPage.value + 3);
            const values: number[] = [];

            for (let i = firstNumber; i <= lastNumber; i++) {
                values.push(i);
            }

            return values;
        });

        const prevButtonCssClass = computed((): string => {
            return currentPage.value > 1 ? "prev" : "prev disabled";
        });

        const nextButtonCssClass = computed((): string => {
            return currentPage.value < props.pageCount ? "next" : "next disabled";
        });

        const hasMultiplePages = computed((): boolean => {
            return props.pageCount > 1;
        });

        const getPageButtonCssClass = (pageButton: number): string | undefined => {
            return pageButton === currentPage.value ? "active" : undefined;
        };

        const onPageButtonClick = (pageButton: number): void => {
            currentPage.value = pageButton;
        };

        const onPrevButtonClick = (): void => {
            currentPage.value = Math.max(1, currentPage.value - 1);
        };

        const onNextButtonClick = (): void => {
            currentPage.value = Math.min(props.pageCount, currentPage.value + 1);
        };

        return {
            currentPage,
            getPageButtonCssClass,
            hasMultiplePages,
            nextButtonCssClass,
            onNextButtonClick,
            onPageButtonClick,
            onPrevButtonClick,
            pageButtons,
            prevButtonCssClass
        };
    },

    template: `
<div v-if="hasMultiplePages" class="grid-page-picker">
    <button :class="prevButtonCssClass" @click.prevent="onPrevButtonClick">
        <i class="fa fa-angle-double-left"></i>
    </button>

    <button v-for="pageButton in pageButtons" :class="getPageButtonCssClass(pageButton)" @click.prevent="onPageButtonClick(pageButton)">
        {{ pageButton }}
    </button>

    <button :class="nextButtonCssClass" @click.prevent="onNextButtonClick">
        <i class="fa fa-angle-double-right"></i>
    </button>
</div>
`
});
