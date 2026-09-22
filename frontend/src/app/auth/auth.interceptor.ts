import { HttpInterceptorFn } from '@angular/common/http';

// Authentication is an HttpOnly same-site cookie, so no token is exposed to JS.
export const authInterceptor: HttpInterceptorFn = (request, next) =>
  next(request.clone({ withCredentials: true }));
