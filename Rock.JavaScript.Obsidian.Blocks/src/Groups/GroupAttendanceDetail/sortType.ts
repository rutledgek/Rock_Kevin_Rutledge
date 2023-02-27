export const SortType = {
    FirstNameFirst: "FirstNameFirst",
    LastNameFirst: "LastNameFirst"
} as const;

export const SortTypeDescription: Record<string, string> = {
    FirstNameFirst: "FirstNameFirst",

    LastNameFirst: "LastNameFirst"
};

export type SortType = typeof SortType[keyof typeof SortType];
