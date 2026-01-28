# Configuração da Autenticação Google SSO

Este guia descreve como configurar a autenticação Google SSO para o projeto EirMed.

## Pré-requisitos

### 1. Criar Projeto no Google Cloud Console

1. Acesse [Google Cloud Console](https://console.cloud.google.com/)
2. Crie um novo projeto ou selecione um existente
3. Vá para "APIs & Services" > "Credentials"
4. Clique em "Create Credentials" > "OAuth 2.0 Client IDs"
5. Configure o consentimento OAuth se necessário
6. Selecione "Web application"
7. Adicione as seguintes URLs:
   - **Authorized JavaScript origins:**
     - `http://localhost:3000`
     - `http://localhost:5000`
   - **Authorized redirect URIs:**
     - `http://localhost:3000`
8. Copie o Client ID e Client Secret

### 2. Configurar o Backend

Edite o arquivo `src/EirMed.API/appsettings.Development.json`:

```json
{
  "Google": {
    "ClientId": "SEU_GOOGLE_CLIENT_ID",
    "ClientSecret": "SEU_GOOGLE_CLIENT_SECRET"
  }
}
```

### 3. Configurar o Frontend

Edite o arquivo `web/.env.local`:

```
NEXT_PUBLIC_API_URL=http://localhost:5000
NEXT_PUBLIC_GOOGLE_CLIENT_ID=SEU_GOOGLE_CLIENT_ID
```

### 4. Executar as Migrations

```bash
cd src/EirMed.API
dotnet ef database update
```

### 5. Iniciar a Aplicação

**Terminal 1 - Backend:**
```bash
cd src/EirMed.API
dotnet run
```

**Terminal 2 - Frontend:**
```bash
cd web
npm run dev
```

### 6. Testar

1. Acesse `http://localhost:3000`
2. Clique em "Entrar com Google"
3. Faça login com sua conta Google
4. Verifique se você é redirecionado para a página principal com seu perfil

## Endpoints da API

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/auth/google` | Login com Google (enviar `idToken` no body) |
| POST | `/api/auth/refresh` | Refresh do token (enviar `refreshToken` no body) |
| POST | `/api/auth/logout` | Logout (enviar `refreshToken` no body) [Requer Auth] |
| GET | `/api/auth/me` | Obter usuário atual [Requer Auth] |

## Estrutura de Tokens

- **Access Token:** JWT válido por 60 minutos (dev) / 15 minutos (prod)
- **Refresh Token:** Token opaco válido por 30 dias (dev) / 7 dias (prod)

## Solução de Problemas

### Erro "Value cannot be null (path1)"

Este erro indica um problema na configuração do NuGet no sistema. Tente:

1. Limpar o cache do NuGet: `dotnet nuget locals all --clear`
2. Verificar se existe um nuget.config global com configurações inválidas
3. Criar um nuget.config local na raiz do projeto

### Erro de CORS

Verifique se as origens estão configuradas em `appsettings.Development.json`:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "https://localhost:3000"
    ]
  }
}
```

### Token do Google inválido

1. Verifique se o Client ID está correto em ambos frontend e backend
2. Verifique se as origens autorizadas no Google Cloud Console incluem `http://localhost:3000`
