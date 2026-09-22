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
