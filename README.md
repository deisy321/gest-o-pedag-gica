# TriadeLearn - Gestão Pedagógica Inteligente

O **TriadeLearn** é uma plataforma de gestão pedagógica desenvolvida em **Blazor Server**, focada na monitorização do progresso dos alunos através do modelo de competências: **Conhecimento, Aptidão e Competência**. 

O sistema utiliza Inteligência Artificial para fornecer feedback formativo em tempo real e análises preditivas para docentes, garantindo um acompanhamento contínuo e personalizado.

## 🏗️ Arquitetura do Software

O projeto segue o padrão de **Separação de Preocupações**, garantindo manutenibilidade e escalabilidade:

*   **Data**: Camada responsável pela persistência de dados utilizando **PostgreSQL** e as migrações via Entity Framework Core.
*   **Models**: Definição das entidades de domínio e estruturação da lógica de negócio.
*   **Services (O Motor)**: Camada onde reside a lógica complexa e a integração com APIs externas, destacando-se o `IAService`.
*   **Pages**: Interface de utilizador modular e segura, com acesso controlado por perfis (Admin, Professor, Aluno).

## 🤖 Mentor Pedagógico (IAService)

A inovação central do sistema é o seu serviço de IA, que atua em dois eixos fundamentais:

### 1. Eixo do Aluno (Feedback Formativo)
*   **Análise em Tempo Real**: O sistema analisa submissões (texto ou PDF) confrontando-as com os objetivos definidos pelo docente.
*   **Apoio ao Autoestudo**: Em vez de dar a resposta, a IA oferece sugestões construtivas para que o aluno possa iterar e melhorar o trabalho antes da entrega final.

### 2. Eixo do Professor (Business Intelligence)
*   **Diagnóstico Automático**: A IA processa dados de avaliação para gerar relatórios sobre o desempenho das turmas.
*   **Deteção de Padrões**: Identifica tendências de notas e alerta o professor sobre grupos ou turmas que necessitem de atenção imediata ou ajustes no plano de aula.

## 🛠️ Tecnologias Utilizadas

*   **Framework**: .NET 8 / Blazor Server.
*   **Base de Dados**: PostgreSQL.
*   **IA**: Groq API (Modelo Llama 3.1).
*   **Notificações**: Protocolo VAPID para notificações Web Push em tempo real.
*   **Segurança**: ASP.NET Core Identity.

## 🔒 Privacidade e Otimização

*   **Segurança de Dados**: O sistema foi desenhado para que identificadores sensíveis permaneçam no servidor; apenas o contexto pedagógico necessário é enviado para análise externa.
*   **Performance**: Implementação de cache inteligente para evitar chamadas redundantes à API, garantindo respostas rápidas e baixo consumo de recursos.

---
Desenvolvido por **Abdaisy Conceição** como projeto de Prova de Aptidão Profissional (PAP).
