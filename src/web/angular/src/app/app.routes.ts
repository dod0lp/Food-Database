import { Routes } from '@angular/router';
import { authGuard } from './auth/auth.guard';
import { FoodListComponent } from './food/food-list/food-list.component';
import { LoginComponent } from './auth/login/login.component';
import { RegisterComponent } from './auth/register/register.component';

export const routes: Routes = [
  { path: '', component: FoodListComponent, data: { scope: 'system' } },
  { path: 'my-foods', component: FoodListComponent, canActivate: [authGuard], data: { scope: 'mine' } },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'favorites', canActivate: [authGuard], loadComponent: () => import('./food/favorites/favorites.component').then(c => c.FavoritesComponent) },
  { path: 'foods/new', canActivate: [authGuard], loadComponent: () => import('./food/create-food/create-food.component').then(c => c.CreateFoodComponent) },
  { path: 'foods/:id', loadComponent: () => import('./food/food-detail/food-detail.component').then(c => c.FoodDetailComponent) },
  { path: '**', redirectTo: '' }
];
