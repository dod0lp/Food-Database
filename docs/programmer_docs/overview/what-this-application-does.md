[Programmer documentation](../README.md)

# What this application does

The application stores foods and their nutrients.\
The food is actually an ingredient here, ingredients can be made out of other ingredients.\
I will use those terms interchangeably.\
A food may be a simple food, or a composite food (a meal) made from other foods (or "ingredients").\
A composite food can use another composite food as an ingredient.

[User stories](../../user%20stories.md)

All stored nutrient values and ingredient proportions are normalized to **100g**.

The shared C# project is like a domain.\
The API exposes domain rules over HTTP and Angular calls the API.\
Do not put calculation rules only in a controller or only in Angular, because then console and web callers diverge (it's not a bad design, developer just have to be careful).