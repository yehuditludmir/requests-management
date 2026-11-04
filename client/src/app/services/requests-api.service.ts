import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { RequestStatus } from '../enums/request-status';
import { BulkUpdateResult } from '../models/bulk-update-result';
import { PagedResult } from '../models/paged-result';
import { RequestFilter } from '../models/request-filter';
import { RequestItem } from '../models/request-item';
import { RequestQuery } from '../models/request-query';
import { RequestStats } from '../models/request-stats';
import { RequestStatusChange } from '../models/request-status-change';

@Injectable({ providedIn: 'root' })
export class RequestsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/requests';

  getRequests(query: RequestQuery): Observable<PagedResult<RequestItem>> {
    return this.http.get<PagedResult<RequestItem>>(this.baseUrl, { params: this.toParams(query) });
  }

  getStats(filter: RequestFilter): Observable<RequestStats> {
    return this.http.get<RequestStats>(`${this.baseUrl}/stats`, { params: this.toParams(filter) });
  }

  // מעדכן סטטוס של פנייה אחת, עם הגרסה שהמשתמש ראה (לזיהוי התנגשויות)
  updateStatus(id: number, status: RequestStatus, rowVersion: string): Observable<RequestItem> {
    return this.http.patch<RequestItem>(`${this.baseUrl}/${id}/status`, { status, rowVersion });
  }

  // מעדכן סטטוס של כמה פניות בבקשה אחת. התשובה כוללת אילו הצליחו ואילו נכשלו
  bulkUpdateStatus(status: RequestStatus, items: RequestItem[]): Observable<BulkUpdateResult> {
    const body = { status, items: items.map(i => ({ id: i.id, rowVersion: i.rowVersion })) };
    return this.http.patch<BulkUpdateResult>(`${this.baseUrl}/status`, body);
  }

  getHistory(id: number): Observable<RequestStatusChange[]> {
    return this.http.get<RequestStatusChange[]>(`${this.baseUrl}/${id}/history`);
  }

  // ממיר אובייקט לפרמטרים ב-URL, ומדלג על שדות ריקים
  private toParams(query: object): HttpParams {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(query)) {
      if (value !== undefined && value !== null && value !== '') {
        params = params.set(key, String(value));
      }
    }
    return params;
  }
}
