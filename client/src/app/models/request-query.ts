import { RequestSortField } from '../enums/request-sort-field';
import { SortDirection } from '../enums/sort-direction';
import { RequestFilter } from './request-filter';

export interface RequestQuery extends RequestFilter {
  page: number;
  pageSize: number;
  sortBy: RequestSortField;
  sortDirection: SortDirection;
}
