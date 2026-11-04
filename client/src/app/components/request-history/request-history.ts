import { DatePipe } from '@angular/common';
import { Component, input, output } from '@angular/core';
import { RequestItem } from '../../models/request-item';
import { RequestStatusChange } from '../../models/request-status-change';

export type HistoryState =
  | { kind: 'loading' }
  | { kind: 'error' }
  | { kind: 'loaded'; changes: RequestStatusChange[] };

@Component({
  selector: 'app-request-history',
  imports: [DatePipe],
  templateUrl: './request-history.html',
  styleUrl: './request-history.css'
})
export class RequestHistory {
  readonly item = input.required<RequestItem>();
  readonly state = input.required<HistoryState>();

  readonly closed = output<void>();
}
