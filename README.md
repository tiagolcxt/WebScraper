### \# 🍄 PubMed Scientific WebScraper (2026 Edition)



Sistema de extração de dados científicos de alta performance, projetado para realizar pesquisas automatizadas no PubMed sobre compostos fúngicos. O motor utiliza processamento paralelo (Multithreading) com camuflagem de identidade para garantir o máximo de acurácia em extrações de larga escala.



#### \##Diferenciais Técnicos



\* \*\*Thread-Safe Identity:\*\* Rotação de `User-Agents` e `Referers` através de `HttpRequestMessage`, eliminando erros de colisão de cabeçalhos em ambientes paralelos.

\* \*\*Orquestração Inteligente:\*\* Gerenciamento de carga com `Parallel.ForEachAsync`, configurado para otimizar o throughput sem disparar gatilhos de segurança (WAF) do PubMed.

\* \*\*Telemetria Avançada:\*\* Dashboard em tempo real com cálculo de \*\*ETA Pessimista\*\* (margem de 15%) e relatório de auditoria de falhas agrupado por motivo técnico.

\* \*\*Arquitetura SOLID:\*\* Total desacoplamento entre navegação, extração, validação e telemetria, facilitando a manutenção e expansão para outras fontes (ex: Google Scholar).



---



#### \##Estrutura do Projeto



| Componente | Responsabilidade |

| --- | --- |

| \*\*`IdentityService`\*\* | Gerencia a "máscara" HTTP (Headers) de forma isolada por thread. |

| \*\*`PubMedNavigator`\*\* | Responsável pela descoberta e limpeza de URLs de pesquisa. |

| \*\*`PubMedParser`\*\* | Extrai e sanitiza os dados brutos (Título, Data, Abstract) do HTML. |

| \*\*`PubMedValidator`\*\* | Auditor de qualidade que filtra captchas ou falhas de rede. |

| \*\*`ScraperOrchestrator`\*\* | O "Maestro" que coordena o fluxo paralelo e a telemetria. |

| \*\*`ScrapeTelemetry`\*\* | Coleta métricas, calcula velocidade e gera o relatório de auditoria. |



---



#### Como Executar



1\. \*\*Pré-requisitos:\*\* .NET 8.0 ou superior.

2\. \*\*Configuração:\*\* No arquivo `Program.cs`, defina a lista de termos (cogumelos) e o range de datas desejado.

3\. \*\*Execução:\*\*

```bash

dotnet run



```

#### Dashboard e Relatórios



Durante a execução, o sistema exibe um progresso dinâmico:

`Progresso: 142/200 | Acurácia: 98.5% | ETA Pessimista: 0m 45s`



Ao final, é gerado um relatório segmentado por categoria:



\* \*\*Separação por Bloco:\*\* Resultados agrupados pelo termo de busca (ex: Reishi, Lion's Mane).

\* \*\*Análise de Performance:\*\* Comparação entre o Tempo Real vs. Tempo Estimado.

\* \*\*Auditoria de Erros:\*\* Lista detalhada de links que falharam e o motivo técnico (ex: 403 Forbidden ou Título Inválido).



---



#### Tratamento de Erros (Resiliência)



O sistema foi blindado contra os erros críticos de concorrência:



\* ✅ \*\*InvalidCastException:\*\* Resolvido movendo headers de `DefaultRequestHeaders` para mensagens locais.

\* ✅ \*\*NullReferenceException:\*\* Eliminado através da imutabilidade dos objetos de requisição.

\* ✅ \*\*Bloqueios de IP:\*\* Mitigados com delays randômicos entre  e .



---





