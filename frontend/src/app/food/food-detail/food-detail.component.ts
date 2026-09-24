import { Component, inject, signal } from '@angular/core';
import { AsyncPipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, map, of, switchMap, tap } from 'rxjs';
import { AuthService } from '../../auth/auth.service';
import { FoodService } from '../food.service';
import type { Food } from '../food.types';
import { FoodFormatter, getNutritionRows, getScaledNutritionRows } from '../food.types';
import { PortionCalculatorComponent } from '../portion-calculator/portion-calculator.component';

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
  imports: [AsyncPipe, PortionCalculatorComponent, RouterLink],
  templateUrl: './food-detail.component.html',
  styleUrl: './food-detail.component.css'
})
export class FoodDetailComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly foods = inject(FoodService);
  readonly auth = inject(AuthService);
  readonly formatter = new FoodFormatter();

  readonly nutritionRows = getNutritionRows;
  readonly scaledNutritionRows = getScaledNutritionRows;
  portionWeight: number | null = null;
  
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
   * Scales an ingredient amount to the entered portion weight.
   * @param food Composite food containing the ingredient.
   * @param ingredientWeight Ingredient amount in the food's base weight.
   * @returns Scaled ingredient weight, or null when it cannot be calculated.
   */
  scaledIngredientWeight(food: Food, ingredientWeight: number): number | null {
    const weight = this.portionWeight;

    if ((typeof weight !== 'number' || !Number.isFinite(weight) || weight <= 0)
        || (!Number.isFinite(food.weight) || food.weight <= 0)
        || (!Number.isFinite(ingredientWeight) || ingredientWeight < 0)) {
      return null;
    }

    return ingredientWeight * weight / food.weight;
  }

}
