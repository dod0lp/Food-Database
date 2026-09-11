import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { FavoriteFood, FoodService } from './food.service';

@Component({
  imports: [FormsModule, RouterLink],
  template: `
    <section class="card">
      <h1>My favorites</h1>
      @if (loading()) { <p>Loading favorites…</p> }
      @if (error()) { <p class="error">{{ error() }}</p> }
      @if (!loading() && !favorites().length) { <p>You have no favorites yet.</p> }
      @for (favorite of favorites(); track favorite.food.id) {
        <article>
          <h2><a [routerLink]="['/foods', favorite.food.id]">{{ favorite.food.name }}</a></h2>
          <label>Your remark
            <textarea [name]="'remark' + favorite.food.id" [(ngModel)]="favorite.remark"></textarea>
          </label>
          <button (click)="saveRemark(favorite)">Save remark</button>
          <h3>Saved package sizes / prices</h3>
          <ul>@for (option of favorite.options; track option.weight) {
            <li>{{ option.weight }} g — {{ option.price ?? 'no price' }}
              <button (click)="removeOption(favorite, option.weight)">Remove</button>
            </li>
          }</ul>
          <div class="option">
            <label>Weight g <input [name]="'weight' + favorite.food.id" type="number" [(ngModel)]="newWeights[favorite.food.id]"></label>
            <label>Price EUR <input [name]="'price' + favorite.food.id" type="number" step="0.01" [(ngModel)]="newPrices[favorite.food.id]"></label>
            <button (click)="addOption(favorite)">Save size</button>
          </div>
          <button class="danger" (click)="removeFavorite(favorite)">Remove from favorites</button>
        </article>
      }
    </section>
  `
  ,styles: `article { border-top:1px solid #ddd; margin-top:1.5rem; padding-top:1rem; } label { display:grid; gap:.3rem; margin:.6rem 0; max-width:25rem; } textarea { min-height:4rem; } .option { display:flex; gap:.5rem; align-items:end; flex-wrap:wrap; } .danger { margin-top:1rem; color:#a00; }`
})
export class FavoritesComponent implements OnInit {
  private readonly foods = inject(FoodService);
  readonly favorites = signal<FavoriteFood[]>([]);
  newWeights: Record<number, number | null> = {};
  newPrices: Record<number, number | null> = {};
  readonly loading = signal(true);
  readonly error = signal('');

  ngOnInit(): void { this.load(); }

  saveRemark(favorite: FavoriteFood): void {
    this.foods.setFavorite(favorite.food.id, favorite.remark).subscribe();
  }

  addOption(favorite: FavoriteFood): void {
    const weight = this.newWeights[favorite.food.id];
    if (!weight || weight <= 0) { return; }
    this.foods.setFavorite(favorite.food.id, null, [{ weight, price: this.newPrices[favorite.food.id] ?? null }])
      .subscribe(() => this.load());
  }

  removeOption(favorite: FavoriteFood, weight: number): void {
    this.foods.removeOption(favorite.food.id, weight).subscribe(() => this.load());
  }

  removeFavorite(favorite: FavoriteFood): void {
    this.foods.removeFavorite(favorite.food.id).subscribe(() => this.load());
  }

  private load(): void {
    this.loading.set(true);
    this.error.set('');
    this.foods.favorites().subscribe({
      next: favorites => {
        this.favorites.set(favorites);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Favorites could not be loaded. Refresh the page and try again.');
      }
    });
  }
}
