import { ChangeDetectionStrategy, Component, Input, OnInit, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';

interface DecisionItem {
  clauseId: string;
  status: string;
  decision?: string;
}

@Component({
  selector: 'cl-review-decide',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="cl-card">
      <h2>Review decisions</h2>
      <p *ngIf="clauses().length === 0">No clauses to decide on.</p>
      <div *ngFor="let c of clauses()" class="cl-card">
        <p><strong>Clause {{ c.clauseId | slice:0:8 }}</strong> <span class="cl-badge" [ngClass]="statusClass(c.status)">{{ c.status }}</span></p>
        <button (click)="decide(c.clauseId, 'Approved')" [disabled]="busy()">Approve</button>
        <button (click)="decide(c.clauseId, 'RejectedWithComment')" [disabled]="busy()">Reject</button>
        <button (click)="decide(c.clauseId, 'NeedsDiscussion')" [disabled]="busy()">Needs Discussion</button>
        <button (click)="submit(c.clauseId, 'Reassign')" [disabled]="busy()">Reassign (SLA expired)</button>
      </div>
    </div>
  `,
})
export class ReviewDecideComponent implements OnInit {
  @Input() contractId!: string;
  private http = inject(HttpClient);
  clauses = signal<DecisionItem[]>([]);
  busy = signal(false);

  ngOnInit() {
    // Pulls the contract detail (which carries the clause list and statuses)
    this.http.get<any>(`/api/v1/contracts/${this.contractId}`).subscribe({
      next: (c) => this.clauses.set((c?.clauses ?? []).map((cl: any) => ({ clauseId: cl.id, status: cl.status }))),
      error: () => this.clauses.set([]),
    });
  }

  statusClass(s: string) {
    return ({
      Flagged: 'flagged', Approved: 'approved', Rejected: 'rejected',
      NeedsDiscussion: 'needs', Compliant: 'compliant',
    } as Record<string, string>)[s] ?? 'unreviewed';
  }

  decide(clauseId: string, decision: string) {
    this.busy.set(true);
    this.http.put(`/api/v1/contracts/${this.contractId}/clauses/${clauseId}/decision`, { decision, comment: null }).subscribe({
      next: () => this.busy.set(false),
      error: () => this.busy.set(false),
    });
  }

  submit(clauseId: string, _action: string) {
    // The "Reassign" button posts to the reassign endpoint; the SLA check is
    // server-side.
    this.busy.set(true);
    this.http.post(`/api/v1/contracts/${this.contractId}/review/reassign`, {
      newPrimaryReviewerId: prompt('New primary reviewer user id') ?? '',
      reason: prompt('Reason for reassignment') ?? '',
    }).subscribe({ next: () => this.busy.set(false), error: () => this.busy.set(false) });
  }
}
