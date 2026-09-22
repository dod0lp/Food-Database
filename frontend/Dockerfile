FROM node:22-alpine AS build
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci
COPY . ./
RUN npm run build

FROM nginx:1.27-alpine
ARG API_PORT=8080
ARG WEB_PORT=4200
ENV API_PORT=${API_PORT}
ENV WEB_PORT=${WEB_PORT}
COPY nginx.conf /etc/nginx/templates/default.conf.template
COPY --from=build /app/dist/food-database-web/browser /usr/share/nginx/html
EXPOSE ${WEB_PORT}
