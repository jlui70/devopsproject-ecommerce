# DevOps Project Ecommerce

> Evolução do projeto `not-so-simple-ecommerce` para demonstração de infraestrutura AWS com Terraform.

Este repositório contém a versão **adaptada** da aplicação original, com:

- **Novo nome:** `devopsproject-ecommerce` (prefixo `dpe` em containers/images)
- **Novo domínio:** `devopsproject.com.br` (local: `devopsproject.local`)
- **Novo frontend:** React + TypeScript com design profissional (sidebar, dashboard, tema customizado)
- **Backend:** todos os microserviços originais **sem alteração** — apenas referências de nome foram atualizadas nos arquivos Docker/Nginx

---

## 📂 Estrutura do Projeto

```
devopsproject-ecommerce/
├── nginx/
│   └── nginx.conf                  # Domínio: devopsproject.local (local) / ecommerce.devopsproject.com.br (produção)
├── docker-compose.yml              # API services (dpe.*)
├── docker-compose.infra.yml        # Infra: LocalStack, Terraform, Postgres, Nginx
├── docker-compose.workers.yml      # Workers: Invoice, Notificator
└── src/
    └── frontend/                   # Novo frontend React + TypeScript
        ├── Dockerfile
        ├── index.html
        ├── package.json
        ├── vite.config.ts
        ├── tsconfig.json
        └── src/
            ├── main.tsx            # Entry point com ThemeProvider
            ├── App.tsx             # Roteador com guard de autenticação
            ├── theme.ts            # Tema MUI customizado (azul profissional)
            ├── components/
            │   ├── layout/
            │   │   ├── Layout.tsx  # Shell com sidebar + topbar
            │   │   ├── Sidebar.tsx # Navegação lateral (dark)
            │   │   └── TopBar.tsx  # Barra superior
            │   └── ui/
            │       └── StatCard.tsx  # Card de métrica para o Dashboard
            ├── hooks/              # Todos os hooks de API (idênticos ao original)
            │   ├── useProduct.ts
            │   ├── useStock.ts
            │   ├── useOrder.ts
            │   └── useReport.ts
            ├── pages/
            │   ├── dashboard/      # NOVO: Dashboard com 4 métricas + links de serviços
            │   ├── login/          # Redesenhado: gradiente escuro + branding
            │   ├── product/        # CRUD de produtos (tabela aprimorada)
            │   ├── stock/          # CRUD de estoque
            │   ├── order/          # CRUD de pedidos (com badge de status)
            │   └── report/         # Relatório de vendas por produto
            ├── types/
            │   └── entities.ts     # Tipos TypeScript (StockEntity, ProductEntity, etc.)
            └── utils/
                └── HttpClient.ts   # Axios com interceptors JWT e redirect 401
```

---

## 🔄 O que mudou em relação ao original

| Item | Original | Novo |
|------|----------|------|
| Nome do projeto | `not-so-simple-ecommerce` | `devopsproject-ecommerce` |
| Prefixo containers | `nsse.*` | `dpe.*` |
| Nome da rede Docker | `nsse-network` | `dpe-network` |
| Prefixo imagens | `nsse/main`, `nsse/order`... | `dpe/main`, `dpe/order`... |
| Domínio local | `devopsnanuvem.internal:44300` | `devopsproject.local:44300` |
| Frontend | Simples (só navbar) | Layout com sidebar + Dashboard |
| Login | MUI padrão branco | Gradiente escuro + branding |
| Redirect 401 | Não implementado | Automático via interceptor Axios |
| TypeScript | Misto (.jsx/.tsx) | 100% TypeScript |
| Tema MUI | Padrão | Customizado (azul + tipografia Inter) |

---

## 🛠️ Configuração Local

### 1. DNS (hosts file)

Adicione ao seu `/etc/hosts` (Linux/Mac) ou `C:\Windows\System32\drivers\etc\hosts` (Windows):

```
127.0.0.1 devopsproject.local
```

### 2. Infra Stack

```bash
docker-compose -f docker-compose.infra.yml up -d
```

Containers criados:
- `dpe.localstack.internal` — AWS local (SQS, SES, SNS, S3)
- `dpe.terraform-init.internal` + `dpe.terraform-apply.internal` — Provisionamento
- `dpe.database.internal` — PostgreSQL 16
- `dpe.nginx.internal` — Proxy reverso na porta 44300

### 3. App Stack

```bash
docker-compose -f docker-compose.workers.yml -f docker-compose.yml up -d
```

