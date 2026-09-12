import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Food, FoodService } from './food.service';

@Component({
  imports: [RouterLink],
  template: `
    <section class="card">
      <h1>{{ mine ? 'My foods' : 'Foods' }}</h1>
      @if (loading()) { <p>Loading foods…</p> }
      @if (error()) { <p class="error">{{ error() }}</p> }
      @if (!loading() && !foods().length) {
        <p>{{ mine ? 'You have not created any foods yet.' : 'No system foods found.' }}</p>
      }
      <ul>@for (food of foods(); track food.id) {
        <li><a [routerLink]="['/foods', food.id]">{{ food.name }}</a> <small>#{{ food.id }}</small></li>
      }</ul>
      <nav class="pages">
        <button type="button" (click)="previous()" [disabled]="page() === 1">← Previous</button>
        <span>Page {{ page() }} of {{ pageCount() }}</span>
        <button type="button" (click)="next()" [disabled]="page() >= pageCount()">Next →</button>
      </nav>
    </section>
  `,
  styles: `.pages { display:flex; align-items:center; gap:1rem; margin-top:1rem; }`
})
export class FoodListComponent implements OnInit {
  private readonly foodsApi = inject(FoodService);
  private readonly route = inject(ActivatedRoute);
  readonly foods = signal<Food[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly page = signal(1);
  readonly total = signal(0);
  readonly pageSize = 50;
  mine = false;

  pageCount(): number { return Math.max(1, Math.ceil(this.total() / this.pageSize)); }

  ngOnInit(): void {
    this.mine = this.route.snapshot.data['scope'] === 'mine';
    this.load(1);
  }

  previous(): void { if (this.page() > 1) { this.load(this.page() - 1); } }
  next(): void { if (this.page() < this.pageCount()) { this.load(this.page() + 1); } }

  private load(page: number): void {
    this.loading.set(true);
    this.error.set('');
    const request = this.mine
      ? this.foodsApi.myFoods(page, this.pageSize)
      : this.foodsApi.list(page, this.pageSize);
    request.subscribe({
      next: response => {
        this.foods.set(response.items);
        this.total.set(response.total);
        this.page.set(page);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Foods could not be loaded.');
      }
    });
  }
}
