# Step 5: External Systems - Identificación de sistemas externos

## Objetivo del paso

Los siguientes elementos a identificar son los sistemas externos que tu proceso usa o con los que se integra. Un ejemplo clásico es un procesador de pagos. El flujo de ejemplo de registro de usuario delega el trabajo de validar y aceptar pagos a un procesador de pagos como PayPal o Stripe.

## Representación visual

Los sistemas externos se representan con **notas adhesivas rosadas/rectangulares** (pink rectangular sticky notes).

```
┌─────────────────┐
│  Credit card    │  ← Nota naranja (evento)
│  entered        │
└─────────────────┘
        ↓
┌─────────────────┐
│  Payment        │  ← Nota rosada rectangular (sistema externo)
│  Processor      │
│  (Stripe)       │
└─────────────────┘
        ↓
┌─────────────────┐
│  Payment        │  ← Nota naranja (evento)
│  accepted       │
└─────────────────┘
```

## ¿Qué constituye un "sistema"?

Los participantes de EventStorming a menudo se confunden sobre qué constituye un "sistema":
- ¿Es un sistema lo que estás construyendo?
- ¿Es otra herramienta interna o microservicio?
- ¿Dónde está el límite?

## Definición de sistema externo

> "Un Sistema Externo es cualquier cosa a la que podemos echarle la culpa" - Alberto Brandolini

Esta definición humorística pero práctica ayuda a clarificar el concepto.

## Criterios para identificar sistemas externos

### Criterio principal: Control y responsabilidad

Un sistema es "externo" si:
- **No tienes control directo** sobre su funcionamiento
- **No eres responsable** de su mantenimiento o desarrollo
- **Dependes de él** pero no puedes modificarlo directamente
- **Puede fallar** independientemente de tu sistema

### Criterio secundario: Límites organizacionales

Un sistema puede ser "externo" si:
- Es operado por **otro equipo** dentro de tu organización
- Tiene **diferentes ciclos de release** que tu sistema
- Requiere **coordinación especial** para cambios
- Tiene **SLAs diferentes** a tu sistema principal

## Tipos de sistemas externos

### 1. Sistemas de terceros (3rd party)

#### SaaS applications
Aplicaciones que no controlas directamente

**Ejemplos**:
```
┌─────────────────────────────┐
│  Stripe Payment Processor   │
└─────────────────────────────┘

┌─────────────────────────────┐
│  SendGrid Email Service     │
└─────────────────────────────┘

┌─────────────────────────────┐
│  Twilio SMS Gateway         │
└─────────────────────────────┘

┌─────────────────────────────┐
│  Auth0 Authentication       │
└─────────────────────────────┘

┌─────────────────────────────┐
│  Salesforce CRM             │
└─────────────────────────────┘
```

#### APIs externas
Servicios de terceros que consumes

**Ejemplos**:
```
┌─────────────────────────────┐
│  Google Maps API            │
└─────────────────────────────┘

┌─────────────────────────────┐
│  Weather Service API        │
└─────────────────────────────┘

┌─────────────────────────────┐
│  Currency Exchange API      │
└─────────────────────────────┘
```

### 2. Sistemas internos de otras áreas

#### Sistemas de otros equipos
Aplicaciones que tu organización posee pero otro equipo opera

**Ejemplos**:
```
┌─────────────────────────────┐
│  User Management System     │
│  (Identity Team)            │
└─────────────────────────────┘

┌─────────────────────────────┐
│  Legacy Billing System      │
│  (Finance Team)             │
└─────────────────────────────┘

┌─────────────────────────────┐
│  Corporate Directory        │
│  (IT Team)                  │
└─────────────────────────────┘

┌─────────────────────────────┐
│  Data Warehouse             │
│  (Analytics Team)           │
└─────────────────────────────┘
```

### 3. Microservicios propios tratados como externos

#### Servicios desacoplados
Servicios que tu equipo construyó pero que operan independientemente

**Ejemplos**:
```
┌─────────────────────────────┐
│  QR Code Generator Service  │
│  (Our Team - Microservice)  │
└─────────────────────────────┘

┌─────────────────────────────┐
│  Notification Service       │
│  (Our Team - Separate)      │
└─────────────────────────────┘

┌─────────────────────────────┐
│  Image Processing Service   │
│  (Our Team - Independent)   │
└─────────────────────────────┘
```

## Ejemplo completo: Flujo de onboarding de usuario

```
┌─────────────────┐
│  Email entered  │
└─────────────────┘
        ↓
┌─────────────────────────────┐
│  Email Verification Service │  ← Sistema externo (3rd party)
│  (SendGrid)                 │
└─────────────────────────────┘
        ↓
┌─────────────────┐
│  Email verified │
└─────────────────┘
        ↓
┌─────────────────┐
│  Credit card    │
│  entered        │
└─────────────────┘
        ↓
┌─────────────────────────────┐
│  Payment Processor          │  ← Sistema externo (3rd party)
│  (Stripe)                   │
└─────────────────────────────┘
        ↓
┌─────────────────┐
│  Payment        │
│  accepted       │
└─────────────────┘
        ↓
┌─────────────────────────────┐
│  User Management System     │  ← Sistema externo (interno)
│  (Identity Team)            │
└─────────────────────────────┘
        ↓
┌─────────────────┐
│  Account        │
│  activated      │
└─────────────────┘
```

