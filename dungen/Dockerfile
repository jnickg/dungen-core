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
RUN dotnet restore -f -r linux-x64 -v normal
# Build the actual app and its dependencies
RUN dotnet publish --no-restore -c Release -r linux-x64 -o /out -v normal

##
##
## Prepare runtime container
##
##
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
WORKDIR /app
#
# Install Requisite Programs & Libraries
#
RUN apt-get update && apt-get install -y \
    bash \
# Install Heroku requisites
    curl \
# Install .NET Core requisites
# System.Drawing requires libgdi to render images to Bitmap
    libgdiplus \
# Install NuGet package requisites
# Minimize squandered space
 && rm -rf /var/lib/apt/lists/*

#
# Copy artifacts into published runtime fs
#
COPY --from=build /out .
COPY --from=nodebuild /src/dist /app/ClientApp/dist
ENV ASPNETCORE_URLS=http://*:$PORT
ENV LD_DEBUG=bindings,files,libs
ENTRYPOINT [ "/bin/bash", "-l", "-c" ]
CMD [ "exec", "/app/DunGen.Site" ]
