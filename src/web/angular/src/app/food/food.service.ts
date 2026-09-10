import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface Food { id: number; name: string; weight: number; description: string; }
export interface FavoriteFood { food: Food; options: { weight: number; price: number | null }[]; }

@Injectable({ providedIn: 'root' })
export class FoodService {
  constructor(private readonly http: HttpClient) {}

  list(): Observable<Food[]> { return this.http.get<Food[]>('/api/foods?from=1&to=50'); }
  favorites(): Observable<FavoriteFood[]> { return this.http.get<FavoriteFood[]>('/api/foods/favorites'); }
}
