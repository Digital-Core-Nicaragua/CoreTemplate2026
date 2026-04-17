# Step 6: Pivotal Events - Identificación de eventos pivotales

## Objetivo del paso

En esta etapa hay más que suficiente información para identificar "pivotal events" (eventos pivotales) o "key events" (eventos clave). Estos eventos son donde hay un cambio significativo en el proceso. Son muy importantes ya que representan la emergencia de capacidades de negocio o un bounded context (contexto acotado).

## ¿Qué es un evento pivotal?

Un evento pivotal es un evento que marca:
- **Cambio significativo en el proceso**: Un punto de inflexión en el flujo
- **Emergencia de capacidades de negocio**: Nuevas funcionalidades o responsabilidades
- **Bounded context**: Límites entre diferentes contextos de dominio
- **Transición de fase**: Cambio de una fase del proceso a otra
- **Handoff crítico**: Transferencia de responsabilidad entre actores o sistemas

## Representación visual

Los eventos pivotales pueden ser marcados con:
- **Líneas verticales** que dividen el tablero
- **Etiquetas de fase** que nombran cada sección
- **Destacado visual** del evento pivotal mismo

```
┌─────────────────────────────────────────────────────────────────┐
│                    FASE 1: User Registration                    │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────┐
│  Name entered   │
└─────────────────┘
        ↓
┌─────────────────┐
│  Email verified │  ← EVENTO PIVOTAL
└─────────────────┘

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

┌─────────────────────────────────────────────────────────────────┐
│                  FASE 2: Payment Verification                   │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────┐
│  Credit card    │
│  entered        │
└─────────────────┘
        ↓
┌─────────────────┐
│  Payment        │  ← EVENTO PIVOTAL
│  completed      │
└─────────────────┘

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

┌─────────────────────────────────────────────────────────────────┐
│                    FASE 3: Account Activation                   │
└─────────────────────────────────────────────────────────────────┘
```

## Técnicas para identificar eventos pivotales

No hay reglas específicas sobre cómo o dónde encontrar eventos pivotales. Sin embargo, hay algunas técnicas y pistas:

### 1. Buscar eventos duplicados

**Señal**: Cuando un solo evento está escrito muchas veces o con varios lenguajes, algo importante está sucediendo.

**Ejemplo**:
```
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│  Payment        │  │  Payment        │  │  Transaction    │
│  completed      │  │  accepted       │  │  finalized      │
└─────────────────┘  └─────────────────┘  └─────────────────┘
```

**Interpretación**: "Payment completed" es un ejemplo que tiene muchos duplicados, indicando que es un evento pivotal.

**Por qué es importante**:
- Múltiples personas lo identificaron independientemente
- Es un punto de cambio significativo en el proceso
- Marca una transición importante

### 2. Buscar handoffs entre personas

**Señal**: Un handoff (traspaso) entre personas puede mostrar que hay un cambio claro en propósito o resultado de negocio.

**Ejemplo**:
```
┌─────────────────┐
│ Potential User  │  ← Persona 1
└─────────────────┘
        ↓
┌─────────────────┐
│  Email verified │  ← EVENTO PIVOTAL (handoff)
└─────────────────┘
        ↓
┌─────────────────┐
│   Known User    │  ← Persona 2
└─────────────────┘
```

**Interpretación**: El cambio de `potential user` a `known user` indica un evento pivotal.

**Por qué es importante**:
- Cambio de responsabilidad
- Diferentes capacidades o permisos
- Transición de estado significativa

### 3. Buscar cambios de lenguaje

**Señal**: Cuando el vocabulario o terminología cambia significativamente, puede indicar un cambio de contexto.

**Ejemplo**:
```
Contexto de Marketing:
┌─────────────────┐
│   Lead created  │
└─────────────────┘
        ↓
┌─────────────────┐
│ Lead qualified  │  ← EVENTO PIVOTAL
└─────────────────┘

Contexto de Ventas:
┌─────────────────┐
│ Prospect        │
│ assigned        │
└─────────────────┘
```

**Interpretación**: El cambio de "lead" a "prospect" indica diferentes contextos de negocio.

### 4. Buscar sistemas externos críticos

