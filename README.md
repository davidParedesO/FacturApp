# 🧾 FacturApp - Asistente Inteligente de Facturación

FacturApp es una aplicación MAUI Blazor Hybrid diseñada para revolucionar la gestión de facturas mediante Inteligencia Artificial. Permite procesar documentos, interactuar con un asistente de IA mediante voz y gestionar clientes y productos de forma eficiente.

## 🚀 Características Principales

- **🤖 Asistente IA (Voz/Texto):** Interactúa con GPT-4o para realizar consultas y operaciones.
- **📄 Procesamiento de Documentos:** Extracción inteligente de datos con Azure Document Intelligence.
- **🎙️ Control por Voz:** Integración total con Azure Speech Services.
- **🗃️ Base de Datos Vectorial:** Implementación con PostgreSQL + pgvector para búsquedas semánticas.
- **🛡️ Arquitectura Robusta:** Procesamiento asíncrono mediante RabbitMQ.

---

## 🛠️ Requisitos Previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Claves de suscripción a [Azure AI Services](https://azure.microsoft.com/en-us/products/ai-services/) (Speech, OpenAI, Document Intelligence).

---

## 🏁 Inicio Rápido

### 1. Levantar la Infraestructura (Docker)

El proyecto utiliza Docker para gestionar la base de datos y la cola de mensajería. Ejecuta el siguiente comando en la raíz del proyecto:

```bash
docker-compose up -d
```

Esto desplegará:
- **PostgreSQL (pgvector):** Puerto 5432.
- **RabbitMQ:** Puerto 5672 (API) y 15672 (Panel de administración).
  - *Admin UI:* [http://localhost:15672](http://localhost:15672) (admin / superpassword123)

### 2. Configurar Claves de API

Para que la IA funcione, debes configurar tus credenciales de Azure en el archivo:
`Config/AppConfig.cs`

Asegúrate de rellenar los siguientes campos:

```csharp
// Azure Speech
public const string SpeechKey    = "TU_CLAVE";
public const string SpeechRegion = "TU_REGION";

// Azure OpenAI
public const string OpenAIEndpoint   = "TU_ENDPOINT";
public const string OpenAIKey        = "TU_CLAVE";
public const string OpenAIDeployment = "gpt-4o";

// Azure Document Intelligence
public const string DocIntelligenceEndpoint = "TU_ENDPOINT";
public const string DocIntelligenceKey      = "TU_CLAVE";
```

### 3. Ejecutar la Aplicación

Puedes ejecutar el proyecto desde Visual Studio, JetBrains Rider o mediante la CLI:

```bash
dotnet build
dotnet run --project FacturApp.csproj
```

---

## 🏗️ Arquitectura Técnica

- **Frontend:** Blazor Hybrid (HTML/CSS dentro de MAUI).
- **Backend:** C# / .NET MAUI.
- **Mensajería:** RabbitMQ para desacoplar procesos pesados.
- **Persistencia:** Entity Framework Core con PostgreSQL.
- **IA:** Integración nativa con Azure Cognitive Services.

---

## 📱 Notas de Desarrollo (Android)

Si ejecutas la app en un emulador o dispositivo físico Android, recuerda que `localhost` no funcionará. El archivo `AppConfig.cs` ya está preparado para detectar el entorno, pero asegúrate de que la IP `LocalHostIp` coincida con la IP local de tu máquina de desarrollo.

---
## 📸 Vistas de la Aplicación

A continuación, algunas capturas del funcionamiento y la interfaz de la aplicación:

<div align="center">
  <img width="600" alt="Vista Principal de FacturApp" src="https://github.com/user-attachments/assets/d325ef5e-c026-4655-bf53-5d18983c34f7" />
  <p><em>Vista Principal de FacturApp</em></p>
</div>

<div align="center">
  <img width="682" alt="Procesamiento de Facturas" src="https://github.com/user-attachments/assets/85f0bf5f-6f59-46b8-84a7-9f0c9222b1e2" />
  <p><em>Asistente IA analizando documentos</em></p>
</div>

<div align="center">
  <img width="716" alt="Interfaz de Gestión" src="https://github.com/user-attachments/assets/4c8c061a-62ac-4eea-9e30-465cecfba6d4" />
  <p><em>Extracción de datos inteligente</em></p>
</div>

### Panel de Administración y Servicios (RabbitMQ)
La arquitectura asíncrona garantiza que la interfaz de usuario nunca se bloquee mientras se procesan las facturas.

<div align="center">
  <img width="1415" alt="Dashboard de RabbitMQ 1" src="https://github.com/user-attachments/assets/629e0d20-7a90-4abe-af8d-08fbbd8aec4c" />
</div>

<div align="center">
  <img width="1418" alt="Dashboard de RabbitMQ 2" src="https://github.com/user-attachments/assets/131fcc23-c414-4dee-8eea-3ef40f7cb70e" />
</div>

<div align="center">
  <img width="1424" alt="Monitorización de Colas" src="https://github.com/user-attachments/assets/9decc914-9416-402e-8f9f-83fd211c3f13" />
</div>

---

Developed with ❤️ by David Paredes.
