<template>
    <Grid :data="loadGridData">
        <Column name="name" title="Name" sortValue="{{ row.name.lastName }} {{ row.name.firstName }}">
            <template #body="{ row }">
                {{ row.name.firstName }} {{ row.name.lastName }}
            </template>
        </Column>

        <Column name="email" title="Email" field="email" sortField="email" />

        <Column name="enteredDateTime" title="Entered On" field="enteredDateTime" sortField="enteredDateTime">
            <template #body="{ row }">
                {{ formatDate(row.enteredDateTime) }}
            </template>
        </Column>

        <DateColumn name="expirationDateTime" title="Expires" field="expirationDateTime" sortField="expirationDateTime" />

        <BooleanColumn name="isUrgent" title="Urgent" field="isUrgent" sortField="isUrgent" />

        <BooleanColumn name="isPublic" title="Public" field="isPublic" sortField="isPublic" />

        <AttributeColumns :attributes="attributeColumns" />
    </Grid>
</template>

<script setup lang="ts">
// #region Imports
import { useConfigurationValues, useInvokeBlockAction } from "@Obsidian/Utility/block";
import { computed, defineComponent, PropType, VNode } from "vue";
import { RockDateTime } from "@Obsidian/Utility/rockDateTime";
import Grid from "./GridTest/grid.partial";
import { AttributeColumnDefinition, GridColumnDefinition, GridData, GridDefinition } from "./GridTest/types";

const Column = defineComponent({
    props: {
        name: {
            type: String as PropType<string>,
            default: ""
        },

        title: {
            type: String as PropType<string>,
            required: false
        },

        sortField: {
            type: String as PropType<string>,
            required: false
        },

        sortValue: {
            type: Object as PropType<((row: Record<string, unknown>, column: GridColumnDefinition) => string | number | undefined)>,
            required: false
        }
    }
});

const dateColumnValueComponent = defineComponent({
    props: {
        column: {
            type: Object as PropType<GridColumnDefinition>,
            required: true
        },
        row: {
            type: Object as PropType<Record<string, unknown>>,
            required: true
        }
    },

    setup(props) {
        const formattedValue = computed(() => {
            if (props.column.field) {
                return RockDateTime.parseISO(props.row[props.column.field] as string)?.toASPString("d") ?? "";
            }
            else {
                return "";
            }
        });

        return {
            formattedValue
        };
    },

    template: `{{ formattedValue }}`
});

const DateColumn = defineComponent({
    props: {
        name: {
            type: String as PropType<string>,
            default: ""
        },

        title: {
            type: String as PropType<string>,
            required: false
        },

        sortField: {
            type: String as PropType<string>,
            required: false
        },

        sortValue: {
            type: Object as PropType<((row: Record<string, unknown>, column: GridColumnDefinition) => string | number | undefined)>,
            required: false
        },

        format: {
            type: Object as PropType<VNode>,
            required: false,
            default: dateColumnValueComponent
        }
    }
});

const booleanColumnValueComponent = defineComponent({
    props: {
        column: {
            type: Object as PropType<GridColumnDefinition>,
            required: true
        },
        row: {
            type: Object as PropType<Record<string, unknown>>,
            required: true
        }
    },

    setup(props) {
        const isTrue = computed(() => {
            if (props.column.field) {
                return props.row[props.column.field] === true;
            }
            else {
                return false;
            }
        });

        return {
            isTrue
        };
    },

    template: `<i v-if="isTrue" class="fa fa-check"></i>`
});

function booleanColumnSortValue(row: Record<string, unknown>, column: GridColumnDefinition): number | undefined {
    if (!column.sortField) {
        return undefined;
    }

    return row[column.sortField] === true ? 1 : 0;
}

const BooleanColumn = defineComponent({
    props: {
        name: {
            type: String as PropType<string>,
            default: ""
        },

        title: {
            type: String as PropType<string>,
            required: false
        },

        sortField: {
            type: String as PropType<string>,
            required: false
        },

        sortValue: {
            type: Object as PropType<((row: Record<string, unknown>, column: GridColumnDefinition) => string | number | undefined)>,
            required: false,
            default: booleanColumnSortValue
        },

        format: {
            type: Object as PropType<VNode>,
            required: false,
            default: booleanColumnValueComponent
        }
    }
});

const AttributeColumns = defineComponent({
    components: {
        Column
    },

    props: {
        attributes: {
            type: Array as PropType<AttributeColumnDefinition[]>,
            default: []
        },

        __attributeColumns: {
            type: Boolean as PropType<boolean>,
            default: true
        }
    }
});

// #endregion

const configuration = useConfigurationValues<{ gridDefinition: GridDefinition }>();
const invokeBlockAction = useInvokeBlockAction();

const attributeColumns = computed((): AttributeColumnDefinition[] => {
    return configuration.gridDefinition.attributeColumns ?? [];
});

const loadGridData = async (): Promise<GridData> => {
    const result = await invokeBlockAction<GridData>("GetGridData");

    if (result.isSuccess && result.data) {
        return {
            rows: result.data.rows
        };
    }
    else {
        throw new Error(result.errorMessage ?? "Unknown error while trying to load grid data.");
    }
};

function formatDate(value?: string): string {
    if (!value) {
        return "";
    }

    const dt = RockDateTime.parseISO(value);

    return dt?.toASPString("g") ?? "";
}
</script>
