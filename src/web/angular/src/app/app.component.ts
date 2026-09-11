import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from './auth/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterLink, RouterOutlet],
  template: `
    <header>
      <a routerLink="/">Food Database</a>
      @if (auth.currentUser()) {
        <a routerLink="/favorites">Favorites</a>
        <a routerLink="/foods/new">Create food</a>
        <button (click)="logout()">Sign out</button>
      } @else {
        <a routerLink="/login">Sign in</a>
        <a routerLink="/register">Register</a>
      }
    </header>
    <main><router-outlet /></main>
  `,
  styles: `header { display:flex; gap:1rem; align-items:center; padding:1rem; background:#172b4d; color:white; } header a { color:white; text-decoration:none; font-weight:600; } header button { margin-left:auto; }`
})
export class AppComponent {
  readonly auth = inject(AuthService);

  constructor() {
    this.auth.loadMe().subscribe();
  }

  logout(): void { this.auth.logout().subscribe(); }
}
