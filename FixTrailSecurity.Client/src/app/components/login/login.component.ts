import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div style="max-width: 400px; margin: 100px auto; padding: 20px; border: 1px solid #ccc; border-radius: 5px; font-family: sans-serif;">
      <h2 style="text-align: center;">FixTrail Security Login</h2>
      <div style="display: flex; flex-direction: column; gap: 15px; margin-top: 20px;">
        <input [(ngModel)]="credentials.email" placeholder="Email" type="email" style="padding: 10px;" />
        <input [(ngModel)]="credentials.password" placeholder="Password" type="password" style="padding: 10px;" />
        <button (click)="onLogin()" style="padding: 10px; background: #0056b3; color: white; border: none; cursor: pointer;">Login</button>
        <p *ngIf="errorMessage" style="color: red; text-align: center;">{{ errorMessage }}</p>
      </div>
    </div>
  `
})
export class LoginComponent {
  authService = inject(AuthService);
  router = inject(Router);
  credentials = { email: '', password: '' };
  errorMessage = '';
  onLogin() {
    this.authService.login(this.credentials).subscribe({
      next: () => this.router.navigate(['/assets']),
      error: (err) => this.errorMessage = 'Invalid email or password.'
    });
  }
}
