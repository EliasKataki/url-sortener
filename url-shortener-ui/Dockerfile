FROM node:18-alpine AS build
WORKDIR /app
COPY url-shortener-ui/package*.json ./
RUN npm install
COPY url-shortener-ui/ .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/build /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"] 