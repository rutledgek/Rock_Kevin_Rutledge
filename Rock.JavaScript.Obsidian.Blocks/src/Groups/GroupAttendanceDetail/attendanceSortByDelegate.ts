import { GroupAttendanceDetailRosterAttendeeBag } from "@Obsidian/ViewModels/Blocks/Groups/GroupAttendanceDetail/groupAttendanceDetailRosterAttendeeBag";

export const SortType = {
    FirstNameFirst: "FirstNameFirst",
    LastNameFirst: "LastNameFirst"
} as const;

export const SortTypeDescription: Record<string, string> = {
    FirstNameFirst: "FirstNameFirst",
    LastNameFirst: "LastNameFirst"
};

export type SortType = typeof SortType[keyof typeof SortType];

export type AttendanceSortByDelegate = (attendance1: GroupAttendanceDetailRosterAttendeeBag, attendance2: GroupAttendanceDetailRosterAttendeeBag) => number;

export function createSortBy(firstBy: AttendanceSortByDelegate, thenBy?: AttendanceSortByDelegate): AttendanceSortByDelegate {
    return (attendance1: GroupAttendanceDetailRosterAttendeeBag, attendance2: GroupAttendanceDetailRosterAttendeeBag): number => {
        const comparison = firstBy(attendance1, attendance2);

        // If attendance1 and attendance2 match, then run the additional `thenBy` comparison.
        if (comparison === 0 && thenBy) {
            return thenBy(attendance1, attendance2);
        }

        return comparison;
    };
}

export const byFirstName: AttendanceSortByDelegate = (attendance1: GroupAttendanceDetailRosterAttendeeBag, attendance2: GroupAttendanceDetailRosterAttendeeBag): number => {
    return compareStrings(attendance1.nickName, attendance2.nickName);
};

export const byLastName: AttendanceSortByDelegate = (attendance1: GroupAttendanceDetailRosterAttendeeBag, attendance2: GroupAttendanceDetailRosterAttendeeBag): number => {
    return compareStrings(attendance1.lastName, attendance2.lastName);
};

function compareStrings(str1: string | null | undefined, str2: string | null | undefined): number {
    return (str1 ?? "").localeCompare(str2 ?? "");
}
