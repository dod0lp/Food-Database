import { Component, inject, OnInit, signal } from '@angular/core';
import { AsyncPipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { switchMap } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { FoodService } from './food.service';

@Component({
  imports: [AsyncPipe, DecimalPipe, RouterLink],
  template: `
    @if (food | async; as item) {
      <section class="card">
        <a routerLink="/">← All foods</a>
        <h1>{{ item.name }}</h1>
        @if (item.description) { <p>{{ item.description }}</p> }
        <h2>Nutrients per {{ item.weight }} g</h2>
        <dl>
          <dt>Energy</dt><dd>{{ item.nutrientContent.energy.kcal }} kcal</dd>
          <dt>Fat</dt><dd>{{ item.nutrientContent.fatContent.total }} g</dd>
          <dt>Saturated fat</dt><dd>{{ item.nutrientContent.fatContent.saturated }} g</dd>
          <dt>Carbs</dt><dd>{{ item.nutrientContent.carbohydrateContent.total }} g</dd>
          <dt>Sugar</dt><dd>{{ item.nutrientContent.carbohydrateContent.sugar }} g</dd>
          <dt>Protein</dt><dd>{{ item.nutrientContent.protein.total }} g</dd>
          <dt>Salt</dt><dd>{{ item.nutrientContent.salt.total }} g</dd>
        </dl>
        @if (item.ingredients.length) {
          <h2>Ingredients</h2>
          <ul>@for (ingredient of item.ingredients; track ingredient.id) {
            <li><a [routerLink]="['/foods', ingredient.id]">{{ ingredient.name }}</a> — {{ ingredient.weight | number:'1.0-2' }} g</li>
          }</ul>
        }
        @if (auth.currentUser() && !isFavorite()) {
          <button (click)="favorite(item.id)">Add to favorites</button>
        } @else if (auth.currentUser()) {
          <p>This food is in <a routerLink="/favorites">your favorites</a>.</p>
        }
        @if (message()) { <p>{{ message() }}</p> }
      </section>
    } @else { <p>Loading food…</p> }
  `,
  styles: `dl { display:grid; grid-template-columns: 10rem 1fr; gap:.3rem 1rem; } dt { font-weight:600; } dd { margin:0; }`
})
export class FoodDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly foods = inject(FoodService);
  readonly auth = inject(AuthService);
  readonly food = this.route.paramMap.pipe(switchMap(params => this.foods.get(Number(params.get('id')))));
  readonly message = signal('');
  readonly isFavorite = signal(false);

  ngOnInit(): void {
    if (this.auth.currentUser()) {
      const id = Number(this.route.snapshot.paramMap.get('id'));
      this.foods.isFavorite(id).subscribe(value => this.isFavorite.set(value));
    }
  }

  favorite(id: number): void {
    this.foods.setFavorite(id, null).subscribe({
      next: () => {
        this.isFavorite.set(true);
        this.message.set('Added to favorites. You can add a note and package sizes in Favorites.');
      },
      error: () => this.message.set('Could not add this food to favorites.')
    });
  }
}
