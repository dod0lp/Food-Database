import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { FoodService, SimpleFoodRequest } from '../food.service';

interface IngredientInput {
  foodId: number | null;
  weightInGrams: number | null;
}

@Component({
  imports: [FormsModule],
  templateUrl: './create-food.component.html',
  styleUrl: './create-food.component.css'
})

export class CreateFoodComponent {
  private readonly foods = inject(FoodService);
  private readonly router = inject(Router);

  name = ''; description = '';
  
  ingredients: IngredientInput[] = [{ foodId: null, weightInGrams: null }];
  kind: 'simple' | 'composite' = 'simple';
  saving = false; error = '';

  readonly nutrients = {
    energy: null as number | null,
    fat: null as number | null,
    saturated: null as number | null,
    carbs: null as number | null,
    sugar: null as number | null,
    protein: null as number | null,
    salt: null as number | null
  };

  readonly nutrientFields = [
    { key: 'energy', label: 'Energy', unit: 'kcal', step: 1 },
    { key: 'fat', label: 'Fat', unit: 'g', step: 0.01 },
    { key: 'saturated', label: 'Saturated fat', unit: 'g', step: 0.01 },
    { key: 'carbs', label: 'Carbs', unit: 'g', step: 0.01 },
    { key: 'sugar', label: 'Sugar', unit: 'g', step: 0.01 },
    { key: 'protein', label: 'Protein', unit: 'g', step: 0.01 },
    { key: 'salt', label: 'Salt', unit: 'g', step: 0.01 }
  ] as const;

  addIngredient(): void {
    this.ingredients.push({ foodId: null, weightInGrams: null });
  }

  removeIngredient(index: number): void {
    this.ingredients.splice(index, 1);
  }

  submit(): void {
    this.saving = true;
    this.error = '';

    if (this.kind === 'simple') {
      const request: SimpleFoodRequest = {
        name: this.name,
        description: this.description || null,
        energyKcal: this.nutrients.energy,
        fatTotal: this.nutrients.fat,
        fatSaturated: this.nutrients.saturated,
        carbsTotal: this.nutrients.carbs,
        carbsSugar: this.nutrients.sugar,
        proteinTotal: this.nutrients.protein,
        saltTotal: this.nutrients.salt
      };

      this.foods.createSimple(request)
        .subscribe({ next: food => this.router.navigate(['/foods', food.id]), error: () => this.fail() });
      return;
    }

    const ingredients = this.ingredients
      .filter(x => x.foodId !== null && x.weightInGrams !== null)
      .map(x => ({ foodId: x.foodId!, weightInGrams: x.weightInGrams! }));
      
    this.foods.createComposite(this.name, this.description, ingredients)
      .subscribe({ next: food => this.router.navigate(['/foods', food.id]), error: () => this.fail() });
  }

  private fail(): void { this.saving = false; this.error = 'Could not create the food. Check the values and ingredient IDs.'; }
}