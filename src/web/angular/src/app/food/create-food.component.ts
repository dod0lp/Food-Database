import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { FoodService, SimpleFoodRequest } from './food.service';

interface IngredientInput { foodId: number | null; weightInGrams: number | null; }

@Component({
  imports: [FormsModule],
  template: `
    <section class="card">
      <h1>Create food</h1>
      <label><input type="radio" name="kind" [(ngModel)]="kind" value="simple"> Simple food</label>
      <label><input type="radio" name="kind" [(ngModel)]="kind" value="composite"> Meal / composite food</label>
      <form (ngSubmit)="submit()">
        <label>Name <input name="name" [(ngModel)]="name" required maxlength="200"></label>
        <label>Description <textarea name="description" [(ngModel)]="description"></textarea></label>
        @if (kind === 'simple') {
          <p>All nutrient values are per 100 g. Leave a value blank when unknown.</p>
          <div class="nutrients">
            <label>Energy kcal <input name="energy" type="number" [(ngModel)]="energy"></label>
            <label>Fat g <input name="fat" type="number" step="0.01" [(ngModel)]="fat"></label>
            <label>Saturated fat g <input name="saturated" type="number" step="0.01" [(ngModel)]="saturated"></label>
            <label>Carbs g <input name="carbs" type="number" step="0.01" [(ngModel)]="carbs"></label>
            <label>Sugar g <input name="sugar" type="number" step="0.01" [(ngModel)]="sugar"></label>
            <label>Protein g <input name="protein" type="number" step="0.01" [(ngModel)]="protein"></label>
            <label>Salt g <input name="salt" type="number" step="0.01" [(ngModel)]="salt"></label>
          </div>
        } @else {
          <p>Use existing food IDs and the actual grams used in the recipe.</p>
          @for (ingredient of ingredients; track $index) {
            <div class="ingredient">
              <label>Food ID <input [name]="'id' + $index" type="number" [(ngModel)]="ingredient.foodId" required></label>
              <label>Grams <input [name]="'weight' + $index" type="number" step="0.01" [(ngModel)]="ingredient.weightInGrams" required></label>
              <button type="button" (click)="removeIngredient($index)">Remove</button>
            </div>
          }
          <button type="button" (click)="addIngredient()">Add ingredient</button>
        }
        @if (error) { <p class="error">{{ error }}</p> }
        <button [disabled]="saving">Create and favorite</button>
      </form>
    </section>
  `,
  styles: `form, .nutrients { display:grid; gap:1rem; margin-top:1rem; } label { display:grid; gap:.3rem; } .nutrients { grid-template-columns:repeat(auto-fit,minmax(10rem,1fr)); } .ingredient { display:flex; gap:1rem; align-items:end; } textarea { min-height:4rem; }`
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
