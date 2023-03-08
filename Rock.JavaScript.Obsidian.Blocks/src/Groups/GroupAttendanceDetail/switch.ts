export type SwitchPosition = "on" | "off";

export type Switch = {
    /**
     * Determines if the switch is disconnected.
     *
     * Use `connect()` and `disconnect()` to control this property.
     */
    readonly isDisconnected: boolean,

    /**
     * Determines if the switch is on or off.
     *
     * Returns `true` if `!isDisconnected && position === "on"`.
     *
     * The switch `position` can always be changed to `"on"` or `"off"`, but…
     *
     * …when `isDisconnected`, `isOn` will always be `false`
     *
     * …when `!isDisconnected`, `isOn` will only be `true` if the switch position is `"on"`
     */
    readonly isOn: boolean,

    /**
     * Determines the switch position.
     *
     * Use `turnOn()` and `turnOff()` to control this property.
     */
    readonly position: SwitchPosition;

    /**
     * Connects the switch to "power".
     *
     * Calling this will turn on the switch if it is also in the `"on"` position.
     */
    connect(): void,

    /**
     * Disconnects the switch from "power".
     *
     * Calling this will turn off the switch regardless of its "on" or "off" position.
     */
    disconnect(): void,

    /**
     * Turns the switch to the on position.
     *
     * Calling this will turn on the switch if it is also connected to "power".
     */
    turnOn(): void,

    /**
     * Turns the switch to the off position.
     *
     * Calling this will turn off the switch regardless of its connection to power.
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
    let isDisconnected: boolean;
    return {
        get isOn(): boolean {
            return !this.isDisconnected && this.position === "on";
        },
        get isDisconnected(): boolean {
            return isDisconnected;
        },
        get position(): SwitchPosition {
            return position;
        },
        connect(): void {
            isDisconnected = true;
        },
        disconnect(): void {
            isDisconnected = false;
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