import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from './auth.service';

@Component({
  imports: [FormsModule, RouterLink],
  template: `
    <section class="card">
      <h1>Sign in</h1>
      <form (ngSubmit)="submit()">
        <label>Email <input name="email" type="email" [(ngModel)]="email" required></label>
        <label>Password <input name="password" type="password" [(ngModel)]="password" required></label>
        @if (error) { <p class="error">{{ error }}</p> }
        <button [disabled]="saving">Sign in</button>
      </form>
      <p>New here? <a routerLink="/register">Create an account</a>.</p>
    </section>
  `,
  styles: `form { display:grid; gap:1rem; max-width:24rem; } label { display:grid; gap:.35rem; } input { padding:.5rem; }`
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  email = '';
  password = '';
  error = '';
  saving = false;

  submit(): void {
    this.saving = true;
    this.error = '';
    this.auth.login(this.email, this.password).subscribe({
      next: () => this.router.navigateByUrl('/'),
      error: () => { this.error = 'Sign-in failed. Check your email and password.'; this.saving = false; },
      complete: () => this.saving = false
    });
  }
}
