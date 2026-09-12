import { Component, inject, OnInit, signal, WritableSignal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { FavoriteFood, Food, FoodService } from './food.service';

interface NutrientRow {
  label: string;
  value: number;
  unit: string;
}

@Component({
  imports: [FormsModule, RouterLink],
  template: `
    <section class="card favorites-card">
      <div class="favorites-heading">
        <div>
          <h1>My favorites</h1>
          <p class="muted">Open a food to view or change its personal details.</p>
        </div>
        <aside class="removal-settings" aria-label="Favorite removal preference">
          <span>When removing a favorite</span>
          <label><input type="radio" name="removeData" value="keep" [(ngModel)]="removeDataChoice"> Keep my details</label>
          <label><input type="radio" name="removeData" value="delete" [(ngModel)]="removeDataChoice"> Delete my details too</label>
        </aside>
      </div>

      @if (loading()) { <p>Loading favorites...</p> }
      @if (error()) { <p class="error">{{ error() }}</p> }
      @if (message()) { <p class="message">{{ message() }}</p> }
      @if (!loading() && !favorites().length) { <p>You have no favorites yet.</p> }

      <div class="favorite-list">
        @for (favorite of favorites(); track favorite.food.id) {
          <article class="favorite-item">
            <div class="favorite-name" (click)="toggleExpanded(favorite.food.id)">
              <button class="expander" type="button" (click)="$event.stopPropagation(); toggleExpanded(favorite.food.id)"
                [attr.aria-expanded]="isExpanded(favorite.food.id)"
                [attr.aria-label]="isExpanded(favorite.food.id) ? 'Hide details for ' + favorite.food.name : 'Show details for ' + favorite.food.name">
                {{ isExpanded(favorite.food.id) ? '-' : '+' }}
              </button>
              <a [routerLink]="['/foods', favorite.food.id]" (click)="$event.stopPropagation()">{{ favorite.food.name }}</a>
            </div>

            @if (isExpanded(favorite.food.id)) {
              <div class="favorite-details">
                @if (nutritionRows(favorite.food).length) {
                  <section class="detail-section nutrition-section">
                    <div class="detail-title"><h2>Nutrients per {{ favorite.food.weight }} g</h2></div>
                    <dl class="nutrients">
                      @for (nutrient of nutritionRows(favorite.food); track nutrient.label) {
                        <dt>{{ nutrient.label }}</dt><dd>{{ formatDecimal(nutrient.value) }} {{ nutrient.unit }}</dd>
                      }
                    </dl>
                  </section>
                }
                <section class="detail-section">
                  <div class="detail-title">
                    <h2>Remark</h2>
                    @if (!isEditingRemark(favorite.food.id)) {
                      <button type="button" class="text-button" (click)="openRemarkEditor(favorite)">{{ hasRemark(favorite) ? 'Edit remark' : '+ Add remark' }}</button>
                    }
                  </div>
                  @if (hasRemark(favorite) && !isEditingRemark(favorite.food.id)) { <p class="remark">{{ favorite.remark }}</p> }
                  @if (isEditingRemark(favorite.food.id)) {
                    <label class="editor-label">Your remark
                      <textarea [name]="'remark' + favorite.food.id" [(ngModel)]="remarkDrafts[favorite.food.id]"></textarea>
                    </label>
                    <div class="actions">
                      <button type="button" (click)="saveRemark(favorite)" [disabled]="isBusy(favorite.food.id)">Save remark</button>
                      <button type="button" class="text-button" (click)="closeRemarkEditor(favorite.food.id)">Cancel</button>
                    </div>
                  }
                </section>

                <section class="detail-section">
                  <div class="detail-title">
                    <h2>Package sizes</h2>
                    @if (!isAddingOption(favorite.food.id)) {
                      <button type="button" class="text-button" (click)="openOptionEditor(favorite.food.id)">+ Add package size</button>
                    }
                  </div>
                  @if (favorite.options.length) {
                    <div class="package-table">
                      <div class="package-table-heading" aria-hidden="true">
                        <span>Package</span><span>Price</span><span>Per 100 g</span><span></span>
                      </div>
                      @for (option of favorite.options; track option.weight) {
                        <div class="package-row">
                          <strong>{{ formatDecimal(option.weight) }} g</strong>
                          <span class="price">@if (option.price !== null) { {{ formatPrice(option.price) }} } @else { No price }</span>
                          <span>@if (option.price !== null) { {{ formatPrice(pricePer100g(option.price, option.weight)) }} } @else { - }</span>
                          <div class="option-actions">
                            <button type="button" class="text-button" (click)="openOptionEditor(favorite.food.id, option.weight, option.price)">Edit</button>
                            <button type="button" class="text-button remove-option" (click)="removeOption(favorite, option.weight)" [disabled]="isBusy(favorite.food.id)">Remove</button>
                          </div>
                          @if (nutritionRows(favorite.food, option.weight).length) {
                            <div class="package-nutrients"><strong>Nutrients for this {{ formatDecimal(option.weight) }} g package</strong>
                              @for (nutrient of nutritionRows(favorite.food, option.weight); track nutrient.label; let last = $last) { <span>{{ nutrient.label }} {{ formatDecimal(nutrient.value) }} {{ nutrient.unit }}@if (!last) { | }</span> }
                            </div>
                          }
                        </div>
                      }
                    </div>
                  }
                  @if (isAddingOption(favorite.food.id)) {
                    <div class="option-editor">
                      @if (isEditingOption(favorite.food.id)) {
                        <div class="locked-weight"><span>Package weight</span><strong>{{ formatDecimal(newWeights[favorite.food.id] ?? 0) }} g</strong></div>
                      } @else {
                        <label>Weight (g)<input [name]="'weight' + favorite.food.id" type="number" min="1" [(ngModel)]="newWeights[favorite.food.id]"></label>
                      }
                      <label>Price (optional)<input [name]="'price' + favorite.food.id" type="number" min="0" step="0.01" [(ngModel)]="newPrices[favorite.food.id]"></label>
                      <div class="actions">
                        <button type="button" (click)="addOption(favorite)" [disabled]="isBusy(favorite.food.id)">{{ isEditingOption(favorite.food.id) ? 'Save price' : 'Save size' }}</button>
                        <button type="button" class="text-button" (click)="closeOptionEditor(favorite.food.id)">Cancel</button>
                      </div>
                    </div>
                  }
                </section>

                <div class="remove-favorite">
                  <button type="button" class="danger" (click)="removeFavorite(favorite)" [disabled]="isBusy(favorite.food.id)">Remove from favorites</button>
                  <small>{{ removeDataChoice === 'keep' ? 'Your remark and package sizes will be kept.' : 'Your remark and package sizes will be deleted.' }}</small>
                </div>
              </div>
            }
          </article>
        }
      </div>
    </section>
  `,
  styles: `
    .favorites-heading { display:flex; align-items:start; justify-content:space-between; gap:1.5rem; margin-bottom:1.25rem; }
    h1, h2 { margin:0; } h2 { font-size:1rem; }
    .muted, small { color:#64748b; } .muted { margin:.35rem 0 0; }
    .removal-settings { min-width:14rem; border:1px solid #d9e1ea; border-radius:.55rem; padding:.65rem .8rem; background:#f8fafc; font-size:.85rem; }
    .removal-settings span { display:block; font-weight:650; margin-bottom:.35rem; } .removal-settings label { display:block; margin:.3rem 0; }
    .favorite-list { border-top:1px solid #e2e8f0; }
    .favorite-item { border-bottom:1px solid #e2e8f0; }
    .favorite-name { display:flex; align-items:center; gap:.65rem; min-height:3.25rem; font-size:1.05rem; font-weight:600; cursor:pointer; }
    .expander { width:1.75rem; height:1.75rem; padding:0; border:1px solid #b9c7d6; border-radius:50%; background:white; color:#075eab; font-size:1.25rem; line-height:1; cursor:pointer; }
    .expander:hover { background:#eff6ff; } .favorite-details { padding:0 .15rem 1.25rem 2.4rem; }
    .detail-section { padding:.9rem 0; border-top:1px solid #edf1f5; } .detail-title { display:flex; align-items:center; justify-content:space-between; gap:1rem; }
    .nutrients { display:grid; grid-template-columns:repeat(2, max-content); gap:.25rem .7rem; margin:.65rem 0 0; } .nutrients dt { font-weight:600; } .nutrients dd { margin:0; }
    .remark { white-space:pre-wrap; margin:.65rem 0 0; color:#334155; }
    .text-button { border:0; padding:.2rem; background:transparent; color:#075eab; text-decoration:underline; cursor:pointer; }
    .text-button:disabled { color:#94a3b8; cursor:default; } .editor-label { display:grid; gap:.35rem; max-width:34rem; margin-top:.65rem; }
    textarea { min-height:5rem; resize:vertical; } .actions { display:flex; align-items:center; gap:.7rem; margin-top:.6rem; }
    .package-table { margin-top:.7rem; border:1px solid #dce5ee; border-radius:.5rem; overflow:hidden; max-width:48rem; }
    .package-table-heading, .package-row { display:grid; grid-template-columns:1.1fr 1fr 1fr auto; gap:.75rem; align-items:center; }
    .package-table-heading { padding:.5rem .7rem; background:#f1f5f9; color:#475569; font-size:.78rem; font-weight:700; text-transform:uppercase; letter-spacing:.03em; }
    .package-row { padding:.7rem; border-top:1px solid #dce5ee; } .package-row > strong { font-size:1.05rem; color:#172554; }
    .price { font-weight:700; color:#12633d; } .option-actions { display:flex; gap:.45rem; white-space:nowrap; }
    .package-nutrients { grid-column:1 / -1; border-top:1px dashed #dce5ee; padding-top:.5rem; color:#475569; font-size:.86rem; line-height:1.65; }
    .package-nutrients strong { color:#334155; margin-right:.45rem; } .package-nutrients span { white-space:nowrap; }
    .remove-option { color:#a13a2a; } .option-editor { display:flex; flex-wrap:wrap; align-items:end; gap:.7rem; margin-top:.7rem; }
    .option-editor label, .locked-weight { display:grid; gap:.3rem; font-size:.9rem; } .locked-weight { padding:.38rem .6rem; min-width:7rem; border:1px solid #dce5ee; border-radius:.35rem; background:#f8fafc; } input { max-width:11rem; }
    .remove-favorite { display:flex; align-items:center; gap:.75rem; flex-wrap:wrap; margin-top:.4rem; } .danger { color:#a13a2a; border-color:#e7b6ae; background:#fff7f5; }
    .message { color:#12633d; }
    @media (max-width: 600px) { .favorites-heading { display:block; } .removal-settings { margin-top:1rem; } .favorite-details { padding-left:0; } .package-table-heading { display:none; } .package-row { grid-template-columns:1fr auto; } .package-row > :nth-child(2), .package-row > :nth-child(3) { font-size:.9rem; } .package-nutrients { grid-column:1 / -1; } }
  `
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
  readonly editingOptionWeights: Record<number, number | null> = {};
  readonly busyIds = signal<Set<number>>(new Set());
  readonly newWeights: Record<number, number | null> = {};
  readonly newPrices: Record<number, number | null> = {};
  readonly remarkDrafts: Record<number, string> = {};
  removeDataChoice: 'keep' | 'delete' = 'keep';

  ngOnInit(): void { this.load(); }
  isExpanded(id: number): boolean { return this.expandedIds().has(id); }
  isEditingRemark(id: number): boolean { return this.remarkEditorIds().has(id); }
  isAddingOption(id: number): boolean { return this.optionEditorIds().has(id); }
  isEditingOption(id: number): boolean { return this.editingOptionWeights[id] !== null && this.editingOptionWeights[id] !== undefined; }
  isBusy(id: number): boolean { return this.busyIds().has(id); }
  hasRemark(favorite: FavoriteFood): boolean { return !!favorite.remark?.trim(); }
  toggleExpanded(id: number): void { this.toggleId(this.expandedIds, id); }

  nutritionRows(food: Food, targetWeight = food.weight): NutrientRow[] {
    const factor = food.weight > 0 ? targetWeight / food.weight : 1;
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
      next: () => { this.updateFavorite(id, current => ({ ...current, options: current.options.filter(x => x.weight !== weight) })); this.message.set('Package size removed.'); this.setBusy(id, false); },
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
  private updateFavorite(id: number, update: (favorite: FavoriteFood) => FavoriteFood): void { this.favorites.update(items => items.map(item => item.food.id === id ? update(item) : item)); }
  private setBusy(id: number, busy: boolean): void { busy ? this.addId(this.busyIds, id) : this.removeId(this.busyIds, id); }
  private addId(store: WritableSignal<Set<number>>, id: number): void { store.update(ids => new Set(ids).add(id)); }
  private removeId(store: WritableSignal<Set<number>>, id: number): void { store.update(ids => { const next = new Set(ids); next.delete(id); return next; }); }
  private toggleId(store: WritableSignal<Set<number>>, id: number): void { store.update(ids => { const next = new Set(ids); next.has(id) ? next.delete(id) : next.add(id); return next; }); }
}
