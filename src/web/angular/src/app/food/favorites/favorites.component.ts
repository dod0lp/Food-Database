import { Component, inject, OnInit, signal, WritableSignal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { FavoriteFood, Food, FoodService } from '../food.service';

interface NutrientRow {
  label: string;
  value: number;
  unit: string;
}

@Component({
  imports: [FormsModule, RouterLink],
  templateUrl: './favorites.component.html',
  styleUrl: './favorites.component.css'
})
export class FavoritesComponent implements OnInit {
  private readonly foods = inject(FoodService);

  readonly favorites = signal<FavoriteFood[]>([]);

  readonly loading = signal(true);
  readonly error = signal('');
  readonly message = signal('');

  readonly expandedIds = signal<Set<number>>(new Set());
  readonly remarkEditorIds = signal<Set<number>>(new Set());
  readonly optionEditorIds = signal<Set<number>>(new Set());
  readonly busyIds = signal<Set<number>>(new Set());

  readonly editingOptionWeights: Record<number, number | null> = {};
  readonly newWeights: Record<number, number | null> = {};
  readonly newPrices: Record<number, number | null> = {};
  readonly remarkDrafts: Record<number, string> = {};
  removeDataChoice: 'keep' | 'delete' = 'keep';

  ngOnInit(): void { this.load(); }
  isExpanded(id: number): boolean { return this.expandedIds().has(id); }
  isEditingRemark(id: number): boolean { return this.remarkEditorIds().has(id); }
  isAddingOption(id: number): boolean { return this.optionEditorIds().has(id); }
  isEditingOption(id: number): boolean { return ((this.editingOptionWeights[id] !== null) && (this.editingOptionWeights[id] !== undefined)); }
  isBusy(id: number): boolean { return this.busyIds().has(id); }
  hasRemark(favorite: FavoriteFood): boolean { return !!favorite.remark?.trim(); }
  toggleExpanded(id: number): void { this.toggleId(this.expandedIds, id); }

  nutritionRows(food: Food, targetWeight = food.weight): NutrientRow[] {
    const factor = food.weight > 0 ? (targetWeight / food.weight) : 1;

    return [
      { label: 'Energy', value: food.nutrientContent.energy.kcal, unit: 'kcal' },
      { label: 'Fat', value: food.nutrientContent.fatContent.total, unit: 'g' },
      { label: 'Saturated fat', value: food.nutrientContent.fatContent.saturated, unit: 'g' },
      { label: 'Carbs', value: food.nutrientContent.carbohydrateContent.total, unit: 'g' },
      { label: 'Sugar', value: food.nutrientContent.carbohydrateContent.sugar, unit: 'g' },
      { label: 'Protein', value: food.nutrientContent.protein.total, unit: 'g' },
      { label: 'Salt', value: food.nutrientContent.salt.total, unit: 'g' }
    ]
      .filter(nutrient => Number.isFinite(nutrient.value) && nutrient.value >= 0)
      .map(nutrient => ({ ...nutrient, value: nutrient.value * factor }));
  }

  pricePer100g(price: number, weight: number): number { return weight > 0 ? price * 100 / weight : 0; }

  formatDecimal(value: number, minimumFractionDigits = 0, maximumFractionDigits = 2): string {
    return new Intl.NumberFormat('sk-SK', { minimumFractionDigits, maximumFractionDigits }).format(value);
  }

  formatPrice(value: number): string { return `${this.formatDecimal(value, 2, 2)}€`; }

  openRemarkEditor(favorite: FavoriteFood): void {
    this.remarkDrafts[favorite.food.id] = favorite.remark ?? '';
    this.addId(this.remarkEditorIds, favorite.food.id);
  }

  closeRemarkEditor(id: number): void { this.removeId(this.remarkEditorIds, id); }

  openOptionEditor(id: number, weight?: number, price?: number | null): void {
    this.editingOptionWeights[id] = weight ?? null;
    this.newWeights[id] = weight ?? null;
    this.newPrices[id] = price ?? null;
    this.addId(this.optionEditorIds, id);
  }

  closeOptionEditor(id: number): void {
    delete this.editingOptionWeights[id]; delete this.newWeights[id]; delete this.newPrices[id];
    this.removeId(this.optionEditorIds, id);
  }

  saveRemark(favorite: FavoriteFood): void {
    const id = favorite.food.id;
    const remark = this.remarkDrafts[id]?.trim() || null;
    this.setBusy(id, true);

    this.foods.setFavorite(id, remark).subscribe({
      next: () => { this.updateFavorite(id, current => ({ ...current, remark })); this.closeRemarkEditor(id); this.message.set('Remark saved.'); this.setBusy(id, false); },
      error: () => this.operationFailed(id)
    });
  }

  addOption(favorite: FavoriteFood): void {
    const id = favorite.food.id;
    const weight = this.newWeights[id];

    if (!weight || weight <= 0) { this.error.set('Enter a package weight greater than zero.'); return; }

    const price = this.newPrices[id] ?? null;
    this.setBusy(id, true);

    this.foods.setFavorite(id, null, [{ weight, price }]).subscribe({
      next: () => {
        this.updateFavorite(id, current => ({ ...current, options: [...current.options.filter(x => x.weight !== weight), { weight, price }].sort((a, b) => a.weight - b.weight) }));
        const wasEditing = this.isEditingOption(id);
        this.closeOptionEditor(id); this.message.set(wasEditing ? 'Package price saved.' : 'Package size saved.'); this.setBusy(id, false);
      },
      error: () => this.operationFailed(id)
    });
  }

  removeOption(favorite: FavoriteFood, weight: number): void {
    const id = favorite.food.id;
    this.setBusy(id, true);

    this.foods.removeOption(id, weight).subscribe({
      next: () => { this.updateFavorite(id, current =>
          ({...current, options: current.options.filter(x => x.weight !== weight) }));
        this.message.set('Package size removed.'); this.setBusy(id, false); },
      error: () => this.operationFailed(id)
    });
  }

  removeFavorite(favorite: FavoriteFood): void {
    const id = favorite.food.id;
    this.setBusy(id, true);

    this.foods.removeFavorite(id, this.removeDataChoice === 'delete').subscribe({
      next: () => { this.favorites.update(items => items.filter(item => item.food.id !== id)); this.removeId(this.expandedIds, id); this.message.set('Removed from favorites.'); this.setBusy(id, false); },
      error: () => this.operationFailed(id)
    });
  }

  private load(): void {
    this.loading.set(true); this.error.set('');

    this.foods.favorites().subscribe({
      next: favorites => { this.favorites.set(favorites); this.loading.set(false); },
      error: () => { this.loading.set(false); this.error.set('Favorites could not be loaded. Refresh the page and try again.'); }
    });
  }

  private operationFailed(id: number): void { this.setBusy(id, false); this.error.set('That change could not be saved. Please try again.'); }
  private updateFavorite(id: number, update: (favorite: FavoriteFood) => FavoriteFood): void {
    this.favorites.update(items => items.map(item => item.food.id === id ? update(item) : item));
  }
  private setBusy(id: number, busy: boolean): void { busy ? this.addId(this.busyIds, id) : this.removeId(this.busyIds, id); }
  private addId(store: WritableSignal<Set<number>>, id: number): void { store.update(ids => new Set(ids).add(id)); }
  private removeId(store: WritableSignal<Set<number>>, id: number): void {
    store.update(ids => { const next = new Set(ids); next.delete(id); return next; });
  }
  private toggleId(store: WritableSignal<Set<number>>, id: number): void {
    store.update(ids => { const next = new Set(ids); next.has(id) ? next.delete(id) : next.add(id); return next; });
  }
}
