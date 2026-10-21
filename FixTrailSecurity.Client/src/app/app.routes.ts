import { Routes } from '@angular/router';
import { AssetsComponent } from './components/assets/assets.component';
import { FindingsComponent } from './components/findings/findings.component';
import { LoginComponent } from './components/login/login.component';
import { authGuard } from './guards/auth.guard';
export const routes: Routes = [
  { path: '', redirectTo: 'assets', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'assets', component: AssetsComponent, canActivate: [authGuard] },
  { path: 'findings/:assetId', component: FindingsComponent, canActivate: [authGuard] }
];
