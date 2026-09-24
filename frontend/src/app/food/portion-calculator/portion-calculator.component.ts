import { Component, computed, input, model } from '@angular/core';
import { FormsModule } from '@angular/forms';

/**
 * Reusable input for selecting a food portion weight.
 */
@Component({
  selector: 'app-portion-calculator',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './portion-calculator.component.html',
  styleUrl: './portion-calculator.component.css'
})
export class PortionCalculatorComponent {
  readonly foodId = input<number>();
  readonly weight = model<number | null>(null);

  readonly inputName = computed(
    () => `portionWeight${this.foodId() ?? ''}`
  );
}