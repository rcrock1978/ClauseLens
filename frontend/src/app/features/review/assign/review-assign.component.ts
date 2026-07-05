import { ChangeDetectionStrategy, Component, Input, OnInit, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'cl-review-assign',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="cl-card">
      <h2>Assign review</h2>
      <form [formGroup]="form" (ngSubmit)="submit()">
        <label>Primary reviewer (required)
          <input formControlName="primaryReviewerId" placeholder="user-id (uuid)" />
        </label>
        <label>Secondary reviewers (optional, max 2)
          <input formControlName="secondaries" placeholder="uuid,uuid" />
        </label>
        <button type="submit" [disabled]="form.invalid || busy">Assign</button>
        <p *ngIf="result()">{{ result() }}</p>
      </form>
    </div>
  `,
})
export class ReviewAssignComponent implements OnInit {
  @Input() contractId!: string;
  private http = inject(HttpClient);
  private fb = inject(FormBuilder);
  private router = inject(Router);
  busy = false;
  result = signal<string | null>(null);

  form = this.fb.nonNullable.group({
    primaryReviewerId: ['', Validators.required],
    secondaries: [''],
  });

  ngOnInit() {}

  submit() {
    if (this.form.invalid) return;
    this.busy = true; this.result.set(null);
    const v = this.form.getRawValue();
    const body = {
      primaryReviewerId: v.primaryReviewerId,
      secondaryReviewerIds: v.secondaries.split(',').map(s => s.trim()).filter(Boolean),
    };
    this.http.post(`/api/v1/contracts/${this.contractId}/review`, body).subscribe({
      next: (r: any) => { this.busy = false; this.result.set(`Assigned. SLA: ${r.slaDeadline}`); },
      error: (e) => { this.busy = false; this.result.set(`Failed: ${e?.error?.error ?? 'unknown'}`); },
    });
  }
}
