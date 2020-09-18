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
RUN dotnet restore -f -r linux-musl-x64 -v normal
# Build the actual app and its dependencies
RUN dotnet publish --no-restore -c Release -r linux-musl-x64 -o /out -v normal

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
# Why the fuck isn't this on the image
    bash \
# Install Heroku requisites
    curl \
# Install .NET Core requisites
# System.Drawing requires libgdi to render images to Bitmap
    libgdiplus \
# Install NuGet package requisites
# Magick.NET glibc6 to render GIF images
    libc6 \
    musl-dev\
    libstdc++6 \
    gcc \
# Minimize squandered space
 && rm -rf /var/lib/apt/lists/*

#RUN ln -s /bin/ldd ld-musl-x86_64.so
#RUN ln -s /lib64/ld-linux-x86-64.so.2 /lib/ld-linux-x86-64.so.2
RUN ln -s /usr/lib/x86_64-linux-musl/libc.so /lib/libc.musl-x86_64.so.1
#
# Copy artifacts into published runtime fs
#
COPY --from=build /out .
COPY --from=nodebuild /src/dist /app/ClientApp/dist
ENV ASPNETCORE_URLS=http://*:$PORT
ENV LD_DEBUG=bindings,files,libs
ENTRYPOINT [ "/bin/bash", "-l", "-c" ]
CMD [ "exec", "/app/DunGen.Site" ]
