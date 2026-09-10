import { Component, inject } from '@angular/core';
import { AsyncPipe } from '@angular/common';
import { FoodService } from './food.service';

@Component({
  imports: [AsyncPipe],
  template: `
    <section class="card">
      <h1>My favorites</h1>
      @if (favorites | async; as results) {
        @if (results.length) {
          <ul>@for (favorite of results; track favorite.food.id) {
            <li><strong>{{ favorite.food.name }}</strong>
              @if (favorite.options.length) { <span> — {{ favorite.options.length }} saved size(s)</span> }
            </li>
          }</ul>
        } @else { <p>You have no favorites yet.</p> }
      } @else { <p>Loading favorites…</p> }
    </section>
  `
})
export class FavoritesComponent {
  readonly favorites = inject(FoodService).favorites();
}
