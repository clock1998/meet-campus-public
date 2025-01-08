FROM node:20-alpine AS build

WORKDIR /app
COPY package.json package-lock.json /app/
RUN npm install -g @angular/cli@latest
RUN npm ci
COPY . /app
EXPOSE 5173
CMD [ "ng","serve", "--host", "0.0.0.0", "--poll", "100" ]

# RUN ulimit -c unlimited
# docker build -t meet-campus-client-dev:latest .
# docker run -p 5173:5173 (Attached mode)
# docker rm
# docker container prune
# docker run -p 5173:5173 -v E:\Projects\campusmeet\client\src:/app/src -it --rm --name meet-campus-client-dev meet-campus-client-dev