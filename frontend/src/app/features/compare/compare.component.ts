import { ChangeDetectionStrategy, Component, Input, OnInit, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'cl-compare',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="cl-card">
      <h2>Side-by-side comparison</h2>
      <pre *ngIf="comparison() as c">{{ c.clauseText }}</pre>
    </div>
  `,
})
export class CompareComponent implements OnInit {
  @Input() contractId!: string;
  @Input() clauseId!: string;
  private http = inject(HttpClient);
  comparison = signal<any | null>(null);

  ngOnInit() {
    this.http.get(`/api/v1/contracts/${this.contractId}/clauses/${this.clauseId}/comparison`).subscribe({
      next: (r) => this.comparison.set(r),
      error: () => this.comparison.set(null),
    });
  }
}
