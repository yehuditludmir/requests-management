import { Component } from '@angular/core';
import { outputFromObservable } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { debounceTime, distinctUntilChanged, filter, map } from 'rxjs';
import { REQUEST_PRIORITIES, RequestPriority } from '../../enums/request-priority';
import { REQUEST_STATUSES, RequestStatus } from '../../enums/request-status';
import { RequestFilter } from '../../models/request-filter';

@Component({
  selector: 'app-requests-filters',
  imports: [ReactiveFormsModule],
  templateUrl: './requests-filters.html',
  styleUrl: './requests-filters.css'
})
export class RequestsFilters {
  readonly statuses = REQUEST_STATUSES;
  readonly priorities = REQUEST_PRIORITIES;

  readonly form = new FormGroup({
    search: new FormControl('', { nonNullable: true }),
    status: new FormControl<RequestStatus | ''>('', { nonNullable: true }),
    priority: new FormControl<RequestPriority | ''>('', { nonNullable: true }),
    organizationName: new FormControl('', { nonNullable: true }),
    assignedTo: new FormControl('', { nonNullable: true }),
    createdFrom: new FormControl('', { nonNullable: true }),
    createdTo: new FormControl('', { nonNullable: true })
  });

  // משדר את הפילטרים בכל שינוי בטופס: אחרי 300ms בלי הקלדה, רק אם הטווח תקין ורק אם משהו השתנה
  readonly filtersChange = outputFromObservable(
    this.form.valueChanges.pipe(
      debounceTime(300),
      map(() => this.toFilter()),
      filter(f => !f.createdFrom || !f.createdTo || f.createdFrom <= f.createdTo),
      distinctUntilChanged((a, b) => JSON.stringify(a) === JSON.stringify(b))
    )
  );

  get invalidDateRange(): boolean {
    const { createdFrom, createdTo } = this.form.getRawValue();
    return !!createdFrom && !!createdTo && createdFrom > createdTo;
  }

  clear(): void {
    this.form.reset();
  }

  // ממיר את ערכי הטופס לפילטר. שדה ריק הופך ל-undefined ולא נשלח לשרת
  private toFilter(): RequestFilter {
    const v = this.form.getRawValue();
    return {
      search: v.search.trim() || undefined,
      status: v.status || undefined,
      priority: v.priority || undefined,
      organizationName: v.organizationName.trim() || undefined,
      assignedTo: v.assignedTo.trim() || undefined,
      createdFrom: v.createdFrom || undefined,
      createdTo: v.createdTo || undefined
    };
  }
}
