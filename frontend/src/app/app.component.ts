import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from './auth/auth.service';

/**
 * Component for loading the main application, and user information.
 */
@Component({
  selector: 'app-root',
  imports: [RouterLink, RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  readonly auth = inject(AuthService);

  constructor() {
    this.auth.loadMe().subscribe();
  }

  /**
   * Logs out current user.
   */
  logout(): void { this.auth.logout().subscribe(); }
}
