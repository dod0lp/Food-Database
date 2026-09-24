/**
 * Interface representing food with ID, name, weight, description, and its nutrients.
 */
export interface Food {
  id: number;
  name: string;
  weight: number;
  description: string;

  nutrientContent: {
    energy: { kcal: number };
    fatContent: { total: number; saturated: number };
    carbohydrateContent: { total: number; sugar: number };
    protein: { total: number };
    salt: { total: number };
  };

  ingredients: Food[];
}

/**
 * Favorite food with remarks and options.
 */
export interface FavoriteFood {
  food: Food;
  remark: string | null;
  options: { weight: number; price: number | null }[];
}

/**
 * Food page for showing mainly.
 */
export interface FoodPage {
  items: Food[];
  total: number;
}

/**
 * Type representing nutrient row with name of nutrient, value, and unit.
 */
export type NutrientRow = {
  label: string;
  value: number;
  unit: string;
}

/**
 * Builds nutrient rows for a food, scaled to the requested weight.
 * @param food Food containing the base nutrient values.
 * @param targetWeight Weight for which nutrient values should be calculated.
 * @returns Nutrient rows scaled to the target weight.
 */
export function getNutritionRows(food: Food, targetWeight = food.weight): NutrientRow[] {
  const factor = food.weight > 0 ? targetWeight / food.weight : 1;

  return [
    { label: 'Energy', value: food.nutrientContent.energy.kcal, unit: 'kcal' },
    { label: 'Fat', value: food.nutrientContent.fatContent.total, unit: 'g' },
    { label: 'Saturated fat', value: food.nutrientContent.fatContent.saturated, unit: 'g' },
    { label: 'Carbs', value: food.nutrientContent.carbohydrateContent.total, unit: 'g' },
    { label: 'Sugar', value: food.nutrientContent.carbohydrateContent.sugar, unit: 'g' },
    { label: 'Protein', value: food.nutrientContent.protein.total, unit: 'g' },
    { label: 'Salt', value: food.nutrientContent.salt.total, unit: 'g' }
  ].map(nutrient => ({
    ...nutrient,
    value: Number.isFinite(nutrient.value) && nutrient.value >= 0
      ? nutrient.value * factor
      : -1
  }));
}

/**
 * Calculates scaled nutrient rows (when the requested weight is valid).
 * @param food Food containing the base nutrient values.
 * @param targetWeight Requested portion weight.
 * @returns Scaled rows, or null when the requested weight is invalid.
 */
export function getScaledNutritionRows(
  food: Food,
  targetWeight: number | null | undefined
): NutrientRow[] | null {
  return (typeof targetWeight === 'number' && Number.isFinite(targetWeight) && targetWeight > 0)
    ? getNutritionRows(food, targetWeight)
    : null;
}

/**
 * Class providing formatting of units for food.
 */
export class FoodFormatter {
  /**
   * Helper function to format nutrient value and unit.
   * @param nutrient Nutrinet to format.
   * @returns Formatted string value from nutrient.
   */
  formatNutrientValue(nutrient: NutrientRow): string {
    return Number.isFinite(nutrient.value) && nutrient.value >= 0
      ? `${this.formatDecimal(nutrient.value)} ${nutrient.unit}`
      : 'Unknown';
  }

  /**
   * Helpre function to format decimal number.
   * @param value Value to format.
   * @param minimumFractionDigits Minimum fraction digits.
   * @param maximumFractionDigits Maximum fraction digits.
   * @returns Formatted decimal to max number of digits.
   */
  formatDecimal(value: number, minimumFractionDigits = 0, maximumFractionDigits = 2): string {
    return new Intl.NumberFormat('sk-SK', { minimumFractionDigits, maximumFractionDigits }).format(value);
  }

  /**
   * Formats price of favorite food remakr.
   * @param value Value to format.
   * @returns Formatted value in EUR form.
   */
  formatPrice(value: number): string {
    return `${this.formatDecimal(value, 2, 2)}€`;
  }
}