**Señal**: Cuando un sistema externo procesa información crítica, a menudo marca un evento pivotal.

**Ejemplo**:
```
┌─────────────────┐
│  Credit card    │
│  entered        │
└─────────────────┘
        ↓
┌─────────────────────────────┐
│  Payment Processor          │  ← Sistema externo crítico
│  (Stripe)                   │
└─────────────────────────────┘
        ↓
┌─────────────────┐
│  Payment        │  ← EVENTO PIVOTAL
│  authorized     │
└─────────────────┘
```

### 5. Buscar cambios de capacidades de negocio

**Señal**: Cuando aparecen nuevas capacidades o funcionalidades disponibles para el usuario.

**Ejemplo**:
```
Antes del evento pivotal:
- Usuario puede navegar el sitio
- Usuario puede ver productos

┌─────────────────┐
│ Account created │  ← EVENTO PIVOTAL
└─────────────────┘

Después del evento pivotal:
- Usuario puede hacer pedidos
- Usuario puede ver historial
- Usuario puede gestionar perfil
```

## Identificación de bounded contexts

### ¿Qué es un bounded context?

Un bounded context es:
- **Límite conceptual**: Donde un modelo de dominio específico es válido
- **Límite de responsabilidad**: Área de responsabilidad clara y específica
- **Límite de lenguaje**: Donde se usa un vocabulario específico consistentemente
- **Límite técnico**: Potencialmente un servicio o aplicación separada

### Relación entre eventos pivotales y bounded contexts

Los eventos pivotales a menudo marcan los límites entre bounded contexts:

```
┌─────────────────────────────────────────────────────────────────┐
│                    BOUNDED CONTEXT: Identity                    │
│                                                                 │
│  ┌─────────────────┐    ┌─────────────────┐                    │
│  │  Name entered   │ -> │  Email verified │ <- EVENTO PIVOTAL  │
│  └─────────────────┘    └─────────────────┘                    │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                   BOUNDED CONTEXT: Billing                     │
│                                                                 │
│  ┌─────────────────┐    ┌─────────────────┐                    │
│  │ Payment entered │ -> │ Payment accepted│ <- EVENTO PIVOTAL  │
│  └─────────────────┘    └─────────────────┘                    │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│                  BOUNDED CONTEXT: Account Mgmt                 │
│                                                                 │
│  ┌─────────────────┐    ┌─────────────────┐                    │
│  │ Profile created │ -> │ Account active  │ <- EVENTO PIVOTAL  │
│  └─────────────────┘    └─────────────────┘                    │
└─────────────────────────────────────────────────────────────────┘
```

## Proceso de identificación

### 1. Revisar todo el flujo
- Examinar la secuencia completa de eventos
- Buscar patrones y agrupaciones naturales
- Identificar puntos de cambio significativo

### 2. Aplicar técnicas de identificación
- Marcar eventos duplicados
- Identificar handoffs entre personas
- Detectar cambios de lenguaje
- Localizar sistemas externos críticos
- Reconocer cambios de capacidades

### 3. Validar con el grupo
- Confirmar que los eventos pivotales tienen sentido
- Asegurar que las fases son lógicas
- Validar que los bounded contexts son coherentes

### 4. Nombrar las fases/contextos
- Dar nombres descriptivos a cada sección
- Usar lenguaje del dominio de negocio
- Asegurar que los nombres son claros y específicos

## Ejemplos de bounded contexts comunes

### E-commerce
```
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│   Catalog       │  │    Shopping     │  │    Checkout     │
│   Management    │  │     Cart        │  │   & Payment     │
└─────────────────┘  └─────────────────┘  └─────────────────┘

┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│   Order         │  │   Shipping &    │  │   Customer      │
│   Management    │  │   Fulfillment   │  │   Service       │
└─────────────────┘  └─────────────────┘  └─────────────────┘
```

### SaaS Application
```
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│   Identity &    │  │   Subscription  │  │   Application   │
│   Access Mgmt   │  │   & Billing     │  │   Core          │
└─────────────────┘  └─────────────────┘  └─────────────────┘

┌─────────────────┐  ┌─────────────────┐
│   Analytics &   │  │   Support &     │
│   Reporting     │  │   Help Desk     │
└─────────────────┘  └─────────────────┘
```

