import { ChangeDetectionStrategy, Component, Input, OnInit, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'cl-audit',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="cl-card">
      <h2>Audit log</h2>
      <table style="width:100%">
        <thead><tr><th>When</th><th>Action</th><th>Actor</th><th>Hash</th></tr></thead>
        <tbody>
          <tr *ngFor="let e of entries()">
            <td>{{ e.createdAt | date:'short' }}</td>
            <td><code>{{ e.actionType }}</code></td>
            <td><code>{{ e.actorId | slice:0:8 }}</code></td>
            <td><code style="font-size:0.7em">{{ e.hash | slice:0:16 }}…</code></td>
          </tr>
        </tbody>
      </table>
    </div>
  `,
})
export class AuditComponent implements OnInit {
  @Input() contractId?: string;
  private http = inject(HttpClient);
  entries = signal<any[]>([]);

  ngOnInit() {
    const url = this.contractId
      ? `/api/v1/audit-log?contractId=${this.contractId}`
      : `/api/v1/audit-log`;
    this.http.get<any[]>(url).subscribe({
      next: (r) => this.entries.set(r ?? []),
      error: () => this.entries.set([]),
    });
  }
}
