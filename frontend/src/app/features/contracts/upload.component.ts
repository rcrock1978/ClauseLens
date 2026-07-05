import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';

@Component({
  selector: 'cl-upload-contract',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="cl-card" style="max-width:560px;margin:2rem auto;">
      <h2>Upload contract</h2>
      <p>PDF or DOCX, up to 25 MB and 50 pages. Password-protected files are rejected.</p>
      <input type="file" accept=".pdf,.docx" (change)="onPick($event)" />
      <p *ngIf="error" style="color: var(--cl-danger)">{{ error }}</p>
      <button [disabled]="!file || busy" (click)="submit()">Upload & analyze</button>
    </div>
  `,
})
export class UploadContractComponent {
  private http = inject(HttpClient);
  private router = inject(Router);

  file: File | null = null;
  busy = false;
  error: string | null = null;

  onPick(e: Event) {
    const input = e.target as HTMLInputElement;
    this.file = input.files?.[0] ?? null;
  }

  submit() {
    if (!this.file) return;
    this.busy = true; this.error = null;
    const fd = new FormData();
    fd.append('file', this.file);
    this.http.post('/api/v1/contracts', fd).subscribe({
      next: (r: any) => { this.busy = false; this.router.navigate(['/contracts', r.contractId]); },
      error: (e) => { this.busy = false; this.error = e?.error?.error ?? 'Upload failed'; },
    });
  }
}
