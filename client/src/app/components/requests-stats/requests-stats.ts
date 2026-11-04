import { DecimalPipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { REQUEST_PRIORITIES } from '../../enums/request-priority';
import { REQUEST_STATUSES } from '../../enums/request-status';
import { RequestStats } from '../../models/request-stats';

export type StatsState =
  | { kind: 'loading' }
  | { kind: 'error' }
  | { kind: 'loaded'; stats: RequestStats };

@Component({
  selector: 'app-requests-stats',
  imports: [DecimalPipe],
  templateUrl: './requests-stats.html',
  styleUrl: './requests-stats.css'
})
export class RequestsStats {
  readonly state = input.required<StatsState>();

  readonly statuses = REQUEST_STATUSES;
  readonly priorities = REQUEST_PRIORITIES;
}
