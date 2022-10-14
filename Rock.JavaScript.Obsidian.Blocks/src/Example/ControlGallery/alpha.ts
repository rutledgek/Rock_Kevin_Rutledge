import { defineComponent } from "vue";

/** Demonstrates a phone number box */
export default defineComponent({
    name: "PhoneNumberBoxGallery",
    setup() {
        return {
            hello: "there"
        };
    },
    template: `
<div>Hello {{hello}}</div>`
});