import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'cl-playbooks',
  standalone: true,
  imports: [CommonModule, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="cl-card">
      <h2>Playbooks</h2>
      <button (click)="importTemplates()">Import NDA / MSA / DPA templates</button>
      <p *ngIf="status()">{{ status() }}</p>
      <p>Existing playbooks will be listed here once a GET endpoint is wired.</p>
    </div>
  `,
})
export class PlaybooksComponent implements OnInit {
  private http = inject(HttpClient);
  status = signal<string | null>(null);

  ngOnInit() {}

  importTemplates() {
    this.http.post('/api/v1/playbooks/import', { templateIds: ['nda', 'msa', 'dpa'] }).subscribe({
      next: (r: any) => this.status.set(`Imported ${r.rulesImported} rules into playbook ${r.playbookId}`),
      error: (e) => this.status.set(`Failed: ${e?.error?.error ?? 'unknown'}`),
    });
  }
}
