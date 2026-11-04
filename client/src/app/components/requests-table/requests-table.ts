import { DatePipe } from '@angular/common';
import { Component, input, output, signal } from '@angular/core';
import { RequestSortField } from '../../enums/request-sort-field';
import { REQUEST_STATUSES, RequestStatus } from '../../enums/request-status';
import { SortDirection } from '../../enums/sort-direction';
import { RequestItem } from '../../models/request-item';

interface Column {
  label: string;
  field: RequestSortField | null;
}

@Component({
  selector: 'app-requests-table',
  imports: [DatePipe],
  templateUrl: './requests-table.html',
  styleUrl: './requests-table.css'
})
export class RequestsTable {
  readonly items = input.required<RequestItem[]>();
  readonly sortBy = input.required<RequestSortField>();
  readonly sortDirection = input.required<SortDirection>();

  readonly sortChange = output<RequestSortField>();
  readonly statusChange = output<RequestItem>();
  readonly historyRequested = output<RequestItem>();
  readonly bulkStatusChange = output<{ items: RequestItem[]; status: RequestStatus }>();

  readonly selected = signal<RequestItem[]>([]);

  readonly statuses = REQUEST_STATUSES;

  readonly columns: Column[] = [
    { label: 'Id', field: null },
    { label: 'Title', field: 'Title' },
    { label: 'Organization', field: 'OrganizationName' },
    { label: 'Status', field: 'Status' },
    { label: 'Priority', field: 'Priority' },
    { label: 'Assigned to', field: null },
    { label: 'Created', field: 'CreatedAt' },
    { label: 'Updated', field: 'UpdatedAt' },
    { label: 'History', field: null }
  ];

  // מחזיר את חץ המיון לכותרת העמודה: ▲ עולה, ▼ יורד, או ריק
  sortIndicator(field: RequestSortField): string {
    if (this.sortBy() !== field) {
      return '';
    }
    return this.sortDirection() === 'Asc' ? '▲' : '▼';
  }

  // מדווח על בחירת סטטוס חדש לפנייה, בלי לשנות את הפנייה המקורית
  onStatusSelected(item: RequestItem, newStatus: string): void {
    const updated: RequestItem = { ...item, status: newStatus as RequestStatus };
    this.statusChange.emit(updated);
  }

  isSelected(item: RequestItem): boolean {
    return this.selected().some(s => s.id === item.id);
  }

  toggle(item: RequestItem): void {
    if (this.isSelected(item)) {
      this.selected.set(this.selected().filter(s => s.id !== item.id));
    } else {
      this.selected.set([...this.selected(), item]);
    }
  }

  allSelected(): boolean {
    return this.items().length > 0 && this.selected().length === this.items().length;
  }

  toggleAll(): void {
    this.selected.set(this.allSelected() ? [] : [...this.items()]);
  }

  // מדווח על עדכון מרובה: הפניות שסומנו והסטטוס שנבחר
  applyBulk(status: string): void {
    this.bulkStatusChange.emit({ items: this.selected(), status: status as RequestStatus });
  }
}
