import { Component, inject, OnInit, signal, WritableSignal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { FoodService } from '../food.service';
import type { FavoriteFood, Food, NutrientRow } from '../food.types';
import { FoodFormatter } from '../food.types';

/**
 * Class component for favorite food.
 */
@Component({
  imports: [FormsModule, RouterLink],
  templateUrl: './favorites.component.html',
  styleUrl: './favorites.component.css'
})
export class FavoritesComponent implements OnInit {
  private readonly foods = inject(FoodService);
  readonly formatter = new FoodFormatter();

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

  /**
   * Returns formatted NutrientRow for food.
   * @param food Food to format its values.
   * @param targetWeight Weight to format into.
   * @returns Formatted NutrientRow from food.
   */
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
    ].map(nutrient => ({
      label: nutrient.label,
      value: (Number.isFinite(nutrient.value) && nutrient.value >= 0)
            ? nutrient.value * factor
            : -1,
      unit: nutrient.unit
    }));
  }

  /**
   * Helper to return price per weight.
   * @param price Current price.
   * @param weight Current weight.
   * @param targetWeight Target weight.
   * @returns Price per desired weight.
   */
  pricePerWeight(price: number, weight: number, targetWeight: number): number { return weight > 0 ? price * targetWeight / weight : 0; }

  /**
   * Helper function to open remark editor.
   * @param favorite Favorite food object.
   */
  openRemarkEditor(favorite: FavoriteFood): void {
    this.remarkDrafts[favorite.food.id] = favorite.remark ?? '';
    this.addId(this.remarkEditorIds, favorite.food.id);
  }

  /**
   * Helper function to close remark editor.
   * @param id ID of a food.
   */
  closeRemarkEditor(id: number): void { this.removeId(this.remarkEditorIds, id); }

  /**
   * Function to open option editor for food by ID.
   * @param id ID of a food.
   * @param weight Weight of a food.
   * @param price Price of a food.
   */
  openOptionEditor(id: number, weight?: number, price?: number | null): void {
    this.editingOptionWeights[id] = weight ?? null;
    this.newWeights[id] = weight ?? null;
    this.newPrices[id] = price ?? null;
    this.addId(this.optionEditorIds, id);
  }

  /**
   * Functiont o close option editor for food by ID.
   * @param id ID of a food.
   */
  closeOptionEditor(id: number): void {
    delete this.editingOptionWeights[id]; delete this.newWeights[id]; delete this.newPrices[id];
    this.removeId(this.optionEditorIds, id);
  }

  /**
   * Function to save remark for a food.
   * @param favorite Favorite food object to remark.
   */
  saveRemark(favorite: FavoriteFood): void {
    const id = favorite.food.id;
    const remark = this.remarkDrafts[id]?.trim() || null;
    this.setBusy(id, true);

    this.foods.setFavorite(id, remark).subscribe({
      next: () => { this.updateFavorite(id, current =>
                ({ ...current, remark }));
                this.closeRemarkEditor(id); this.message.set('Remark saved.');
                this.setBusy(id, false); },
      error: () => this.operationFailed(id)
    });
  }

  /**
   * Function to add option to favorite food.
   * @param favorite Favorite food object.
   * @returns Returns if weight is not correct, and sets error.
   */
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

  /**
   * Remove favorite food option.
   * @param favorite Favorite food object.
   * @param weight Weight key to remove.
   */
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

  /**
   * Remove favorite food.
   * @param favorite Favorite food to remove.
   */
  removeFavorite(favorite: FavoriteFood): void {
    const id = favorite.food.id;
    this.setBusy(id, true);

    this.foods.removeFavorite(id, this.removeDataChoice === 'delete').subscribe({
      next: () => { this.favorites.update(items => items.filter(item => item.food.id !== id)); this.removeId(this.expandedIds, id); this.message.set('Removed from favorites.'); this.setBusy(id, false); },
      error: () => this.operationFailed(id)
    });
  }

  /**
   * Helper function to load favorite foods.
   */
  private load(): void {
    this.loading.set(true); this.error.set('');

    this.foods.favorites().subscribe({
      next: favorites => { this.favorites.set(favorites); this.loading.set(false); },
      error: () => { this.loading.set(false); this.error.set('Favorites could not be loaded. Refresh the page and try again.'); }
    });
  }

  /**
   * Helper function to set error if operation failed.
   * @param id ID of a food.
   */
  private operationFailed(id: number): void { this.setBusy(id, false); this.error.set('That change could not be saved. Please try again.'); }

  /**
   * Function to update favorite foods.
   * @param id ID of a food to update.
   * @param update Object to update.
   */
  private updateFavorite(id: number, update: (favorite: FavoriteFood) => FavoriteFood): void {
    this.favorites.update(items => items.map(item => item.food.id === id ? update(item) : item));
  }

  /**
   * Helper function to set busy food.
   * @param id ID ofa  food to set.
   * @param busy If is busy.
   */
  private setBusy(id: number, busy: boolean): void { busy ? this.addId(this.busyIds, id) : this.removeId(this.busyIds, id); }

  /**
   * Helper function to add ID of a food.
   * @param store Set of favorite foods.
   * @param id ID of a food to add.
   */
  private addId(store: WritableSignal<Set<number>>, id: number): void { store.update(ids => new Set(ids).add(id)); }

  /**
   * Helper function to remove ID of a food.
   * @param store Set of favorite foods.
   * @param id ID of a food to remove.
   */
  private removeId(store: WritableSignal<Set<number>>, id: number): void {
    store.update(ids => { const next = new Set(ids); next.delete(id); return next; });
  }

  /**
   * Helper function to toggle expanded food by its ID in a favorite food set.
   * @param store Set offavorite foods.
   * @param id ID of a food to toggle.
   */
  private toggleId(store: WritableSignal<Set<number>>, id: number): void {
    store.update(ids => { const next = new Set(ids); next.has(id) ? next.delete(id) : next.add(id); return next; });
  }
}
