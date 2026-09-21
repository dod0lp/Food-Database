import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FoodService } from '../food.service';
import type { Food } from '../food.types';

/**
 * Component for listing foods.
 */
@Component({
  imports: [RouterLink],
  templateUrl: './food-list.component.html',
  styleUrl: './food-list.component.css'
})
export class FoodListComponent implements OnInit {
  private readonly foodsApi = inject(FoodService);
  private readonly route = inject(ActivatedRoute);

  readonly foods = signal<Food[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly page = signal(1);
  readonly total = signal(0);

  /** Number of foods per page */
  readonly pageSize = 50;
  mine = false;

  /**
   * Gets page count based on number of foods and pagesize.
   * @returns Page count.
   */
  pageCount(): number { return Math.max(1, Math.ceil(this.total() / this.pageSize)); }

  /**
   * Init function.
   */
  ngOnInit(): void {
    this.mine = this.route.snapshot.data['scope'] === 'mine';
    this.load(1);
  }

  /**
   * Loads the previous page of foods.
   */
  previousPage(): void { if (this.page() > 1) { this.load(this.page() - 1); } }

  /**
   * Loads next page for foods.
   */
  nextPage(): void { if (this.page() < this.pageCount()) { this.load(this.page() + 1); } }

  /**
   * Loads page based on page number.
   * @param page Pgae number.
   */
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
