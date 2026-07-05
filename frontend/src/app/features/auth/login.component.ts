import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'cl-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="cl-card" style="max-width:480px;margin:4rem auto;">
      <h2>Sign in</h2>
      <form [formGroup]="form" (ngSubmit)="submit()">
        <label>Email
          <input type="email" formControlName="email" />
        </label>
        <label>Password
          <input type="password" formControlName="password" />
        </label>
        <button type="submit" [disabled]="form.invalid || busy">Sign in</button>
        <p *ngIf="error" style="color: var(--cl-danger)">{{ error }}</p>
      </form>
      <p>No account? <a routerLink="/auth/signup">Create a tenant</a></p>
    </div>
  `,
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);
  private router = inject(Router);
  busy = false;
  error: string | null = null;

  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  submit() {
    if (this.form.invalid) return;
    this.busy = true; this.error = null;
    this.http.post('/api/v1/auth/login', this.form.getRawValue()).subscribe({
      next: (r: any) => {
        localStorage.setItem('cl_token', r.accessToken);
        this.busy = false;
        this.router.navigate(['/contracts']);
      },
      error: (e) => { this.busy = false; this.error = e?.error?.error ?? 'Sign in failed'; },
    });
  }
}
