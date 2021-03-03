FROM nginx:1.19.7-alpine

EXPOSE 8082

ADD ./nginx.conf /etc/nginx/conf.d/default.conf
ADD ./dist /var/www
