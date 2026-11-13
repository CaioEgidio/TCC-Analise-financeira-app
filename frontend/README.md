# FinData — frontend

Aplicação React + TypeScript + Vite. Consulte o [README principal](../README.md) para configuração do banco, API, arquitetura e testes.

Nesta pasta:

```powershell
npm ci
npm run dev
```

Abra `http://localhost:5173`, com a API em `http://localhost:5068`. O Vite encaminha `/api` para ela.

```powershell
npm run lint
npm run build
```

O build gera `dist/`. O proxy é exclusivo do desenvolvimento: publicar essa pasta exige configurar o encaminhamento de `/api` na hospedagem.
