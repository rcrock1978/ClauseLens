import { ChangeDetectionStrategy, Component, Input, OnInit, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'cl-risks',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="cl-card">
      <h2>Risk flags</h2>
      <p *ngIf="loading()">Loading…</p>
      <table *ngIf="!loading() && risks().length > 0" style="width:100%; border-collapse: collapse">
        <thead>
          <tr>
            <th>Rule (clause type / condition)</th>
            <th>Severity</th>
            <th>Confidence</th>
            <th>Standard language</th>
            <th>Rationale</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let r of risks()">
            <td>
              <strong>{{ r.ruleClauseType }}</strong><br />
              <em>{{ r.ruleCondition }}</em>
            </td>
            <td><span class="cl-badge" [ngClass]="severityClass(r.severity)">{{ r.severity }}</span></td>
            <td><span class="cl-badge" [ngClass]="confidenceClass(r.confidence)">{{ r.confidence }}</span></td>
            <td><code>{{ r.ruleStandardLanguage }}</code></td>
            <td>{{ r.rationale }}</td>
          </tr>
        </tbody>
      </table>
      <p *ngIf="!loading() && risks().length === 0">No risk flags for this contract.</p>
    </div>
  `,
})
export class RisksComponent implements OnInit {
  @Input() contractId!: string;
  private http = inject(HttpClient);
  risks = signal<any[]>([]);
  loading = signal(true);

  ngOnInit() {
    this.http.get<any[]>(`/api/v1/contracts/${this.contractId}/risks`).subscribe({
      next: (r) => { this.risks.set(r ?? []); this.loading.set(false); },
      error: () => this.loading.set(false),
    });
  }

  severityClass(s: number) { return ({ 3: 'high', 2: 'high', 1: 'medium', 0: 'low' } as any)[s] ?? 'low'; }
  confidenceClass(c: string) { return c === 'high' ? 'low' : c === 'low' ? 'high' : 'medium'; }
}
