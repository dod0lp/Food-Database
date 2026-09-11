import { Component, inject } from '@angular/core';
import { AsyncPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { FoodService } from './food.service';

@Component({
  imports: [AsyncPipe, RouterLink],
  template: `
    <section class="card">
      <h1>Foods</h1>
      @if (foods | async; as results) {
        <ul>@for (food of results; track food.id) { <li><a [routerLink]="['/foods', food.id]">{{ food.name }}</a> <small>#{{ food.id }}</small></li> }</ul>
      } @else { <p>Loading foods…</p> }
    </section>
  `
})
export class FoodListComponent {
  readonly foods = inject(FoodService).list();
}
