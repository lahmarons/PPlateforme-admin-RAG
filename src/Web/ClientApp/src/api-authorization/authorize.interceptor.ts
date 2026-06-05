import { Inject, Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse, HttpResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthorizeInterceptor implements HttpInterceptor {
  loginUrl: string;

  constructor(@Inject('BASE_URL') baseUrl: string) {
    this.loginUrl = `${baseUrl}connexion`;
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const isAuthRequest = req.url.includes('/api/auth/');

    return next.handle(req).pipe(
      catchError(error => {
        if (!isAuthRequest && error instanceof HttpErrorResponse && error.status === 401) {
          window.location.href = `${this.loginUrl}?returnUrl=${window.location.pathname}`;
        }
        return throwError(() => error);
      }),
      map((event: HttpEvent<any>) => {
        if (!isAuthRequest && event instanceof HttpResponse && event.status === 401) {
          window.location.href = `${this.loginUrl}?returnUrl=${window.location.pathname}`;
        }
        return event;
      }));
  }
}
