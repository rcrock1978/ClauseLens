import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'cl-signup',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="cl-card" style="max-width:480px;margin:4rem auto;">
      <h2>Create your tenant</h2>
      <form [formGroup]="form" (ngSubmit)="submit()">
        <label>Organization name
          <input formControlName="tenantName" placeholder="Acme Legal" />
        </label>
        <label>Admin email
          <input type="email" formControlName="adminEmail" placeholder="[email protected]" />
        </label>
        <label>Password (min 12)
          <input type="password" formControlName="password" />
        </label>
        <button type="submit" [disabled]="form.invalid || busy">Create tenant</button>
        <p *ngIf="error" style="color: var(--cl-danger)">{{ error }}</p>
      </form>
      <p>Already verified? <a routerLink="/auth/verify">Verify email</a></p>
    </div>
  `,
})
export class SignupComponent {
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);
  private router = inject(Router);

  busy = false;
  error: string | null = null;

  form = this.fb.nonNullable.group({
    tenantName: ['', [Validators.required, Validators.maxLength(200)]],
    adminEmail: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(12)]],
  });

  submit() {
    if (this.form.invalid) return;
    this.busy = true; this.error = null;
    this.http.post('/api/v1/tenants/signup', this.form.getRawValue()).subscribe({
      next: () => { this.busy = false; this.router.navigate(['/auth/verify']); },
      error: (e) => { this.busy = false; this.error = e?.error?.error ?? 'Signup failed'; },
    });
  }
}