### 4. Acesso

| URL | Serviço |
|-----|---------|
| `https://devopsproject.local:44300` | Frontend |
| `https://devopsproject.local:44300/main/swagger` | Main API |
| `https://devopsproject.local:44300/order/swagger` | Order API |
| `https://devopsproject.local:44300/identity/swagger` | Identity API |
| `https://devopsproject.local:44300/healthchecks/ui` | Health Checks |
| `https://devopsproject.local:44300/invoice/swagger` | Invoice Worker |
| `https://devopsproject.local:44300/notificator/swagger` | Notificator Worker |

---

## ☁️ Produção — devopsproject.com.br

Para expor a aplicação via AWS + seu domínio real, você precisará:

### CNAMEs sugeridos

| CNAME | Destino |
|-------|---------|
| `ecommerce.devopsproject.com.br` | ALB da AWS |
| `api.devopsproject.com.br` | ALB da AWS (opcional, unificado pelo nginx no ECS) |

### Fluxo de infraestrutura (Terraform)

```
Route53 (ecommerce.devopsproject.com.br)
    └─> ALB (HTTPS 443)
         └─> ECS Fargate
              ├─> nginx (proxy reverso)
              ├─> main-api
              ├─> order-api
              ├─> identity-api
              ├─> health-checker
              ├─> invoice-worker
              └─> notificator-worker
```

> O Terraform já presente no projeto (`iac/`) pode ser adaptado para criar esses recursos.

---

## 🎨 Novo Frontend — Decisões de Design

| Aspecto | Escolha |
|---------|---------|
| Framework | React 18 + TypeScript |
| Build tool | Vite 4 |
| UI Library | MUI v5 (mantido do original) |
| Data tables | material-react-table v1 (mantido) |
| HTTP client | Axios com interceptors JWT |
| Estado assíncrono | @tanstack/react-query v4 |
| Layout | Sidebar fixa 260px + TopBar 64px |
| Cores | Azul `#2563eb` (primária) + fundo escuro `#0f172a` (sidebar) |
| Fonte | Inter (Google Fonts) |
| Auth guard | Roteamento protegido no `App.tsx` |

### Páginas

- **Login** — Tela de fundo escuro com degradê, logo "D", form com ícones
- **Dashboard** — 4 cards de métricas + grade de links para os microserviços
- **Produtos** — Tabela CRUD com modal de criação
- **Estoque** — Tabela CRUD com seletor de produto
- **Pedidos** — Tabela CRUD com badge colorido de status
- **Relatórios** — Tabela somente-leitura com formatação de moeda

---

## ⚙️ Variáveis de Ambiente

Copie o `.env` original do projeto base e ajuste:

```env
# Certificados
CERTIFICATES_HOST_PATH=./certificates
CERTIFICATES_CONTAINER_PATH=/app/certificates
ASPNETCORE_KESTREL__CERTIFICATES__DEFAULT__PASSWORD=sua_senha

# Postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DB=devopsproject

# Nginx
NGINX_CONF_HOST_PATH=./nginx/nginx.conf
NGINX_CONF_CONTAINER_PATH=/etc/nginx/nginx.conf
NGINX_CERT_HOST_PATH=./certificates/nginx-certificate.crt
NGINX_CERT_CONTAINER_PATH=/etc/nginx/certificates/nginx-certificate.crt
NGINX_KEY_HOST_PATH=./certificates/signing-request.key
NGINX_KEY_CONTAINER_PATH=/etc/nginx/certificates/signing-request.key

# Terraform
TERRAFORM_HOST_PATH=./iac
TERRAFORM_CONTAINER_PATH=/workspace

# AWS (LocalStack)
AWS_CREDENTIALS_HOST_PATH=~/.aws
AWS_CREDENTIALS_CONTAINER_PATH=/root/.aws
```

---

## 📦 Instalando o Frontend (desenvolvimento local)

```bash
cd src/frontend
npm install
npm run dev
```

O frontend sobe em `http://localhost:3000` e faz proxy via nginx para as APIs.

---

## 🚀 Fluxo de teste

1. Crie um **Produto** (nome + preço)
2. Adicione **Estoque** para esse produto
3. Realize um **Pedido** consumindo o estoque
4. Veja o relatório em **Relatórios** com a receita calculada
5. O **Invoice Worker** gera a fatura via SQS/S3
6. O **Notificator Worker** envia a notificação via SES/SNS
