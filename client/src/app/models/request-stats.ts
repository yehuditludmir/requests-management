import { RequestPriority } from '../enums/request-priority';
import { RequestStatus } from '../enums/request-status';

export interface RequestStats {
  byStatus: Record<RequestStatus, number>;
  byPriority: Record<RequestPriority, number>;
}
