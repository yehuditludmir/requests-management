import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { CurrentUserService } from '../services/current-user.service';

// מוסיף לכל בקשה לשרת את ה-Header X-User עם המשתמש הנוכחי
export const userInterceptor: HttpInterceptorFn = (req, next) => {
  const user = inject(CurrentUserService).currentUser();
  return next(req.clone({ setHeaders: { 'X-User': user } }));
};
