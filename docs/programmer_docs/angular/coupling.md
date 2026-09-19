[Programmer documentation](../README.md)

# Angular coupling

Usually each component consists of `.css` `.html` `.ts` file.

Angular is an API client, not a database client.

Main coupling points:
- `food.service.ts`: endpoint URLs and TypeScript food model
- `auth.service.ts`: auth endpoints and `currentUser` signal
- `auth.interceptor.ts`: sends credentials for cookie auth
- `auth.guard.ts`: protects UI routes
- `app.routes.ts`: page routes
- `nginx`/development proxy: forwards `/api` to the API.

- When API request/response shape changes:
    - Chagne `C#` contract/controller, matching `Angular` service/components.
    - Keep browser calls rooted at `/api` (so proxy can hide container addresses).