## Preguntas para identificar sistemas externos

### Durante el mapeo de eventos
- "¿Quién o qué procesa este evento?"
- "¿Dónde se almacena esta información?"
- "¿Qué sistema valida esto?"
- "¿Quién envía esta notificación?"
- "¿Dónde se ejecuta esta lógica?"

### Para validar si es externo
- "¿Podemos modificar este sistema directamente?"
- "¿Somos responsables si este sistema falla?"
- "¿Necesitamos coordinar con otro equipo para cambios?"
- "¿Tiene este sistema su propio ciclo de release?"
- "¿Podríamos reemplazar este sistema fácilmente?"

## Beneficios de identificar sistemas externos

### 1. Identificación de dependencias
- Visibilidad de puntos de falla externos
- Comprensión de la cadena de dependencias
- Identificación de cuellos de botella

### 2. Planificación de integración
- Requisitos de API y conectividad
- Manejo de errores y timeouts
- Estrategias de fallback

### 3. Definición de bounded contexts
- Límites naturales del sistema
- Responsabilidades claras
- Interfaces bien definidas

### 4. Análisis de riesgos
- Sistemas críticos fuera de control
- Puntos de falla únicos
- Estrategias de mitigación

## Consideraciones técnicas

### Patrones de integración
Para cada sistema externo, considera:
- **Sincrónico vs Asincrónico**: ¿Necesitas respuesta inmediata?
- **Push vs Pull**: ¿Quién inicia la comunicación?
- **Batch vs Real-time**: ¿Cuál es la frecuencia de integración?
- **Retry y Circuit Breaker**: ¿Cómo manejas fallos?

### Manejo de errores
- ¿Qué pasa si el sistema externo no responde?
- ¿Hay estrategias de fallback?
- ¿Cómo se notifican los errores?
- ¿Hay mecanismos de retry?

### Monitoreo y observabilidad
- ¿Cómo monitoreamos la salud del sistema externo?
- ¿Qué métricas son importantes?
- ¿Cómo detectamos degradación de performance?

## Mejores prácticas

### ✅ Hacer
1. **Ser específico**: Incluir nombre del sistema y equipo responsable
2. **Documentar propósito**: ¿Para qué se usa este sistema?
3. **Identificar interfaces**: ¿Cómo se comunica con tu sistema?
4. **Considerar alternativas**: ¿Hay opciones de fallback?
5. **Evaluar criticidad**: ¿Qué tan crítico es este sistema?

### ❌ Evitar
1. **Asumir control**: No tratar sistemas externos como propios
2. **Ignorar dependencias**: Todos los sistemas externos son importantes
3. **Sobrecomplicar**: No todos los sistemas necesitan integración compleja
4. **Subestimar riesgos**: Los sistemas externos pueden fallar

## Facilitación efectiva

### Preguntas útiles
- "¿Qué sistemas están involucrados en este proceso?"
- "¿Quién es responsable de mantener este sistema?"
- "¿Qué pasa si este sistema no está disponible?"
- "¿Hay alternativas a este sistema?"
- "¿Cómo se comunica nuestro sistema con este?"

### Técnicas de identificación
- Seguir el flujo de datos
- Mapear responsabilidades
- Identificar puntos de validación
- Buscar integraciones existentes
- Revisar arquitectura actual

## Validación del paso

Antes de continuar al Step 6, confirma:

**[Answer]: ¿Se identificaron todos los sistemas externos que interactúan con el proceso?**

**[Answer]: ¿Cada sistema externo tiene un propósito claro y específico?**

**[Answer]: ¿Se documentó qué equipo o proveedor es responsable de cada sistema?**

**[Answer]: ¿Se consideraron las implicaciones de falla de cada sistema externo?**

**[Answer]: ¿Se identificaron las interfaces y métodos de comunicación?**

**[Answer]: ¿Se evaluó la criticidad de cada dependencia externa?**

## Resultado esperado

Al final de este paso, deberías tener:
- Sistemas externos claramente identificados con sticky notes rosados rectangulares
- Propósito específico de cada sistema externo documentado
- Responsabilidades y ownership claros para cada sistema
- Interfaces y métodos de comunicación identificados
- Dependencias críticas vs no críticas clasificadas
- Comprensión de puntos de falla potenciales
- Base para definir bounded contexts en el siguiente paso

## Próximo paso

Una vez que los sistemas externos están identificados y las dependencias son claras, es tiempo de identificar eventos pivotales y definir bounded contexts en Step 6: Pivotal Events.
