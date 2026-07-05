import { Routes } from '@angular/router';

export const APP_ROUTES: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'contracts' },
  {
    path: 'auth/signup',
    loadComponent: () => import('./features/auth/signup.component').then((m) => m.SignupComponent),
  },
  {
    path: 'auth/login',
    loadComponent: () => import('./features/auth/login.component').then((m) => m.LoginComponent),
  },
  {
    path: 'auth/verify',
    loadComponent: () => import('./features/auth/verify.component').then((m) => m.VerifyComponent),
  },
  {
    path: 'contracts',
    loadComponent: () => import('./features/contracts/list.component').then((m) => m.ContractsListComponent),
  },
  {
    path: 'contracts/upload',
    loadComponent: () => import('./features/contracts/upload.component').then((m) => m.UploadContractComponent),
  },
  {
    path: 'contracts/:id',
    loadComponent: () => import('./features/contracts/detail.component').then((m) => m.ContractDetailComponent),
  },
  {
    path: 'contracts/:id/review/assign',
    loadComponent: () => import('./features/review/assign/review-assign.component').then((m) => m.ReviewAssignComponent),
  },
  {
    path: 'contracts/:id/review/decide',
    loadComponent: () => import('./features/review/decide/review-decide.component').then((m) => m.ReviewDecideComponent),
  },
  {
    path: 'playbooks',
    loadComponent: () => import('./features/playbooks/playbooks.component').then((m) => m.PlaybooksComponent),
  },
  {
    path: 'audit',
    loadComponent: () => import('./features/audit/audit.component').then((m) => m.AuditComponent),
  },
  { path: '**', redirectTo: 'contracts' },
];
