import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import type { FavoriteFood, Food, FoodPage } from './food.types';

/**
 * Request that represents food object with name, description and nutrinet values.
 */
export interface SimpleFoodRequest {
  name: string;
  description: string | null;
  energyKcal: number | null;
  fatTotal: number | null; fatSaturated: number | null;
  carbsTotal: number | null; carbsSugar: number | null;
  proteinTotal: number | null; saltTotal: number | null;
}

/**
 * Food service for API calls, requests into food databse
 */
@Injectable({ providedIn: 'root' })
export class FoodService {
  constructor(private readonly http: HttpClient) {}

  /**
   * Gets a page of public foods.
   */
  list(page: number, pageSize: number): Observable<FoodPage> {
    return this.http.get<FoodPage>(`/api/foods?page=${page}&pageSize=${pageSize}`);
  }

  /**
   * Gets a page of foods created by the current user.
   */
  myFoods(page: number, pageSize: number): Observable<FoodPage> {
    return this.http.get<FoodPage>(`/api/foods/mine?page=${page}&pageSize=${pageSize}`);
  }

  /**
   * Gets one food by ID.
   */
  get(id: number): Observable<Food> { return this.http.get<Food>(`/api/foods/${id}`); }

  /**
   * Checks whether a food is in the current user's favorites.
   */
  isFavorite(id: number): Observable<boolean> { return this.http.get<boolean>(`/api/foods/${id}/favorite`); }

  /**
   * Gets the current user's favorite foods.
   */
  favorites(): Observable<FavoriteFood[]> { return this.http.get<FavoriteFood[]>('/api/foods/favorites'); }

  /**
   * Creates a simple food.
   */
  createSimple(request: SimpleFoodRequest): Observable<Food> { return this.http.post<Food>('/api/foods', request); }

  /**
   * Creates a food from other foods.
   */
  createComposite(name: string, description: string, ingredients: { foodId: number; weightInGrams: number }[]): Observable<Food> {
    return this.http.post<Food>('/api/foods/composites', { name, description, ingredients });
  }
  
  /**
   * Adds or updates a favorite food's personal details.
   */
  setFavorite(id: number, remark: string | null, options: { weight: number; price: number | null }[] = []): Observable<Food> {
    return this.http.put<Food>(`/api/foods/${id}/favorite`, { remark, options });
  }
  
  /**
   * Removes a food from the current user's favorites.
   */
  removeFavorite(id: number, deletePersonalData = false): Observable<void> {
    return this.http.delete<void>(`/api/foods/${id}/favorite?deletePersonalData=${deletePersonalData}`);
  }
  
  /**
   * Removes a saved package option from a favorite food.
   */
  removeOption(id: number, weight: number): Observable<void> {
    return this.http.delete<void>(`/api/foods/${id}/favorite/options/${weight}`);
  }
}
