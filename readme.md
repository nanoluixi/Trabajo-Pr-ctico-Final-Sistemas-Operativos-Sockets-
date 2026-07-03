# Proyecto Final - Cliente/Servidor de Transferencia de Archivos 📡

Aplicación de comunicación cliente/servidor basada en sockets TCP construida en .NET 10. Permite a un cliente autenticarse, listar, subir, descargar y borrar archivos en el servidor, con registro de actividades y almacenamiento local de archivos.

## 🌟 Características principales

- Conexión TCP entre cliente y servidor usando sockets.
- Autenticación básica por nombre de usuario.
- Transferencia de archivos entre cliente y servidor.
- Comandos de gestión remota:
  - `listar servidor`
  - `listar local`
  - `subir <archivo>`
  - `descargar <archivo>`
  - `borrar <archivo>`
  - `bye`
- Registro de sesiones y acciones en archivos de log.
- Directorios locales para archivos en subida, archivos subidos y descargas.

## 🧱 Stack tecnológico

- .NET 10 (C#)
- Sockets TCP (`System.Net.Sockets`)
- Proyectos separados:
  - `socket-servidor` (servidor)
  - `socket-cliente` (cliente)
  - `SocketCommon` (lógica compartida)

## 📁 Estructura del proyecto

- `socket-servidor/`
  - `Program.cs`
  - `Services/`
  - `Client/`
  - `Logs/`
  - `Users/`
- `socket-cliente/`
  - `Program.cs`
  - `Services/`
  - `LocalFiles/`
- `SocketCommon/`
  - `Services/`

## 🚀 Instalación y configuración local

1. Clona o copia el repositorio a tu equipo.
2. Abre la carpeta raíz del proyecto en tu editor o terminal.
3. Asegúrate de tener instalado .NET 10 SDK.
4. Desde la raíz, compila la solución:

```bash
dotnet build socket-servidor.sln
```


### Variables de entorno

- No se requieren variables de entorno para ejecutar este proyecto.
- El servidor utiliza el puerto `8080` por defecto.
- El cliente se conecta a `localhost` en el puerto `8080`.

## ▶️ Ejecución en modo desarrollo

### 1. Ejecutar el servidor

Abre una terminal en la carpeta raíz y ejecuta:

```bash
cd socket-servidor
dotnet run 
```

El servidor creará automáticamente los directorios:
- `socket-servidor/Logs`
- `socket-servidor/Users`

### 2. Ejecutar el cliente

Abre otra terminal y ejecuta:

```bash
cd socket-cliente
dotnet run 
```

El cliente creará automáticamente los directorios locales:
- `socket-cliente/LocalFiles/up`
- `socket-cliente/LocalFiles/upped`
- `socket-cliente/LocalFiles/downloads`

## 💡 Uso básico

1. Inicia primero el servidor.
2. Inicia luego el cliente.
3. En el cliente, ingresa tu nombre de usuario cuando el servidor lo solicite.
4. Usa los comandos disponibles para interactuar con los archivos remotos.

## 🎓 Nota

Este proyecto forma parte del trabajo final de la materia **Sistemas Operativos** de la **Universidad Nacional de Hurlingham (UNAHUR)**.

---

Si quieres mejorar el proyecto, puedes agregar autenticación adicional, cifrado de conexión o una interfaz gráfica para cliente/servidor.
