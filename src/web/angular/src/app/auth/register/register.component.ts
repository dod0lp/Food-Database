import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../auth.service';

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