## Beneficios de identificar bounded contexts

### 1. Arquitectura de software
- **Microservicios**: Cada bounded context puede ser un microservicio
- **Equipos**: Cada contexto puede ser responsabilidad de un equipo
- **Bases de datos**: Cada contexto puede tener su propia base de datos
- **APIs**: Interfaces claras entre contextos

### 2. Organización del trabajo
- **Responsabilidades claras**: Cada equipo sabe de qué es responsable
- **Desarrollo paralelo**: Equipos pueden trabajar independientemente
- **Releases independientes**: Cada contexto puede tener su propio ciclo
- **Escalabilidad organizacional**: Facilita crecimiento del equipo

### 3. Mantenimiento y evolución
- **Cambios localizados**: Modificaciones afectan solo un contexto
- **Testing**: Pruebas más focalizadas y manejables
- **Debugging**: Problemas más fáciles de localizar
- **Refactoring**: Mejoras incrementales por contexto

## Mejores prácticas

### ✅ Hacer
1. **Buscar patrones naturales**: Los bounded contexts emergen del dominio
2. **Validar con expertos**: Los contextos deben tener sentido para el negocio
3. **Usar lenguaje del dominio**: Nombres que el negocio entiende
4. **Mantener cohesión**: Cada contexto debe tener un propósito claro
5. **Considerar equipos**: Los contextos deben ser manejables por equipos

### ❌ Evitar
1. **Forzar divisiones**: No crear contextos artificiales
2. **Demasiados contextos**: Evitar fragmentación excesiva
3. **Muy pocos contextos**: No crear contextos demasiado grandes
4. **Ignorar el negocio**: Los contextos técnicos sin sentido de negocio
5. **Cambios frecuentes**: Los bounded contexts deben ser relativamente estables

## Facilitación efectiva

### Preguntas útiles
- "¿Dónde vemos cambios significativos en el proceso?"
- "¿Qué eventos aparecen duplicados o con diferentes nombres?"
- "¿Dónde cambia la responsabilidad entre personas?"
- "¿Qué eventos marcan nuevas capacidades para el usuario?"
- "¿Dónde cambia el vocabulario o lenguaje usado?"

### Técnicas de facilitación
- Usar líneas físicas o virtuales para marcar divisiones
- Permitir debate sobre límites de contextos
- Validar que cada contexto tiene un propósito claro
- Asegurar que los nombres son comprensibles para todos

## Validación del paso

Antes de finalizar el workshop, confirma:

**[Answer]: ¿Se identificaron eventos pivotales claros que marcan cambios significativos?**

**[Answer]: ¿Los bounded contexts tienen nombres descriptivos y comprensibles?**

**[Answer]: ¿Cada bounded context tiene un propósito y responsabilidad claros?**

**[Answer]: ¿Los límites entre contextos son lógicos y naturales?**

**[Answer]: ¿Los bounded contexts son manejables por equipos de desarrollo?**

**[Answer]: ¿El grupo está de acuerdo con la división propuesta?**

## Resultado esperado

Al final de este paso, deberías tener:
- Eventos pivotales claramente identificados y marcados
- Bounded contexts definidos con límites claros
- Nombres descriptivos para cada contexto o fase
- Comprensión compartida de responsabilidades por contexto
- Base sólida para decisiones arquitectónicas
- Roadmap claro para desarrollo e implementación
- Identificación de equipos y ownership por contexto
- Interfaces y contratos entre contextos definidos

## Conclusión del workshop

Con la identificación de eventos pivotales y bounded contexts, el workshop de EventStorming está completo. Los artefactos generados proporcionan:

- **Mapa completo del dominio**: Eventos, actores, sistemas y contextos
- **Base para arquitectura**: Bounded contexts como guía para microservicios
- **Plan de trabajo**: Contextos como unidades de desarrollo
- **Entendimiento compartido**: Lenguaje común entre equipos
- **Identificación de riesgos**: Hotspots y dependencias externas

El siguiente paso es el seguimiento post-workshop para resolver hotspots y comenzar la implementación basada en los bounded contexts identificados.
