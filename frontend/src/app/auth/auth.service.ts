import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { catchError, map, Observable, of, tap } from 'rxjs';

/**
 * Interface representing current user.
 */
export interface CurrentUser { id: number; email: string; }

/**
 * Auth service for communicating.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly baseUrl = '/api/auth';
  readonly currentUser = signal<CurrentUser | null>(null);

  constructor(private readonly http: HttpClient) {}

  /**
   * Registers user with email and password.
   * @param email Email of a user.
   * @param password  Password of a user.
   * @returns Observable emitting the registered user.
   */
  register(email: string, password: string): Observable<CurrentUser> {
    return this.http.post<CurrentUser>(`${this.baseUrl}/register`, { email, password });
  }

    /**
   * Logins user with email and password.
   * @param email Email of a user.
   * @param password  Password of a user.
   * @returns Observable emitting the logged-in user result.
   */
  login(email: string, password: string): Observable<CurrentUser> {
    return this.http.post<CurrentUser>(`${this.baseUrl}/login`, { email, password }).pipe(
      tap(user => this.currentUser.set(user))
    );
  }

  /**
   * Loads current user information.
   * @returns Observable emitting the current user, or null if not authenticated.
   */
  loadMe(): Observable<CurrentUser | null> {
    return this.http.get<CurrentUser>(`${this.baseUrl}/me`).pipe(
      tap(user => this.currentUser.set(user)),
      catchError(() => {
        this.currentUser.set(null);
        return of(null);
      })
    );
  }

  /**
   * Logs out current user.
   * @returns Observable emitting void.
   */
  logout(): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/logout`, {}).pipe(
      tap(() => this.currentUser.set(null))
    );
  }
}
