import { Component, inject, OnInit, signal } from '@angular/core';
import { AsyncPipe, DecimalPipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { switchMap } from 'rxjs';
import { AuthService } from '../../auth/auth.service';
import { FoodService } from '../food.service';

@Component({
  imports: [AsyncPipe, DecimalPipe, RouterLink],
  templateUrl: './food-detail.component.html',
  styleUrl: './food-detail.component.css'
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
