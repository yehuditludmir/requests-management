import { RequestStatus } from '../enums/request-status';

export interface RequestStatusChange {
  previousStatus: RequestStatus;
  newStatus: RequestStatus;
  changedAt: string;
  changedBy: string;
}
