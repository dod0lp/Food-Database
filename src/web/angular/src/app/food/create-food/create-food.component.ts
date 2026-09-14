import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { FoodService, SimpleFoodRequest } from '../food.service';

interface IngredientInput { foodId: number | null; weightInGrams: number | null; }

@Component({
  imports: [FormsModule],
  templateUrl: './create-food.component.html',
  styleUrl: './create-food.component.css'
})
export class CreateFoodComponent {
  private readonly foods = inject(FoodService);
  private readonly router = inject(Router);
  kind: 'simple' | 'composite' = 'simple';
  name = ''; description = '';
  energy: number | null = null; fat: number | null = null; saturated: number | null = null;
  carbs: number | null = null; sugar: number | null = null; protein: number | null = null; salt: number | null = null;
  ingredients: IngredientInput[] = [{ foodId: null, weightInGrams: null }];
  saving = false; error = '';

  addIngredient(): void { this.ingredients.push({ foodId: null, weightInGrams: null }); }
  removeIngredient(index: number): void { this.ingredients.splice(index, 1); }

  submit(): void {
    this.saving = true; this.error = '';
    if (this.kind === 'simple') {
      const request: SimpleFoodRequest = {
        name: this.name, description: this.description || null, energyKcal: this.energy,
        fatTotal: this.fat, fatSaturated: this.saturated, carbsTotal: this.carbs,
        carbsSugar: this.sugar, proteinTotal: this.protein, saltTotal: this.salt
      };
      this.foods.createSimple(request).subscribe({ next: food => this.router.navigate(['/foods', food.id]), error: () => this.fail() });
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
