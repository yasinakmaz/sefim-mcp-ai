# Şefim Knowledge Authoring

`private/` altındaki dosyalar business bilgisinin source halidir ve git'e eklenmez. Sadece `templates/` dosyalarını kopyalayın, `<!-- USER: ... -->` alanlarını doldurun, ardından doğrulayıp şifreli paketi üretin.

```bash
export SEFIM_KNOWLEDGE_KEY="$(openssl rand -base64 32)"
dotnet run --project sefim-ai-mcp -- knowledge validate
dotnet run --project sefim-ai-mcp -- knowledge pack
```

`knowledge.pack` release ile dağıtılabilir; private Markdown dağıtıma eklenmez. Key, deployment sırasında process environment veya işletim sistemi secret store üzerinden sağlanmalıdır.
