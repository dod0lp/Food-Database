# User docs

## Using Food Database

Food Database is a catalogue of foods, their nutritional values, and meals
made from those foods. *Food* can mean an individual ingredient, or a complete meal.
A meal is a composite food: it is made from
other foods, can also be composite foods. Nutritional values are
shown and stored per 100g unless a saved package size is being displayed.

### Browse foods

The home page lists system foods. Open a food to see its description and
nutritional values. When the food is a meal, its ingredients are listed too.
Each ingredient links to its own page. Use the page controls when the
catalogue contains more foods than fit on one page.

You can browse the public catalogue without an account. Creating foods,
marking favorites, and using the personal **My foods** and **Favorites** pages
require an account.

### Create an account and sign in

Choose **Register**, enter an email address and password, then sign in from
the **Sign in** page. Once signed in, there also are links to **Favorites**,
**My foods**, and **Create food**. You can also sign out.

### Keep favorites, notes, package sizes, and prices

Open a food and select **Add to favorites**. Then open **Favorites** and
expand the food to manage your information about that food:

- Add a private remark, (maybe shopping or recipe notes, etc).
- Add one or more package sizes in grams.
  - Optionally add a price for each package size.

For every saved package size, the page calculates nutrition for that package.
When a price is available, it also calculates scaled price per 100 g.
Package weights must be greater than zero.
A food can have several package sizes, and each package price may differ.

Removing a favorite can either keep or delete your personal remark and package
sizes for it, according to the selection at the top of the Favorites page. 
The food itself stays in the shared database. Your favorites, notes, and options are
private and are not visible to other users.

### Create a food or meal

Select **Create food**.
A new food is automatically placed in your favorites.

For a **simple food**, provide a name, optional description, and any known
nutrient values. Enter values per 100 g.
You can leave blank input when it is unknown. Food name is needed.

For a **meal / composite food**, you need to find ID of the foods you want to
use -- by opening them in the catalogue or My foods list. Enter the meal name and
description, then add each food ID and the actual number of grams used in the
meal/recipe.
The app combines the ingredient weights and nutrients,
saves the result as a food normalized to 100 g, and shows the ingredient list on the
meal's detail page. Composite foods can be used as ingredients in other foods.

Your created items are available in **My foods**.
Anyone can access them, but only with link you send them (ID for a food).