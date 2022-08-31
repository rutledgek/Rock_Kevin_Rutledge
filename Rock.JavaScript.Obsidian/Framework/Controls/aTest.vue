<template>
    <Alert alertType="success" dismissible />
    <div>{{ myValue }}</div>
</template>

<script lang="ts">
    import { defineComponent, PropType } from "vue";
    import Alert from "./alert2.vue";

    export default defineComponent({
        name: "aTest",

        components: {
            Alert
        },

        props: {
            /** The value to be displayed on the page. */
            myValue: {
                type: String as PropType<string>,
                required: false
            }
        },

        setup() {
            return {
                myValue: "hello"
            };
        }
    });
</script>
