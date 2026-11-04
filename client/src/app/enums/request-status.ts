export const REQUEST_STATUSES = ['New', 'InProgress', 'Waiting', 'Completed'] as const;

export type RequestStatus = (typeof REQUEST_STATUSES)[number];
