import { RequestPriority } from '../enums/request-priority';
import { RequestStatus } from '../enums/request-status';

export interface RequestFilter {
  status?: RequestStatus;
  priority?: RequestPriority;
  organizationName?: string;
  assignedTo?: string;
  createdFrom?: string;
  createdTo?: string;
  search?: string;
}
