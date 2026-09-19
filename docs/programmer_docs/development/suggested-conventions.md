[Programmer documentation](../README.md)

# Conventions

- Use `Async` suffixes for async operations. (C# convention)
- Use `AsNoTracking` for read-only EF queries. (EF "convention")
- Pass `HttpContext.RequestAborted` from controllers to repository methods. (ASP.NET convention CancellationToken)
- Treat composite foods as ordinary `Food` plus "ingredients".
  - By current logic, it is possible, just "recursively" look up ingredients.
- Document a new rule near its repository method and update user stories.
- It would be more safe to try to have operations atomic...
  - It's possible to use not .id properties, but EF object itself...
    - But for working with domain food multiple saves are needed for a generated ID, so it is not really working here.