import { Routes } from '@angular/router';
import { authGuard } from './auth/auth.guard';
import { FoodListComponent } from './food/food-list.component';
import { LoginComponent } from './auth/login.component';
import { RegisterComponent } from './auth/register.component';

export const routes: Routes = [
  { path: '', component: FoodListComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'favorites', canActivate: [authGuard], loadComponent: () => import('./food/favorites.component').then(c => c.FavoritesComponent) },
  { path: '**', redirectTo: '' }
];
