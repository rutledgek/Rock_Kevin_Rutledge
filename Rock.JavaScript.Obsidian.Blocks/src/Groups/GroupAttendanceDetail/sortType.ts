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

export type AttendeeComparer = (attendee1: GroupAttendanceDetailRosterAttendeeBag, attendee2: GroupAttendanceDetailRosterAttendeeBag) => number;

export function compare(comparer: AttendeeComparer, thenBy?: AttendeeComparer): AttendeeComparer {
    return (m1: GroupAttendanceDetailRosterAttendeeBag, m2: GroupAttendanceDetailRosterAttendeeBag): number => {
        const comparison = comparer(m1, m2);

        if (comparison === 0 && thenBy) {
            return thenBy(m1, m2);
        }

        return comparison;
    };
}

export const byFirstName: AttendeeComparer = (member1: GroupAttendanceDetailRosterAttendeeBag, member2: GroupAttendanceDetailRosterAttendeeBag): number => {
    return compareStrings(member1.nickName, member2.nickName);
};

export const byLastName: AttendeeComparer = (member1: GroupAttendanceDetailRosterAttendeeBag, member2: GroupAttendanceDetailRosterAttendeeBag): number => {
    return compareStrings(member1.lastName, member2.lastName);
};

function compareStrings(str1: string | null | undefined, str2: string | null | undefined): number {
    return (str1 ?? "").localeCompare(str2 ?? "");
}
