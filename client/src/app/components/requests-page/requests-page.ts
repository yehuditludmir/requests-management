import { AsyncPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { BehaviorSubject, Observable, Subject, catchError, map, of, startWith, switchMap } from 'rxjs';
import { RequestSortField } from '../../enums/request-sort-field';
import { RequestStatus } from '../../enums/request-status';
import { SortDirection } from '../../enums/sort-direction';
import { PagedResult } from '../../models/paged-result';
import { RequestFilter } from '../../models/request-filter';
import { RequestItem } from '../../models/request-item';
import { RequestQuery } from '../../models/request-query';
import { RequestsApiService } from '../../services/requests-api.service';
import { HistoryState, RequestHistory } from '../request-history/request-history';
import { RequestsFilters } from '../requests-filters/requests-filters';
import { RequestsStats, StatsState } from '../requests-stats/requests-stats';
import { RequestsTable } from '../requests-table/requests-table';

type ViewState =
  | { kind: 'loading' }
  | { kind: 'error' }
  | { kind: 'loaded'; result: PagedResult<RequestItem>; query: RequestQuery };

const LOADING: ViewState = { kind: 'loading' };
const ERROR: ViewState = { kind: 'error' };

@Component({
  selector: 'app-requests-page',
  imports: [AsyncPipe, RequestsFilters, RequestsTable, RequestHistory, RequestsStats],
  templateUrl: './requests-page.html',
  styleUrl: './requests-page.css'
})
export class RequestsPage {
  private readonly api = inject(RequestsApiService);

  readonly notice = signal<{ isError: boolean; text: string } | null>(null);

  readonly historyItem = signal<RequestItem | null>(null);

  private readonly historyRequest$ = new Subject<number>();

  private readonly query$ = new BehaviorSubject<RequestQuery>({
    page: 1,
    pageSize: 20,
    sortBy: 'CreatedAt',
    sortDirection: 'Desc'
  });

  // טוען את עמוד הפניות בכל שינוי בפרמטרים, ומחזיר את מצב המסך: טעינה, שגיאה או תוצאה
  readonly state$: Observable<ViewState> = this.query$.pipe(
    switchMap(query =>
      this.api.getRequests(query).pipe(
        map((result): ViewState => ({ kind: 'loaded', result, query })),
        catchError(() => of(ERROR)),
        startWith(LOADING)
      )
    )
  );

  // טוען את הסיכומים לפי הפילטרים הנוכחיים, ומחזיר את מצב הטעינה שלהם
  readonly stats$: Observable<StatsState> = this.query$.pipe(
    map(({ page, pageSize, sortBy, sortDirection, ...filter }): RequestFilter => filter),
    switchMap(filter =>
      this.api.getStats(filter).pipe(
        map((stats): StatsState => ({ kind: 'loaded', stats })),
        catchError(() => of<StatsState>({ kind: 'error' })),
        startWith<StatsState>({ kind: 'loading' })
      )
    )
  );

  // טוען את ההיסטוריה של הפנייה שנבחרה. בחירה בפנייה אחרת מבטלת את הבקשה הקודמת
  readonly historyState = toSignal(
    this.historyRequest$.pipe(
      switchMap(id =>
        this.api.getHistory(id).pipe(
          map((changes): HistoryState => ({ kind: 'loaded', changes })),
          catchError(() => of<HistoryState>({ kind: 'error' })),
          startWith<HistoryState>({ kind: 'loading' })
        )
      )
    ),
    { initialValue: { kind: 'loading' } as HistoryState }
  );

  // מחיל פילטרים חדשים וחוזר לעמוד הראשון. גודל העמוד והמיון נשמרים
  applyFilters(filter: RequestFilter): void {
    const { pageSize, sortBy, sortDirection } = this.query$.value;
    this.query$.next({ ...filter, page: 1, pageSize, sortBy, sortDirection });
  }

  // ממיין לפי העמודה שנבחרה. לחיצה חוזרת על אותה עמודה הופכת את הכיוון
  changeSort(field: RequestSortField): void {
    const current = this.query$.value;
    const sortDirection: SortDirection =
      current.sortBy === field && current.sortDirection === 'Asc' ? 'Desc' : 'Asc';
    this.query$.next({ ...current, sortBy: field, sortDirection, page: 1 });
  }

  goToPage(page: number): void {
    this.query$.next({ ...this.query$.value, page });
  }

  reload(): void {
    this.query$.next({ ...this.query$.value });
  }

  // מעדכן סטטוס של פנייה, מציג הודעת הצלחה או שגיאה, וטוען מחדש את הנתונים
  updateStatus(item: RequestItem): void {
    this.api.updateStatus(item.id, item.status, item.rowVersion).subscribe({
      next: () => {
        this.notice.set({ isError: false, text: `Request ${item.id} changed to ${item.status}.` });
        this.reload();
        if (this.historyItem()?.id === item.id) {
          this.showHistory(item);
        }
      },
      error: (error: HttpErrorResponse) => {
        this.notice.set({ isError: true, text: this.errorMessage(error) });
        this.reload();
      }
    });
  }

  // מעדכן סטטוס של כמה פניות, ומציג כמה הצליחו וכמה נכשלו ולמה
  updateStatusBulk(change: { items: RequestItem[]; status: RequestStatus }): void {
    this.api.bulkUpdateStatus(change.status, change.items).subscribe({
      next: result => {
        const failures = result.failed.map(f => `#${f.id}: ${f.error}`).join(' ');
        this.notice.set({
          isError: result.failed.length > 0,
          text: `${result.succeeded.length} updated, ${result.failed.length} failed. ${failures}`
        });
        this.reload();
        const open = this.historyItem();
        if (open && change.items.some(i => i.id === open.id)) {
          this.showHistory(open);
        }
      },
      error: () => {
        this.notice.set({ isError: true, text: 'Bulk update failed. Please try again.' });
        this.reload();
      }
    });
  }

  // פותח את פאנל ההיסטוריה של הפנייה וטוען אותה
  showHistory(item: RequestItem): void {
    this.historyItem.set(item);
    this.historyRequest$.next(item.id);
  }

  closeHistory(): void {
    this.historyItem.set(null);
  }

  // מתרגם שגיאת עדכון מהשרת להודעה למשתמש
  private errorMessage(error: HttpErrorResponse): string {
    if (error.status === 409) {
      return 'Someone else updated this request. The list was refreshed, please try again.';
    }
    if (error.status === 400) {
      return error.error?.errors?.Status?.[0] ?? 'The update is not valid.';
    }
    return 'Failed to update the request. Please try again.';
  }
}
