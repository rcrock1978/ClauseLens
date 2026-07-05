import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'cl-contracts-list',
  standalone: true,
  imports: [RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="cl-card">
      <h2>Contracts</h2>
      <p>Contract listing is wired to <code>GET /api/v1/contracts</code> (deferred to a follow-up; use upload page for now).</p>
      <a routerLink="/contracts/upload"><button>Upload a contract</button></a>
    </div>
  `,
})
export class ContractsListComponent {}
