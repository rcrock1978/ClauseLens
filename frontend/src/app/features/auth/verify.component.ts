import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'cl-verify',
  standalone: true,
  imports: [ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="cl-card" style="max-width:480px;margin:4rem auto;">
      <h2>Verify your email</h2>
      <p>Enter the token from the verification email.</p>
      <form [formGroup]="form" (ngSubmit)="submit()">
        <input formControlName="token" placeholder="verification token" />
        <button type="submit" [disabled]="form.invalid || busy">Verify</button>
        <p *ngIf="result" style="color: var(--cl-ok)">{{ result }}</p>
        <p *ngIf="error" style="color: var(--cl-danger)">{{ error }}</p>
      </form>
    </div>
  `,
})
export class VerifyComponent {
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);
  busy = false; result: string | null = null; error: string | null = null;

  form = this.fb.nonNullable.group({ token: ['', Validators.required] });

  submit() {
    if (this.form.invalid) return;
    this.busy = true; this.result = null; this.error = null;
    this.http.post('/api/v1/auth/verify-email', this.form.getRawValue()).subscribe({
      next: (r: any) => { this.busy = false; this.result = `Verified as ${r.role}`; },
      error: (e) => { this.busy = false; this.error = e?.error?.error ?? 'Verification failed'; },
    });
  }
}
