export type SwitchPosition = "on" | "off";

export type Switch = {
    /**
     * Determines if the switch is disabled.
     *
     * Use `enable()` and `disable()` to control this property.
     */
    readonly isDisabled: boolean,

    /**
     * Determines if the switch is on or off.
     *
     * Returns `true` if `!isDisabled && position === "on"`.
     *
     * The switch `position` can always be changed to `"on"` or `"off"`, but…
     *
     * …when `isDisabled`, `isOn` will always be `false`
     *
     * …when `!isDisabled`, `isOn` will only be `true` if the switch position is `"on"`
     */
    readonly isOn: boolean,

    /**
     * Determines the switch position.
     *
     * Use `turnOn()` and `turnOff()` to control this property.
     */
    readonly position: SwitchPosition;

    /**
     * Enables the switch.
     *
     * Calling this will turn on the switch if it is also in the `"on"` position.
     */
    enable(): void,

    /**
     * Disables the switch.
     *
     * Calling this will turn off the switch regardless of its "on" or "off" position.
     */
    disable(): void,

    /**
     * Turns the switch to the on position.
     *
     * Calling this will turn on the switch if it is enabled.
     */
    turnOn(): void,

    /**
     * Turns the switch to the off position.
     *
     * Calling this will turn off the switch regardless of enabled/disabled state.
     */
    turnOff(): void,

    /**
     * Returns a wrapped version of the function `f` that, when invoked, will only invoke the original `f` if this switch is on.
     */
    connectToFunc<Request>(f: (r: Request) => Promise<void>): typeof f
};

/**
 * Creates a switch that can be turned on and off.
 */
export function createSwitch(): Switch {
    let position: SwitchPosition;
    let isDisabled: boolean;
    return {
        get isOn(): boolean {
            return !this.isDisabled && this.position === "on";
        },
        get isDisabled(): boolean {
            return isDisabled;
        },
        get position(): SwitchPosition {
            return position;
        },
        enable(): void {
            isDisabled = false;
        },
        disable(): void {
            isDisabled = true;
        },
        turnOn(): void {
            if (position === "on") {
                return;
            }

            position = "on";
        },
        turnOff(): void {
            if (position === "off") {
                return;
            }

            position = "off";
        },
        connectToFunc<Request>(func: (request: Request) => Promise<void>): typeof func {
            return async (r: Request): Promise<void> => {
                if (!this.isOn) {
                    return;
                }

                return await func(r);
            };
        }
    };
}