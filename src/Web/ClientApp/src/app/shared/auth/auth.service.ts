import { Injectable, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

export interface AuthResult {
  userId: string;
  userName: string;
  roles: string[];
  redirectUrl: string;
  citoyenId?: number | null;
}

export interface RegisterCitoyenRequest {
  email: string;
  password: string;
  nom: string;
  adresse: string;
  telephone: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly _currentUser = signal<AuthResult | null>(null);
  readonly currentUser = this._currentUser.asReadonly();

  constructor(private readonly http: HttpClient) {}

  login(email: string, password: string): Observable<AuthResult> {
    return this.http.post<AuthResult>('/api/auth/login', { email, password }, { withCredentials: true }).pipe(
      tap((user) => this._currentUser.set(user))
    );
  }

  registerCitoyen(request: RegisterCitoyenRequest): Observable<AuthResult> {
    return this.http.post<AuthResult>('/api/auth/register/citoyen', request, { withCredentials: true }).pipe(
      tap((user) => this._currentUser.set(user))
    );
  }

  me(): Observable<AuthResult | null> {
    return this.http.get<AuthResult | null>('/api/auth/me', { withCredentials: true }).pipe(
      tap((user) => this._currentUser.set(user))
    );
  }

  logout(): Observable<void> {
    return this.http.post<void>('/api/auth/logout', {}, { withCredentials: true }).pipe(
      tap(() => this._currentUser.set(null))
    );
  }

  getErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse) {
      const body = error.error;

      if (typeof body === 'string' && body.trim()) {
        return body;
      }

      if (body?.title && typeof body.title === 'string') {
        return body.title;
      }

      if (body?.errors && typeof body.errors === 'object') {
        const values = Object.values(body.errors).flat().filter((value): value is string => typeof value === 'string');
        if (values.length > 0) {
          return values.join(' ');
        }
      }

      if (error.message) {
        return error.message;
      }
    }

    return fallback;
  }
}
