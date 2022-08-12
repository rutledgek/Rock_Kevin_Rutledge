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

/** A function that will be called in response to an action. */
export type GridActionCallback = (event: Event) => void | Promise<void>;

/** Defines a single action related to a Grid control. */
export type GridAction = {
    /**
     * The title of the action, this should be a very short (one or two words)
     * description of the action that will be performed, such as "Delete".
     */
    title?: string;

    /**
     * The tooltip to display for this action.
     */
    tooltip?: string;

    /**
     * The CSS class for the icon used when displaying this action.
     */
    iconCssClass?: string;

    /** The callback function that will handle the action. */
    handler?: GridActionCallback;

    /** If true then the action will be disabled and not respond to clicks. */
    disabled?: boolean;

    /** True if the action is currently executing. */
    executing: boolean;
};

type GridData = {
    columns: GridColumnDefinition[];

    rows: Record<string, unknown>[];
};

type GridColumnDefinition = {
    /** The name of the property in the rows objects. */
    name: string;

    /** The title to display in the column header. */
    title?: string | null;

    /** Formats the value for display in the cell. Should return HTML safe content. */
    format?: (value: unknown) => string;

    /** Determines if the value matches the quick search filter. */
    quickFilter?: (needle: string, haystack: unknown) => boolean;

    /** Determines if the value matches the custom column filter. */
    filter?: (needle: unknown, haystack: unknown) => boolean;

    /** Sorts the two values and determines the order. */
    sort?: (a: unknown, b: unknown) => number;
};
