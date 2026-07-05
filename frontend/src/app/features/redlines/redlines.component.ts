import { ChangeDetectionStrategy, Component, Input, OnInit, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'cl-redlines',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="cl-card">
      <h2>Redlines</h2>
      <p *ngIf="loading()">Loading…</p>
      <div *ngFor="let r of redlines()" class="cl-card">
        <span class="cl-badge" [ngClass]="r.confidence === 'high' ? 'low' : r.confidence === 'low' ? 'high' : 'medium'">{{ r.confidence }}</span>
        <p><strong>Suggested:</strong> {{ r.suggestedText }}</p>
        <p><strong>Rationale:</strong> {{ r.rationale }}</p>
        <p><strong>Citations:</strong> <code>{{ r.citations }}</code></p>
        <p><strong>Status:</strong> {{ r.status }}</p>
      </div>
    </div>
  `,
})
export class RedlinesComponent implements OnInit {
  @Input() contractId!: string;
  private http = inject(HttpClient);
  redlines = signal<any[]>([]);
  loading = signal(true);

  ngOnInit() {
    this.http.get<any[]>(`/api/v1/contracts/${this.contractId}/redlines`).subscribe({
      next: (r) => { this.redlines.set(r ?? []); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }
}
