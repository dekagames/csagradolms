# Estágio 1: Build - Compila a aplicação
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia e restaura dependências
COPY ["BlazorApp/BlazorApp.csproj", "BlazorApp/"]
RUN dotnet restore "BlazorApp/BlazorApp.csproj"

# Copia todo o código e publica
COPY . .
WORKDIR "/src/BlazorApp"
RUN dotnet publish "BlazorApp.csproj" -c Release -o /app/publish

# Estágio 2: Runtime - Configura o Nginx
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Copia apenas os arquivos estáticos publicados
COPY --from=build /app/publish/wwwroot .

# Configuração customizada do Nginx para Blazor WASM
COPY BlazorApp/nginx.conf /etc/nginx/nginx.conf

# Expõe a porta 80 (HTTP) e 443 (HTTPS - opcional)
EXPOSE 80
EXPOSE 443

# Entrypoint padrão do Nginx (não precisa ser declarado)