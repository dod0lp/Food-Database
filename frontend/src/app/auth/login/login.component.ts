import { Component, inject, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService, getAuthErrorMessage } from '../auth.service';

/**
 * Component for user login.
 */
@Component({
  imports: [FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  email = '';
  password = '';
  readonly error = signal('');
  readonly saving = signal(false);
  submitted = false;

  /**
   * Submit login.
   */
  submit(form: NgForm): void {
    this.submitted = true;
    this.error.set('');

    if (form.invalid) {
      form.control.markAllAsTouched();
      this.error.set('Enter a valid email address and password.');
      return;
    }

    this.saving.set(true);
    this.auth.login(this.email.trim(), this.password).subscribe({
      next: () => this.router.navigateByUrl('/'),
      error: error => {
        this.error.set(getAuthErrorMessage(
          error,
          'Sign-in failed. Check your email and password.'
        ));
        this.saving.set(false);
      },
      complete: () => this.saving.set(false)
    });
  }
}
