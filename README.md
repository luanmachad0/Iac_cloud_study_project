# Sports Betting MVP

MVP fullstack para portfolio com foco em entrega rapida: API .NET, frontend React, Docker e CI/CD para deploy em EC2.

## Stack atual

- Backend: .NET (Clean Architecture)
- Frontend: React + Vite
- Infra local: Docker Compose (API, Postgres, Redis, LocalStack)
- CI/CD: GitHub Actions (CI + deploy em EC2 via SSH)
- Observabilidade: Serilog JSON, `/health` e `/metrics`

## Fluxo de demo (entrevista)

- Criar aposta: `POST /api/bets`
- Liquidar aposta: `POST /api/bets/{betId}/settle`
- Consultar resultado: `GET /api/bets/{betId}/result`

## Rodando local

### 1) Backend com dependencias

```bash
docker compose up -d --build
```

API: `http://localhost:8080`

### 2) Frontend React

```bash
cd frontend
npm install
npm run dev
```

Frontend: `http://localhost:5173`

## CI/CD (GitHub Actions)

O workflow `.github/workflows/ci-cd.yml` faz:

- CI no push/PR:
  - restore + testes do backend
  - build da imagem da API
  - build do frontend React
- CD no push em `main`:
  - conecta no EC2 via SSH
  - executa `git pull`
  - executa `docker compose -f docker-compose.ec2.yml up -d --build`

## Checklist de deploy rapido no EC2 (Free Tier)

### 1) Criar instancia

- Criar EC2 Ubuntu `t2.micro` ou `t3.micro`
- Liberar inbound:
  - `22` (SSH)
  - `80` (API)

### 2) Preparar servidor

```bash
sudo apt-get update -y
sudo apt-get install -y docker.io docker-compose-plugin git
sudo usermod -aG docker $USER
```

Depois, reconecte por SSH.

### 3) Clonar projeto no EC2

```bash
git clone <URL_DO_REPO> app
cd app
docker compose -f docker-compose.ec2.yml up -d --build
```

### 4) Configurar secrets no GitHub

- `EC2_HOST`: IP publico da instancia
- `EC2_USER`: usuario SSH (ex.: `ubuntu`)
- `EC2_PORT`: `22`
- `EC2_SSH_KEY`: chave privada da instancia
- `EC2_APP_PATH`: caminho do projeto no servidor (ex.: `/home/ubuntu/app`)

### 5) Validar

- API health: `http://SEU_IP/health`
- API metrics: `http://SEU_IP/metrics`
- Front local consumindo API remota:
  - `frontend/.env` com `VITE_API_BASE_URL=http://SEU_IP`
  - `npm run dev`

## Proximos passos

- Front em S3 + CloudFront
- Deploy container da API em ECS/Fargate ou EKS
- OpenTelemetry + Grafana
- Evolucao para microservicos
