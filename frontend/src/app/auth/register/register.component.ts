import { Component, inject, signal } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService, getAuthErrorMessage } from '../auth.service';

/**
 * Class component for user registration.
 */
@Component({
  imports: [FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  email = '';
  password = '';
  readonly error = signal('');
  readonly saving = signal(false);
  submitted = false;

  /**
   * Register the user with the entered email and password.
   */
  submit(form: NgForm): void {
    this.submitted = true;
    this.error.set('');

    if (form.invalid) {
      form.control.markAllAsTouched();
      this.error.set('Correct the highlighted fields.');
      return;
    }

    this.saving.set(true);
    this.auth.register(this.email.trim(), this.password).subscribe({
      next: () => this.router.navigateByUrl('/login'),
      error: error => {
        this.error.set(getAuthErrorMessage(
          error,
          'Registration failed. This email may already be registered.'
        ));
        this.saving.set(false);
      },
      complete: () => this.saving.set(false)
    });
  }
}
