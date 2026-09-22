import { Component, inject, signal } from '@angular/core';
import { AsyncPipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, map, of, switchMap, tap } from 'rxjs';
import { AuthService } from '../../auth/auth.service';
import { FoodService } from '../food.service';
import type { Food, NutrientRow } from '../food.types';
import { FoodFormatter } from '../food.types';

/**
 * Nutrient content from imported Food
 */
type NutrientContent = Food['nutrientContent'];

/**
 * Result of request for loading food
 */
type FoodLoadResult =
  | { kind: 'found'; food: Food }
  | { kind: 'not-found' }
  | { kind: 'error' };

/**
 * Class component for food details.
 */
@Component({
  imports: [AsyncPipe, DecimalPipe, RouterLink],
  templateUrl: './food-detail.component.html',
  styleUrl: './food-detail.component.css'
})
export class FoodDetailComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly foods = inject(FoodService);
  readonly auth = inject(AuthService);
  readonly formatter = new FoodFormatter();
  
  readonly message = signal('');
  readonly isFavorite = signal(false);

  /**
   * Gets food by ID. Sends reply.
   */
  readonly food = this.route.paramMap.pipe(
    map(params => Number(params.get('id'))),
    tap(() => {
      this.message.set('');
      this.isFavorite.set(false);
    }),
    switchMap(id => {
      if (!Number.isSafeInteger(id) || id <= 0) {
        return of<FoodLoadResult>({ kind: 'not-found' });
      }

      return this.foods.get(id).pipe(
        tap(food => this.loadFavoriteStatus(food.id)),
        map(food => ({ kind: 'found', food }) satisfies FoodLoadResult),
        catchError((error: HttpErrorResponse) => of<FoodLoadResult>(
          error.status === 404 ? { kind: 'not-found' } : { kind: 'error' }
        ))
      );
    })
  );

  /**
   * Loads favorite food by ID.
   * @param id ID of food to check.
   * @returns Status if it loaded or not.
   */
  private loadFavoriteStatus(id: number): void {
    if (!this.auth.currentUser()) {
      return;
    }

    this.foods.isFavorite(id).pipe(
      catchError(() => of(false))
    ).subscribe(value => this.isFavorite.set(value));
  }


  /**
   * Add food to favorites, by call to FoodService.
   * @param id ID of a food.
   */
  favorite(id: number): void {
    this.foods.setFavorite(id, null).subscribe({
      next: () => {
        this.isFavorite.set(true);
        this.message.set('Added to favorites. You can add a note and package sizes in Favorites.');
      },
      error: () => this.message.set('Could not add this food to favorites.')
    });
  }

  /**
   * Nutrition rows for writing out food details into rows, mainly for description for example with "dt" and "dd" html tags
   * @param content NutrientContent of food.
   * @returns Row to output
   */
  nutritionRows(content: NutrientContent): NutrientRow[] {
    return [
      { label: 'Energy', value: content.energy.kcal, unit: 'kcal' },
      { label: 'Fat', value: content.fatContent.total, unit: 'g' },
      { label: 'Saturated fat', value: content.fatContent.saturated, unit: 'g' },
      { label: 'Carbs', value: content.carbohydrateContent.total, unit: 'g' },
      { label: 'Sugar', value: content.carbohydrateContent.sugar, unit: 'g' },
      { label: 'Protein', value: content.protein.total, unit: 'g' },
      { label: 'Salt', value: content.salt.total, unit: 'g' }
    ];
  }
}
