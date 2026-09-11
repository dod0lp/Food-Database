import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface Food {
  id: number;
  name: string;
  weight: number;
  description: string;
  nutrientContent: {
    energy: { kcal: number };
    fatContent: { total: number; saturated: number };
    carbohydrateContent: { total: number; sugar: number };
    protein: { total: number };
    salt: { total: number };
  };
  ingredients: Food[];
}

export interface FavoriteFood {
  food: Food;
  remark: string | null;
  options: { weight: number; price: number | null }[];
}

export interface SimpleFoodRequest {
  name: string; description: string | null; energyKcal: number | null;
  fatTotal: number | null; fatSaturated: number | null;
  carbsTotal: number | null; carbsSugar: number | null;
  proteinTotal: number | null; saltTotal: number | null;
}

@Injectable({ providedIn: 'root' })
export class FoodService {
  constructor(private readonly http: HttpClient) {}

  list(): Observable<Food[]> { return this.http.get<Food[]>('/api/foods?from=1&to=50'); }
  get(id: number): Observable<Food> { return this.http.get<Food>(`/api/foods/${id}`); }
  isFavorite(id: number): Observable<boolean> { return this.http.get<boolean>(`/api/foods/${id}/favorite`); }
  favorites(): Observable<FavoriteFood[]> { return this.http.get<FavoriteFood[]>('/api/foods/favorites'); }
  createSimple(request: SimpleFoodRequest): Observable<Food> { return this.http.post<Food>('/api/foods', request); }
  createComposite(name: string, description: string, ingredients: { foodId: number; weightInGrams: number }[]): Observable<Food> {
    return this.http.post<Food>('/api/foods/composites', { name, description, ingredients });
  }
  setFavorite(id: number, remark: string | null, options: { weight: number; price: number | null }[] = []): Observable<Food> {
    return this.http.put<Food>(`/api/foods/${id}/favorite`, { remark, options });
  }
  removeFavorite(id: number): Observable<void> { return this.http.delete<void>(`/api/foods/${id}/favorite`); }
  removeOption(id: number, weight: number): Observable<void> {
    return this.http.delete<void>(`/api/foods/${id}/favorite/options/${weight}`);
  }
}
