##
##
## NPM build and restore
##
FROM node:12 AS nodebuild
WORKDIR /src
COPY ./dungen-site/ClientApp ./
RUN npm install --progress=true
RUN npm run build

##
##
## Build the dungen-site app
##
##
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY . .
# NuGet restore as a distinct step
RUN dotnet restore -f -r linux-musl-x64
# Build the actual app and its dependencies
RUN dotnet publish --no-restore -c Release -r linux-musl-x64 -o /out

##
##
## Copy artifacts and complete deployment
##
##
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS runtime
WORKDIR /app

#
# Install Heroku requisites
#
RUN apk add --no-cache curl

#
# Install .NET Core requisites
#
# System.Drawing requires libgdi to render images to Bitmap
RUN apk add libgdiplus --update-cache --repository=http://dl-cdn.alpinelinux.org/alpine/edge/testing/ --allow-untrusted

#
# Install NuGet package requisites
#
# Magick.NET requires glibc to function on Linux, which Alpine doesn't include
RUN apk --no-cache add ca-certificates wget
RUN wget -q -O /etc/apk/keys/sgerrand.rsa.pub https://alpine-pkgs.sgerrand.com/sgerrand.rsa.pub && \
    wget https://github.com/sgerrand/alpine-pkg-glibc/releases/download/2.28-r0/glibc-2.28-r0.apk && \
    apk add glibc-2.28-r0.apk
#
# Copy artifacts into published runtime fs
COPY --from=build /out .
COPY --from=nodebuild /src/dist /app/ClientApp/dist
ENV ASPNETCORE_URLS=http://*:$PORT
CMD [ "/app/DunGen.Site" ]
