# Sicofri: Sistema de Control de Inventario de Productos Congelados

Aplicación para **Pescadería Calimar** desarrollada en el curso **IF-6100 Análisis y Diseño de Sistemas** (UCR, Sede del Pacífico, II Semestre 2026).

Sicofri reemplaza el registro en papel del inventario: controla las existencias de las **6 ubicaciones** (5 congeladores en Puntarenas y 1 cámara en San José), registra los traslados entre ellas, avisa al Jefe cuando un producto está próximo a caducar y organiza los pedidos y viajes de entrega a hoteles y restaurantes.

> **Estado actual (30/11/2026):** la **API está completa y probada** (43 endpoints, 10 módulos). Falta la **aplicación móvil .NET MAUI**. Ver [Pendientes](#12-pendientes-y-próximos-pasos).

---

## Contenido

1. [Tecnologías](#1-tecnologías)
2. [Estructura del repositorio](#2-estructura-del-repositorio)
3. [Instalación (primera vez)](#3-instalación-primera-vez)
4. [Configuración de secretos](#4-configuración-de-secretos)
5. [Firebase (notificaciones push)](#5-firebase-notificaciones-push)
6. [Crear la base de datos y ejecutar](#6-crear-la-base-de-datos-y-ejecutar)
7. [Roles y permisos](#7-roles-y-permisos)
8. [Endpoints por módulo](#8-endpoints-por-módulo)
9. [Reglas de negocio y decisiones de diseño](#9-reglas-de-negocio-y-decisiones-de-diseño)
10. [Arquitectura y convenciones](#10-arquitectura-y-convenciones)
11. [Trabajo con Git](#11-trabajo-con-git)
12. [Pendientes y próximos pasos](#12-pendientes-y-próximos-pasos)
13. [Solución de problemas comunes](#13-solución-de-problemas-comunes)

---

## 1. Tecnologías

| Pieza | Tecnología | Versión |
| --- | --- | --- |
| API | ASP.NET Core Web API (C#) | .NET 10 (LTS) |
| Base de datos | PostgreSQL | 18 |
| Acceso a datos | Entity Framework Core + Npgsql | 10.0.x |
| Autenticación | JWT (`Microsoft.AspNetCore.Authentication.JwtBearer`) | 10.0.x |
| Contraseñas | BCrypt (`BCrypt.Net-Next`) | 4.2.0 |
| Notificaciones push | Firebase Cloud Messaging (`FirebaseAdmin`) | 3.6.0 |
| Documentación / pruebas | OpenAPI + Swagger UI (`Swashbuckle.AspNetCore.SwaggerUI`) | 10.3.1 |
| App móvil (pendiente) | .NET MAUI | .NET 10 |

> **Nunca instalar paquetes 11.x ni versiones "preview".** Todo lo que empieza con `Microsoft.` debe quedar en la **misma versión 10.0.x**. Visual Studio ofrece actualizar a 11.0: **ignorar** esas actualizaciones.

La app se desarrolla y demuestra en **Android**. iOS queda compatible por el código compartido, pero no se prueba (el equipo no tiene Mac ni cuenta de Apple Developer).

---

## 2. Estructura del repositorio

```
sicofri-inventario/
├── README.md
├── .gitignore
├── Sicofri.API/                      ← carpeta de la solución
│   ├── Sicofri.API.sln (o .slnx)
│   └── Sicofri.API/                  ← proyecto de la API
│       ├── Controllers/              → reciben las peticiones y revisan permisos
│       ├── Services/                 → reglas de negocio
│       │   └── Interfaces/
│       ├── Repositories/             → acceso a PostgreSQL
│       │   └── Interfaces/
│       ├── Entities/                 → las tablas como clases C#
│       ├── Data/
│       │   ├── SicofriDbContext.cs
│       │   ├── Configurations/       → reglas de cada tabla (largos, decimales, relaciones)
│       │   └── Seed/                 → datos iniciales (6 ubicaciones, configuración, Jefe)
│       ├── Migrations/               → historia de cambios de la base de datos
│       ├── Security/                 → JWT, BCrypt, eventos de autenticación
│       ├── Exceptions/               → ReglaNegocioException
│       ├── Middleware/               → manejo de errores y validación en español
│       └── Program.cs                → configuración y registro de todas las piezas
└── Sicofri.Shared/                   ← clases compartidas entre la API y la app
    ├── Dtos/                         → lo que viaja entre la app y la API
    ├── Enums/                        → Rol, EstadoPedido, EstadoViaje, TipoLote, Provincia
    └── Utilidades/                   → NumeroPedido (PE-000001), NumeroViaje (VJ-000001)
```

---

## 3. Instalación (primera vez)

### 3.1 Programas necesarios

1. **Visual Studio Community** (versión más reciente) con las cargas de trabajo:
   - **ASP.NET y desarrollo web**
   - **Desarrollo de .NET Multi-platform App UI** (para la app)
2. **.NET 10 SDK**. Comprobar con `dotnet --list-sdks`: debe aparecer una línea `10.0.x`.
3. **PostgreSQL 18** desde el instalador de **EDB** (postgresql.org → Download → Windows):
   - Componentes: PostgreSQL Server, pgAdmin 4, Command Line Tools. **Desmarcar Stack Builder.**
   - Puerto: **5432**. Anotar la contraseña del usuario `postgres`.
4. **Git** (git-scm.com).

### 3.2 Clonar el repositorio

Clonar en una carpeta **fuera de OneDrive** (OneDrive bloquea archivos de compilación y causa errores):

```
mkdir C:\Proyectos
cd C:\Proyectos
git clone https://github.com/AntPhantom0205/sicofri-inventario.git
```

Abrir en Visual Studio la solución que está en `Sicofri.API\` y compilar con `Ctrl + Shift + B`.

---

## 4. Configuración de secretos

Las contraseñas y claves **nunca** van en `appsettings.json` (ese archivo se sube a GitHub y es público). Van en los **secretos de usuario** de Visual Studio, que viven fuera del proyecto.

Clic derecho en el proyecto `Sicofri.API` → **Administrar secretos de usuario** → pegar y completar:

```json
{
  "ConnectionStrings": {
    "SicofriDb": "Host=localhost;Port=5432;Database=sicofri;Username=postgres;Password=SU_CONTRASEÑA_DE_POSTGRES"
  },
  "Jwt": {
    "Clave": "UNA_CLAVE_ALEATORIA_DE_AL_MENOS_32_CARACTERES"
  },
  "JefeInicial": {
    "Contrasena": "CONTRASEÑA_DEL_JEFE_MINIMO_8"
  },
  "Firebase": {
    "RutaCredenciales": "C:/Proyectos/Secretos/sicofri-firebase.json"
  }
}
```

| Secreto | Para qué sirve |
| --- | --- |
| `ConnectionStrings:SicofriDb` | Conexión a su PostgreSQL local. La base `sicofri` **no hay que crearla**: la crea la migración |
| `Jwt:Clave` | Firma los tokens de sesión. Mínimo 32 caracteres o la API no arranca |
| `JefeInicial:Contrasena` | Contraseña del usuario `jefe`, que se crea solo la primera vez que arranca la API |
| `Firebase:RutaCredenciales` | Ruta a la llave de Firebase (ver sección 5). Si falta, la API funciona pero no envía avisos push |

**Generar una clave JWT** (PowerShell):

```powershell
[Convert]::ToBase64String((1..48 | ForEach-Object { [byte](Get-Random -Maximum 256) }))
```

> En las rutas de JSON usar barras normales `/` (o dobles `\\`). Una sola barra invertida `\` rompe el archivo.

> Al subir la API a un servidor real hay que generar **otra** clave JWT y otra contraseña de Jefe, y configurarlas como variables de entorno del servidor.

---

## 5. Firebase (notificaciones push)

El proyecto de Firebase se llama **Sicofri Calimar** (console.firebase.google.com). Solo se usa **Cloud Messaging** (gratuito).

1. Pedir al dueño del proyecto que lo agregue: **Configuración del proyecto → Usuarios y permisos → Agregar miembro** (rol *Editor*).
2. En **Configuración del proyecto → Cuentas de servicio → SDK de Firebase Admin → Generar nueva clave privada**. Se descarga un JSON.
3. Moverlo a `C:\Proyectos\Secretos\` (fuera del repositorio) y renombrarlo `sicofri-firebase.json`.
   - Activar **Ver → Mostrar → Extensiones de nombre de archivo** en el Explorador. Si no, el archivo puede quedar como `sicofri-firebase.json.json` y la API no lo encuentra.
4. Poner la ruta en los secretos (sección 4).

Al arrancar la API, la consola debe mostrar `Firebase inicializado correctamente.`

> **Esa llave es un secreto.** Nunca subirla a GitHub ni enviarla por WhatsApp o correo. Cada persona genera la suya. El `.gitignore` ya excluye `*firebase-adminsdk*.json` y `sicofri-firebase.json` como protección extra.

Para la **app MAUI** habrá que **registrar la app Android** en el mismo proyecto de Firebase y descargar `google-services.json` (ver Pendientes).

---

## 6. Crear la base de datos y ejecutar

### 6.1 Aplicar las migraciones

1. Comprobar que PostgreSQL esté corriendo: `Windows + R` → `services.msc` → `postgresql-x64-18` **En ejecución**.
2. En Visual Studio: **Herramientas → Administrador de paquetes NuGet → Consola del Administrador de paquetes**.
3. Ejecutar:

```
Update-Database -Project Sicofri.API -StartupProject Sicofri.API
```

Esto crea la base `sicofri`, las 11 tablas, las 6 ubicaciones y la configuración inicial (10 días de anticipación).

**Migraciones existentes** (en orden):

| Migración | Qué hace |
| --- | --- |
| `Inicial` | Todas las tablas, relaciones, reglas CHECK y datos iniciales |
| `AgregaTipoLoteYCodigoLote` | Tipo de lote (Empacado/Fresco/CongeladoPropio) y código de lote en `producto` |
| `ModuloEntregas` | Tabla `viaje_encargado` (1 a 3 por viaje) y datos de bitácora en `pedido` y `viaje` |
| `NotificacionAtencion` | Quién atendió cada notificación y cuándo; una sola notificación por lote |
| `ClienteContacto` | Teléfono y persona de contacto (opcionales) en `cliente` |

> Comprobar en pgAdmin: `SELECT "MigrationId" FROM "__EFMigrationsHistory";` debe listar las 5.

### 6.2 Ejecutar la API

El botón **https** (Iniciar) en Visual Studio. Se abre **Swagger** en `https://localhost:7009/swagger` (el puerto puede variar).

En la consola deben aparecer, en este orden aproximado: `Firebase inicializado correctamente`, `Now listening on: https://localhost:...`, y a los 10 segundos la primera **revisión de caducidades**.

### 6.3 Probar en Swagger

1. `POST /api/auth/login` con `{"nombreUsuario": "jefe", "contrasena": "<la de JefeInicial>"}`.
2. Copiar **solo** el valor de `token` (sin comillas, sin `",` ni la línea de `expira`).
3. Botón **Authorize** → pegar el token (sin escribir "Bearer") → *Authorize*.
4. Crear usuarios de prueba con `POST /api/usuarios` (roles `Secretaria` o `Empleado`).

---

## 7. Roles y permisos

Hay **un solo Jefe** (creado automáticamente). Las demás cuentas solo pueden ser **Secretaria** o **Empleado**.

| Funcionalidad | Jefe | Secretaria | Empleado |
| --- | :---: | :---: | :---: |
| Administrar usuarios (crear, editar, desactivar) | Sí | No | No |
| Productos: registrar, editar, consultar, eliminar | Sí | Sí | Sí |
| Inventario por ubicación | Sí | Sí | Sí |
| Traslados y su bitácora | Sí | Sí | Sí |
| Ver configuración | Sí | Sí | Sí |
| Cambiar días de anticipación | Sí | No | No |
| Ver y atender notificaciones de caducidad | Sí | Sí | No |
| Ejecutar revisión manual de caducidades | Sí | No | No |
| Recibir el aviso push | Sí | No | No |
| Clientes: consultar y registrar | Sí | Sí | Sí |
| Clientes: editar y eliminar | Sí | Sí | No |
| Pedidos: consultar y armar | Sí | Sí | Sí |
| Pedidos: actualizar y cancelar | Sí | Sí | No |
| Viajes: programar, editar, eliminar, consultar | Sí | Sí | No |
| Viajes: iniciar y marcar entregas | Sí | No | Sí (solo si es encargado) |
| Mis entregas | No | No | Sí |
| Bitácora de entregas | Sí | Sí | No |

---

## 8. Endpoints por módulo

Todos requieren token, salvo el login. Las respuestas de error siguen el formato de la [sección 10.3](#103-formato-de-errores).

### Autenticación: `api/auth`

| Método | Ruta | Descripción |
| --- | --- | --- |
| POST | `/api/auth/login` | Inicia sesión y devuelve el token JWT (válido 8 horas) |
| PUT | `/api/auth/dispositivo` | Registra (o borra con `null`) el token de Firebase del celular |

### Usuarios: `api/usuarios` (solo Jefe)

| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/api/usuarios` | Usuarios activos |
| GET | `/api/usuarios/{id}` | Un usuario |
| POST | `/api/usuarios` | Crea Secretaria o Empleado |
| PUT | `/api/usuarios/{id}` | Edita nombre, usuario, rol y (opcional) contraseña |
| DELETE | `/api/usuarios/{id}` | Desactiva (no borra) |

### Productos: `api/productos`

| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/api/productos?nombre=&idUbicacion=&incluirAgotados=` | Busca lotes |
| GET | `/api/productos/proximos-a-caducar` | Lotes en riesgo según su tipo |
| GET | `/api/productos/{id}` | Un lote |
| POST | `/api/productos` | Registra un lote |
| PUT | `/api/productos/{id}` | Corrige un lote (no cambia la ubicación) |
| DELETE | `/api/productos/{id}` | Solo si no tiene traslados ni pedidos |

### Inventario: `api/inventario`

| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/api/inventario/ubicaciones` | Resumen de las 6 ubicaciones (lotes, kg, lotes en riesgo) |
| GET | `/api/inventario/ubicaciones/{id}/productos` | Lotes con existencia en una ubicación |

### Traslados: `api/traslados`

| Método | Ruta | Descripción |
| --- | --- | --- |
| POST | `/api/traslados` | Traslada kilos de un lote a otra ubicación |
| GET | `/api/traslados/bitacora?producto=&desde=&hasta=&pagina=&tamanoPagina=` | Bitácora de movimientos |

### Configuración: `api/configuracion`

| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/api/configuracion` | Días de anticipación y reglas de frescos |
| PUT | `/api/configuracion` | Cambia los días de anticipación (Jefe, 1 a 60) |

### Notificaciones: `api/notificaciones` (Jefe y Secretaria)

| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/api/notificaciones?atendida=&pagina=&tamanoPagina=` | Bitácora de notificaciones, pendientes primero |
| GET | `/api/notificaciones/pendientes/cantidad` | Número para la campanita |
| PUT | `/api/notificaciones/{id}/atender` | Marca como atendida (registra quién y cuándo) |
| POST | `/api/notificaciones/revisar` | Revisión inmediata (solo Jefe) |

### Clientes: `api/clientes`

| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/api/clientes?nombre=&provincia=` | Busca clientes |
| GET | `/api/clientes/por-provincia` | Las 7 provincias con sus clientes y entregas (para el mapa) |
| GET | `/api/clientes/{id}` | Un cliente |
| POST | `/api/clientes` | Registra (todos los roles) |
| PUT | `/api/clientes/{id}` | Edita, incluso para agregar teléfono o contacto después (Jefe/Secretaria) |
| DELETE | `/api/clientes/{id}` | Solo si no tiene pedidos (Jefe/Secretaria) |

### Pedidos: `api/pedidos`

| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/api/pedidos?estado=&busqueda=&pagina=&tamanoPagina=` | Lista; `busqueda` acepta `PE-000012`, `12` o nombre del cliente |
| GET | `/api/pedidos/{id}` | Detalle con líneas y rastro |
| POST | `/api/pedidos` | Arma un pedido y descuenta los kilos |
| PUT | `/api/pedidos/{id}` | Reemplaza líneas de un pedido Armado (Jefe/Secretaria) |
| PUT | `/api/pedidos/{id}/cancelar` | Cancela y devuelve los kilos (Jefe/Secretaria) |

### Viajes: `api/viajes`

| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/api/viajes?estado=&desde=&hasta=` | Lista (Jefe/Secretaria) |
| GET | `/api/viajes/{id}` | Detalle completo (Jefe/Secretaria) |
| GET | `/api/viajes/mis-entregas` | Viajes pendientes del empleado con sesión (Empleado) |
| GET | `/api/viajes/bitacora?desde=&hasta=&idEmpleado=&estado=` | Historial de entregas (Jefe/Secretaria) |
| POST | `/api/viajes` | Programa un viaje (Jefe/Secretaria) |
| PUT | `/api/viajes/{id}` | Edita un viaje Programado (Jefe/Secretaria) |
| DELETE | `/api/viajes/{id}` | Elimina un viaje Programado (Jefe/Secretaria) |
| PUT | `/api/viajes/{id}/iniciar` | Inicia el viaje (encargado o Jefe) |
| PUT | `/api/viajes/{id}/pedidos/{idPedido}/entregar` | Marca un pedido entregado (encargado o Jefe) |

---

## 9. Reglas de negocio y decisiones de diseño

Estas reglas se definieron con el equipo y con el contacto de **Pescadería Calimar**. **No cambiarlas sin consultar**, porque responden a cómo trabaja la empresa.

### 9.1 Principio general: nada que tenga historial se borra

| Entidad | "Eliminar" significa |
| --- | --- |
| Usuario | Se **desactiva**: no aparece en listas ni puede iniciar sesión, pero su nombre queda en traslados y viajes |
| Pedido | Se **cancela**: estado `Cancelado`, se registra quién y cuándo, conserva sus líneas |
| Notificación | Se **atiende** (por una persona) o se **cierra sola** si el lote se agota |
| Lote | Solo se borra si nunca se trasladó ni se usó en un pedido; si no, se deja en 0 kg |

### 9.2 Productos y caducidad (definido con Calimar)

Cada lote tiene un **tipo** que define cómo se obtiene su fecha de caducidad:

| Tipo | Caso real | Caducidad |
| --- | --- | --- |
| `Empacado` | Viene de otra compañía con etiqueta | Se **digita** la de la etiqueta. Código de lote **obligatorio** |
| `Fresco` | Lo prepara Calimar | **Producción + 3 días** (calculada, no se digita) |
| `CongeladoPropio` | Calimar lo congela | **Producción + N meses** (3, 6, etc., se indican al registrar) |

- **Alertas:** los frescos se avisan **1 día antes**; los demás, con los **días configurables** (10 por defecto).
- Las reglas viven en `Services/ReglasCaducidad.cs`.
- No existe tabla de tipos de producto ni de proveedores: la empresa reconoce a los proveedores solo por su nombre.

### 9.3 Traslados

- Un traslado **divide el lote**: si hay 40 kg y se mueven 15, el original queda en 25 y se crea un lote de 15 en el destino con las mismas fechas.
- Si en el destino ya existe un **lote gemelo** (mismo producto, proveedor, tipo, código y fechas), se le **suman** los kilos en vez de crear otro.
- Todo ocurre en una **transacción**, y la resta usa `UPDATE ... WHERE cantidad >= X` para evitar que dos usuarios saquen los mismos kilos.
- El responsable sale del **token**, nunca del cuerpo de la petición.

### 9.4 Notificaciones

- Un **revisor automático** (`RevisorCaducidadWorker`) revisa al arrancar la API y **todos los días a las 6:00 a. m.** (hora de Costa Rica).
- **Una sola notificación por lote** (garantizado también por un índice único en la base).
- El push llega **solo al Jefe**. Si Firebase falla o no está configurado, la notificación igual se guarda.
- Los teléfonos de Calimar son **compartidos**: al registrar un token se le quita a cualquier otro usuario que lo tuviera.
- Los tokens que Firebase rechaza se borran automáticamente.

### 9.5 Pedidos

- Se **descuentan los kilos al armar** (el pedido nace `Armado`), en una transacción.
- No se puede usar un lote vencido **ni uno que venza antes de la fecha de entrega**.
- Un mismo lote no puede aparecer dos veces en un pedido.
- **Actualizar** (solo `Armado`): devuelve los kilos viejos y descuenta los nuevos; los kilos ya apartados por el pedido cuentan como disponibles.
- **Cancelar** (solo `Armado`): devuelve cada kilo a su lote original y saca el pedido de su viaje.
- Flujo: `Armado → EnViaje → Entregado` (o `Armado → Cancelado`).

### 9.6 Viajes

- **1 a 3 empleados** encargados por viaje (por seguridad, después de un asalto a un repartidor que viajaba solo).
- Solo pedidos `Armado` y sin otro viaje.
- La fecha del viaje **no puede ser posterior** a la fecha de entrega de sus pedidos.
- Solo se edita o elimina mientras está `Programado`.
- **No se puede iniciar antes de su fecha** ni sin pedidos.
- Al iniciar: viaje `EnCurso`, pedidos `EnViaje`.
- Al entregar el **último** pedido, el viaje pasa **solo** a `Finalizado`.
- No existe flujo de "no entregado": según Calimar, las entregas siempre se completan (salida 4 a. m., última entrega 5–6 p. m.).

### 9.7 Fechas y horas

- Las **fechas** (caducidad, entrega, viaje) se guardan como `date`.
- Las **horas** (traslados, entregas, inicio y fin de viaje) se guardan en **UTC** y la API las devuelve en **hora de Costa Rica** (`-06:00`). Ver `Services/HoraCostaRica.cs`.
- "Hoy" siempre se calcula con la zona horaria de Costa Rica, sin importar dónde esté el servidor.

---

## 10. Arquitectura y convenciones

### 10.1 Capas

```
Controlador  →  recibe la petición y revisa el rol ([Authorize])
     ↓
Servicio     →  aplica las reglas de negocio; lanza ReglaNegocioException
     ↓
Repositorio  →  consulta y guarda en PostgreSQL (EF Core)
```

- Los servicios **nunca** usan el `DbContext` directamente; solo repositorios.
- Los controladores **no tienen** `try/catch`: los errores los atrapa `ExceptionMiddleware`.
- Los **DTOs** (en `Sicofri.Shared`) separan lo que se guarda de lo que se envía: por ejemplo, el hash de la contraseña y el token de Firebase nunca salen de la API.

### 10.2 Nombres

- **C#:** PascalCase (`IdProducto`, `FechaCaducidad`). Conceptos del negocio en español; palabras del framework en inglés (`Controller`, `Service`, `Repository`).
- **Base de datos:** minúscula con guion bajo (`id_producto`, `fecha_caducidad`). La conversión es **automática** en `SicofriDbContext.AplicarNombresSnakeCase()`; no hay que escribir nombres de columnas.
- **Enums:** se guardan como **texto** en la base (`Jefe`, `Armado`) y viajan como texto en el JSON.
- **Rutas REST:** en plural (`/api/pedidos`), con verbos HTTP estándar.

### 10.3 Formato de errores

Todas las respuestas de error tienen la misma forma (`ErrorResponse`):

```json
{
  "mensaje": "No hay suficiente 'Camarón jumbo' en Cámara San José. Disponible: 20 kg.",
  "errores": null,
  "detalle": null
}
```

| Código | Cuándo |
| --- | --- |
| 400 | Datos inválidos o regla de negocio (`errores` trae el detalle por campo si el JSON está mal formado) |
| 401 | Sin sesión, token vencido o mal copiado (el mensaje dice cuál) |
| 403 | El rol no tiene permiso, o el empleado no es encargado de ese viaje |
| 404 | No existe |
| 409 | Conflicto: duplicado, estado que no permite la acción, o dos usuarios a la vez |
| 500 | Error inesperado. En **Development**, `detalle` trae el error técnico exacto |

### 10.4 Agregar un módulo nuevo (receta)

1. Entidad en `Entities/` + configuración en `Data/Configurations/`.
2. `Add-Migration NombreDescriptivo` → **leer el archivo generado** → `Update-Database`.
3. DTOs en `Sicofri.Shared/Dtos/<Modulo>/` (como `record`).
4. Interfaz + repositorio.
5. Interfaz + servicio (reglas y `ReglaNegocioException`).
6. Controlador con `[Authorize]` y los roles correspondientes.
7. Registrar repositorio y servicio en `Program.cs` (sección 4).
8. Probar en Swagger **todos** los casos (éxito, errores y permisos) antes de seguir.

---

## 11. Trabajo con Git

**Antes de empezar a trabajar cada día:**

```
git pull
```

**Al terminar algo que funciona:**

```
git add .
git status        ← revisar que no aparezca ningún secreto ni archivo .json de Firebase
git commit -m "Descripción clara de lo que cambió"
git push
```

Reglas:

- **Nunca** subir secretos: contraseñas, clave JWT, llave de Firebase.
- **Nunca editar ni borrar una migración que ya está en GitHub.** Si hay que corregir algo, se crea una migración nueva.
- Después de un `git pull` que trae una migración nueva, ejecutar `Update-Database`.
- Commits pequeños y frecuentes, con mensajes que expliquen qué cambió.

---

## 12. Pendientes y próximos pasos

### 12.1 Aplicación móvil .NET MAUI (prioridad)

Pantallas definidas en la Etapa VI:

| Módulo | Pantallas |
| --- | --- |
| Autenticación | Inicio de sesión · Crear usuario · Confirmación · Gestión de usuarios |
| General | Dashboard con accesos según el rol · Configuración (días de anticipación, perfil) |
| Productos e inventario | Registrar lote (con el selector de tipo) · Inventario por ubicación · Detalle de ubicación |
| Traslados | Formulario de traslado · Confirmación · Bitácora |
| Notificaciones | Campanita con lista y atención |
| Clientes | Registro y edición · Mapa de Costa Rica por provincia |
| Pedidos | Crear pedido con líneas · Confirmación · Lista con filtros |
| Viajes | Programar viaje · Mis entregas (iniciar, marcar entregado) · Bitácora de entregas |

Pasos técnicos para la app:

- Crear el proyecto `Sicofri.App` (.NET MAUI, .NET 10) y referenciar `Sicofri.Shared`.
- Patrón **MVVM** (`Models/`, `Views/`, `ViewModels/`, `Services/`).
- Guardar el token con `SecureStorage` y enviarlo como `Authorization: Bearer ...`.
- Leer los errores con `ErrorResponse` y mostrar `mensaje` al usuario.
- Mostrar `"San José"` con `ProvinciaExtensiones.NombreParaMostrar()` y los números con `NumeroPedido` / `NumeroViaje`.
- **Firebase en la app:** registrar la app Android en el proyecto *Sicofri Calimar*, descargar `google-services.json` y, al iniciar sesión, llamar a `PUT /api/auth/dispositivo` con el token del celular (y con `null` al cerrar sesión). Recién ahí se puede probar el push de punta a punta.
- El emulador de Android no ve `localhost` de la PC: usar `https://10.0.2.2:<puerto>` o publicar la API.

### 12.2 Otros pendientes

- Proyecto **`Sicofri.Tests`** (xUnit) para las reglas críticas: cálculo de caducidad, traslados, descuento y devolución de kilos en pedidos.
- **Despliegue** de la API y PostgreSQL en la nube, con secretos como variables de entorno.

### 12.3 Mejoras futuras (documentadas, no requeridas)

- Botón "Finalizar viaje" para registrar la hora de llegada del camión a Puntarenas.
- Reabrir una notificación si un lote agotado recupera kilos por una cancelación (hoy aparece en "próximos a caducar", pero no genera un segundo aviso).
- Seguimiento GPS de los viajes (requiere permisos de ubicación en segundo plano, clave de Google Maps y consentimiento de los empleados).

---

## 13. Solución de problemas comunes

| Síntoma | Causa | Solución |
| --- | --- | --- |
| `Failed to connect to 127.0.0.1:5432` | PostgreSQL no está corriendo o no está instalado | `services.msc` → iniciar `postgresql-x64-18` |
| `password authentication failed` | Contraseña incorrecta en los secretos | Revisar `ConnectionStrings:SicofriDb` |
| `no existe la columna ...` (error 500) | Falta aplicar una migración | `Update-Database` |
| `Falta la cadena de conexión 'SicofriDb'` | Secretos sin configurar o JSON mal escrito | Revisar comas, llaves y comillas en `secrets.json` |
| `La clave 'Jwt:Clave' falta o es muy corta` | Clave JWT de menos de 32 caracteres | Generar una nueva (sección 4) |
| `Firebase no está configurado...` | Ruta incorrecta o extensión oculta (`.json.json`) | Mostrar extensiones y revisar la ruta |
| 401 *"El token está mal formado"* | Se copió el token con comillas o con `",` | Copiar solo el texto del token |
| 401 *"...su firma no coincide"* | Token viejo o se cambió la clave JWT | Iniciar sesión de nuevo |
| `CS0104 referencia ambigua` | Dos clases con el mismo nombre en carpetas distintas | Borrar la clase duplicada (ej. un `Service` dentro de `Repositories`) |
| `CS0311` en `Program.cs` | La clase no declara su interfaz | Debe decir `public class X : IX` |
| Errores raros de archivos bloqueados | Proyecto dentro de OneDrive | Moverlo a `C:\Proyectos` |
| Swagger no se abre solo | `launchBrowser` desactivado | En `launchSettings.json`: `"launchBrowser": true`, `"launchUrl": "swagger"` |
| Un filtro de Swagger pide números en vez de nombres | Falta configurar los enums para OpenAPI | Ya resuelto con `ConfigureHttpJsonOptions` en `Program.cs` |

---

**Equipo:** Nikole Arias Zamora · Sara Benavides Murillo · Maureen Madrigal Rojas · Madelyn Medina Hernández · Anthony Yahir Obregón Antón · Emmanuel Solórzano Elizondo

**Profesora:** Mag. Karol Sugeil Sánchez Pérez
