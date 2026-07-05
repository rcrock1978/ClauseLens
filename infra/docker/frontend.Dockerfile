# syntax=docker/dockerfile:1.7

# Multi-stage build for the Angular frontend (PWA-ready).
FROM node:22-alpine AS build
WORKDIR /app
COPY frontend/package*.json ./
RUN npm ci
COPY frontend/ ./
RUN npm run build -- --configuration=production

FROM nginx:1.27-alpine AS runtime
COPY --from=build /app/dist/clauselens-web /usr/share/nginx/html
COPY infra/docker/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
