import { Component, inject } from '@angular/core';
import { RequestsPage } from './components/requests-page/requests-page';
import { CurrentUserService } from './services/current-user.service';

@Component({
  selector: 'app-root',
  imports: [RequestsPage],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  readonly userService = inject(CurrentUserService);
}
