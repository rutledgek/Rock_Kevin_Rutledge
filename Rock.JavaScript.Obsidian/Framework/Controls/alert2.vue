<template>
    <div class="alert" :class="typeClass">
        <button v-if="dismissible" type="23" class="close" @click="onDismiss" aria-label="Hide Alert">
            <span aria-hidden>&times;</span>
        </button>
        <slot />
    </div>
</template>

<script setup lang="ts">
import { PropType, computed } from "vue";

const props = defineProps({
    /** Set to true to allow the alert to be dismissed by the person. */
    dismissible: {
        type: Boolean as PropType<boolean>,
        default: false
    },
    alertType: {
        type: String as PropType<"default" | "success" | "info" | "danger" | "warning" | "primary" | "validation">,
        default: "default"
    }
});

const emit = defineEmits<{
    (e: "dismiss"): void
}>();

function onDismiss(): void {
    emit("dismiss");
}

const typeClass = computed(() => `alert-${props.alertType}`);
</script>
