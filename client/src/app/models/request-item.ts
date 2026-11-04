import { RequestPriority } from '../enums/request-priority';
import { RequestStatus } from '../enums/request-status';

export interface RequestItem {
  id: number;
  title: string;
  organizationName: string;
  status: RequestStatus;
  priority: RequestPriority;
  assignedTo: string | null;
  createdAt: string;
  updatedAt: string;
  rowVersion: string;
}
