import { GroupAttendanceDetailAttendanceBag } from "@Obsidian/ViewModels/Blocks/Groups/GroupAttendanceDetail/groupAttendanceDetailAttendanceBag";

function bindThis(filter: IRosterFilter): void {
    filter.filter.bind(filter);
    filter.hasFilter.bind(filter);
    filter.isFilter.bind(filter);
}

export interface IRosterFilter {
    filter(attendance: GroupAttendanceDetailAttendanceBag): boolean;
    /**
     * Returns `true` if this filter is the same instance as `rosterFilter`; otherwise, `false`.
     *
     * Useful if you want to check if this is a specific type of filter.
     */
    isFilter(rosterFilter: IRosterFilter): boolean;
    /**
     * Returns `true` if this filter is the same instance as `rosterFilter` OR if `rosterFilter` is one of its own aggregate filters; otherwise, `false`.
     *
     * Useful if you want to check if this filter is or has a specific type of filter.
     */
    hasFilter(rosterFilter: IRosterFilter): boolean;
}

export interface IAggregateRosterFilter extends IRosterFilter {
    filters: IRosterFilter[];
}

export function createFilter(filter: (attendance: GroupAttendanceDetailAttendanceBag) => boolean): IRosterFilter {
    const rosterFilter: IRosterFilter = {
        filter,
        hasFilter: function(filter: IRosterFilter): boolean {
            return hasSameFilter(this, filter);
        },
        isFilter: function(filter: IRosterFilter): boolean {
            return isSameFilter(this, filter);
        }
    };

    bindThis(rosterFilter);

    return rosterFilter;
}

export function createAggregateFilter(filters: IRosterFilter[], filter: (filters: IRosterFilter[], attendance: GroupAttendanceDetailAttendanceBag) => boolean): IAggregateRosterFilter {
    const aggregateRosterFilter: IAggregateRosterFilter = {
        hasFilter: function(filter: IRosterFilter): boolean {
            return hasSameFilter(this, filter);
        },
        isFilter: function(filter: IRosterFilter): boolean {
            return isSameFilter(this, filter);
        },
        filters: filters,
        filter: function(attendance: GroupAttendanceDetailAttendanceBag): boolean {
            return filter(this.filters, attendance);
        }
    };

    bindThis(aggregateRosterFilter);

    return aggregateRosterFilter;
}

export const NoFilter = createFilter(_ => true);

export const DidAttendFilter = createFilter(attendance => attendance.didAttend);

const lastNameStartsWithFilters: Record<string, IRosterFilter> = {};

export function createLastNameStartsWithFilter(lastNameInitial: string): IRosterFilter {
    let lastNameStartsWithFilter = lastNameStartsWithFilters[lastNameInitial];

    if (lastNameStartsWithFilter) {
        return lastNameStartsWithFilter;
    }

    lastNameStartsWithFilter = createFilter(attendance => attendance.lastName?.startsWith(lastNameInitial) === true);
    lastNameStartsWithFilters[lastNameInitial] = lastNameStartsWithFilter;

    return lastNameStartsWithFilter;
}

const firstNameStartsWithFilters: Record<string, IRosterFilter> = {};

export function createFirstNameStartsWithFilter(firstNameInitial: string): IRosterFilter {
    let firstNameStartsWithFilter = firstNameStartsWithFilters[firstNameInitial];

    if (firstNameStartsWithFilter) {
        return firstNameStartsWithFilter;
    }

    firstNameStartsWithFilter = createFilter(attendance => attendance.nickName?.startsWith(firstNameInitial) === true);
    firstNameStartsWithFilters[firstNameInitial] = firstNameStartsWithFilter;

    return firstNameStartsWithFilter;
}

export function createLogicalAndFilter(rosterFilter1: IRosterFilter, rosterFilter2: IRosterFilter): IRosterFilter {
    // Return the first filter if both are the same instance.
    if (isSameFilter(rosterFilter1, rosterFilter2)) {
        return rosterFilter1;
    }

    return createEveryFilter(rosterFilter1, rosterFilter2);
}

export function createLogicalOrFilter(rosterFilter1: IRosterFilter, rosterFilter2: IRosterFilter): IRosterFilter {
    // Return the first filter if both are the same instance.
    if (isSameFilter(rosterFilter1, rosterFilter2)) {
        return rosterFilter1;
    }

    return createSomeFilter(rosterFilter1, rosterFilter2);
}

export function createLogicalNotFilter(rosterFilter: IRosterFilter): IRosterFilter {
    return createFilter(m => !rosterFilter.filter(m));
}

/**
 * Creates a filter that will return `true` if any of the specified `rosterFilters` returns `true`; otherwise, `false`.
 *
 * The aggregate filters can be modified via the `filters` property of the returned object.
 */
export function createSomeFilter(...rosterFilters: IRosterFilter[]): IAggregateRosterFilter {
    return createAggregateFilter(rosterFilters, (filters, attendance) => filters.some(filter => filter.filter(attendance)));
}

/**
 * Creates a filter that will return `true` if all of the specified `rosterFilters` returns `true`; otherwise, `false`.
 *
 * The aggregate filters can be modified via the `filters` property of the returned object.
 */
export function createEveryFilter(...rosterFilters: IRosterFilter[]): IAggregateRosterFilter {
    return createAggregateFilter(rosterFilters, (filters, attendance) => filters.every(filter => filter.filter(attendance)));
}

/**
 * Returns `true` if `rosterFilter1` is the same instance as `rosterFilter2`; otherwise, `false`.
 */
function isSameFilter(rosterFilter1: IRosterFilter, rosterFilter2: IRosterFilter): boolean {
    return rosterFilter1?.filter === rosterFilter2?.filter;
}

/**
 * Returns `true` if `rosterFilter1` is the same instance as `rosterFilter2` OR if `rosterFilter2` is one of `rosterFilter1`'s own aggregate filters; otherwise, `false`
 */
function hasSameFilter(rosterFilter1: IRosterFilter, rosterFilter2: IRosterFilter): boolean {
    if (rosterFilter1?.filter === rosterFilter2?.filter) {
        return true;
    }

    if (isAggregateRosterFilter(rosterFilter1)) {
        return rosterFilter1.filters.some(f => isSameFilter(f, rosterFilter2));
    }

    return false;
}

function isAggregateRosterFilter(rosterFilter: IRosterFilter): rosterFilter is IAggregateRosterFilter {
    return !!(rosterFilter as IAggregateRosterFilter)?.filters;
}