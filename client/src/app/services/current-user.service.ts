import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class CurrentUserService {
  readonly users = ['dana', 'yossi', 'michal', 'avi', 'noa', 'eitan', 'shira', 'omer', 'tamar', 'ron'];

  readonly currentUser = signal(this.users[0]);
}
