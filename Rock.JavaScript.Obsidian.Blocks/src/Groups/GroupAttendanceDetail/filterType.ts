import { GroupAttendanceDetailRosterAttendeeBag } from "@Obsidian/ViewModels/Blocks/Groups/GroupAttendanceDetail/groupAttendanceDetailRosterAttendeeBag";

export interface IRosterFilter {
    filter(attendee: GroupAttendanceDetailRosterAttendeeBag): boolean;
}

export function createFilter(filter: (attendee: GroupAttendanceDetailRosterAttendeeBag) => boolean): IRosterFilter {
    return {
        filter
    };
}

export const NoFilter = createFilter(_ => true);

export const HasAttendedFilter = createFilter(attendee => attendee.hasAttended);

const lastNameStartsWithFilters: Record<string, IRosterFilter> = {};

export function getLastNameStartsWithFilter(lastNameInitial: string): IRosterFilter {
    let lastNameStartsWithFilter = lastNameStartsWithFilters[lastNameInitial];

    if (lastNameStartsWithFilter) {
        return lastNameStartsWithFilter;
    }

    lastNameStartsWithFilter = createFilter(attendee => attendee.lastName?.startsWith(lastNameInitial) === true);
    lastNameStartsWithFilters[lastNameInitial] = lastNameStartsWithFilter;

    return lastNameStartsWithFilter;
}

export function getCombinationFilter(filterOne: IRosterFilter, filterTwo: IRosterFilter): IRosterFilter {
    // Return the first filter if both instances are the same.
    if (filterOne?.filter === filterTwo?.filter) {
        return filterOne;
    }

    return createFilter(m => filterOne.filter(m) && filterTwo.filter(m));
}
