import { Routes } from '@angular/router';
import { AssetsComponent } from './components/assets/assets.component';
export const routes: Routes = [
  { path: '', redirectTo: 'assets', pathMatch: 'full' },
  { path: 'assets', component: AssetsComponent }
];
