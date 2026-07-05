import { ChangeDetectionStrategy, Component, Input, OnInit, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'cl-contract-detail',
  standalone: true,
  imports: [CommonModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="cl-card" *ngIf="contract; else loading">
      <h2>{{ contract.fileName }}</h2>
      <p>Status: <span class="cl-badge" [ngClass]="statusClass()">{{ contract.status }}</span></p>
      <p>Size: {{ contract.fileSize }} bytes · Format: {{ contract.fileFormat }}</p>
      <div class="cl-row">
        <a [routerLink]="['/contracts', id, 'review', 'assign']"><button>Assign reviewer</button></a>
        <a [routerLink]="['/contracts', id, 'review', 'decide']"><button>Make decisions</button></a>
      </div>
      <h3>Clauses ({{ contract.clauses.length }})</h3>
      <ol>
        <li *ngFor="let cl of contract.clauses" style="margin-bottom:1rem">
          <strong>{{ cl.heading || 'Clause ' + cl.index }}</strong>
          <span class="cl-badge" [ngClass]="clauseClass(cl.status)">{{ cl.status }}</span>
          <p>{{ cl.text }}</p>
          <p *ngIf="cl.systemNote" style="color: var(--cl-warn); font-style: italic">{{ cl.systemNote }}</p>
        </li>
      </ol>
    </div>
    <ng-template #loading><div class="cl-card">Loading…</div></ng-template>
  `,
})
export class ContractDetailComponent implements OnInit {
  @Input() id!: string;
  private http = inject(HttpClient);
  contract: any;

  ngOnInit() {
    this.http.get(`/api/v1/contracts/${this.id}`).subscribe((c) => (this.contract = c));
  }

  statusClass() {
    const s = (this.contract?.status ?? '').toLowerCase();
    return s === 'readyforreview' ? 'compliant' : s === 'inreview' ? 'needs' : 'unreviewed';
  }
  clauseClass(status: string) {
    return ({
      Flagged: 'flagged', Approved: 'approved', Rejected: 'rejected',
      NeedsDiscussion: 'needs', Compliant: 'compliant',
    } as Record<string, string>)[status] ?? 'unreviewed';
  }
}
