import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { catchError, map, Observable, of, tap } from 'rxjs';

export interface CurrentUser { id: number; email: string; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = '/api/auth';
  readonly currentUser = signal<CurrentUser | null>(null);

  constructor(private readonly http: HttpClient) {}

  register(email: string, password: string): Observable<CurrentUser> {
    return this.http.post<CurrentUser>(`${this.baseUrl}/register`, { email, password });
  }

  login(email: string, password: string): Observable<CurrentUser> {
    return this.http.post<CurrentUser>(`${this.baseUrl}/login`, { email, password }).pipe(
      tap(user => this.currentUser.set(user))
    );
  }

  loadMe(): Observable<CurrentUser | null> {
    return this.http.get<CurrentUser>(`${this.baseUrl}/me`).pipe(
      tap(user => this.currentUser.set(user)),
      catchError(() => {
        this.currentUser.set(null);
        return of(null);
      })
    );
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/logout`, {}).pipe(
      tap(() => this.currentUser.set(null))
    );
  }
}
