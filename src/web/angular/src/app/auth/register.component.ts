import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from './auth.service';

@Component({
  imports: [FormsModule, RouterLink],
  template: `
    <section class="card">
      <h1>Create account</h1>
      <p>Use 12+ characters including upper/lowercase, a number, and a symbol.</p>
      <form (ngSubmit)="submit()">
        <label>Email <input name="email" type="email" [(ngModel)]="email" required></label>
        <label>Password <input name="password" type="password" [(ngModel)]="password" required minlength="12"></label>
        @if (error) { <p class="error">{{ error }}</p> }
        <button [disabled]="saving">Create account</button>
      </form>
      <p>Already registered? <a routerLink="/login">Sign in</a>.</p>
    </section>
  `,
  styles: `form { display:grid; gap:1rem; max-width:24rem; } label { display:grid; gap:.35rem; } input { padding:.5rem; }`
})
export class RegisterComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  email = '';
  password = '';
  error = '';
  saving = false;

  submit(): void {
    this.saving = true;
    this.error = '';
    this.auth.register(this.email, this.password).subscribe({
      next: () => this.router.navigateByUrl('/login'),
      error: () => { this.error = 'Registration failed. This email may already be used or the password is too weak.'; this.saving = false; },
      complete: () => this.saving = false
    });
  }
}
