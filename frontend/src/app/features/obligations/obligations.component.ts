import { ChangeDetectionStrategy, Component, Input, OnInit, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'cl-obligations',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="cl-card">
      <h2>Obligations</h2>
      <ul>
        <li *ngFor="let o of obligations()">
          <strong>{{ o.responsibleParty }}:</strong> {{ o.description }}
          <em *ngIf="o.dueDate"> — due {{ o.dueDate | date }}</em>
          <em *ngIf="o.triggerCondition"> — triggered by {{ o.triggerCondition }}</em>
        </li>
      </ul>
    </div>
  `,
})
export class ObligationsComponent implements OnInit {
  @Input() contractId!: string;
  private http = inject(HttpClient);
  obligations = signal<any[]>([]);

  ngOnInit() {
    this.http.get<any[]>(`/api/v1/contracts/${this.contractId}/obligations`).subscribe({
      next: (r) => this.obligations.set(r ?? []),
      error: () => this.obligations.set([]),
    });
  }
}
