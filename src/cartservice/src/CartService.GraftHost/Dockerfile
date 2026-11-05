FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /usr/src

ARG BUILD_CONFIGURATION=Release

COPY . ./
RUN dotnet restore ./CartService.GraftHost/CartService.GraftHost.csproj --configfile ./NuGet.Config
RUN dotnet publish ./CartService.GraftHost/CartService.GraftHost.csproj -c ${BUILD_CONFIGURATION} --no-restore -o /usr/src/publish

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /usr/app

RUN apt-get update && \
    apt-get install -y --no-install-recommends ca-certificates curl tar libcap2-bin libxml2 openssl libxmlsec1 && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build-env /usr/src/publish ./dll/

RUN curl -fsSL -o gg.deb "https://github.com/grft-dev/graftcode-gateway/releases/latest/download/gg_linux_amd64.deb" \
    && dpkg -i gg.deb \
    && rm gg.deb

RUN groupadd -r appuser && useradd -r -g appuser appuser && chown -R appuser:appuser /usr/app
RUN setcap 'cap_net_bind_service=+ep' "$(command -v gg)"

WORKDIR /usr/app/dll
USER appuser

EXPOSE 80
EXPOSE 81

CMD ["gg", "--modules", "cartservice.dll", "--types", "cartservice.services.CartService", "--methods", "AddItem,GetCart,EmptyCart"]
