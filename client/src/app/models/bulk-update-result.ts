import { RequestItem } from './request-item';

export interface BulkUpdateResult {
  succeeded: RequestItem[];
  failed: { id: number; reason: string; error: string }[];
}
