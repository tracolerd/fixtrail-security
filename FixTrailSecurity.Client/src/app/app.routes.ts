import { Routes } from '@angular/router';
import { AssetsComponent } from './components/assets/assets.component';
import { FindingsComponent } from './components/findings/findings.component';
export const routes: Routes = [
  { path: '', redirectTo: 'assets', pathMatch: 'full' },
  { path: 'assets', component: AssetsComponent },
  { path: 'findings/:assetId', component: FindingsComponent }
];
