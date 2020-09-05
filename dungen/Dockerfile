##
## NPM build and restore
##
FROM node:12 AS nodebuild
WORKDIR /src
COPY ./dungen-site/ClientApp ./
RUN npm install --progress=true
RUN npm run build

##
## Build the dungen-site app
##
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY . .
# NuGet restore as a distinct step
RUN dotnet restore -f -r linux-musl-x64
# Build the actual app and its dependencies
RUN dotnet publish --no-restore -c Release -r linux-musl-x64 -o /out

##
## Copy artifacts and complete deployment
##
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS runtime
WORKDIR /app
# Install Heroku requisites
RUN apk add --no-cache curl
# Copy artifacts into published runtime fs
COPY --from=build /out .
COPY --from=nodebuild /src/dist /app/ClientApp/dist
ENV ASPNETCORE_URLS=http://*:$PORT
CMD [ "/app/dungen-site" ]