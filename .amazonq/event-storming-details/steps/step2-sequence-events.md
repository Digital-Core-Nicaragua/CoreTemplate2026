# Step 2: Sequence Events - Secuenciación de eventos

## Objetivo del paso

Una vez que las personas han creado suficientes eventos, es tiempo de secuenciarlos. La secuenciación no es más que hablar sobre el orden en que los eventos ocurren relativos unos a otros. Aunque este proceso suena simple, debería generar muchas conversaciones y debates.

## Actividades principales

### 1. Discusión del lenguaje
Durante esta etapa habrá eventos duplicados que están escritos de manera similar, o completamente diferente. El grupo necesita:
- Discutir el lenguaje usado en los eventos
- Acordar palabras o frases
- Identificar qué eventos son duplicados
- Determinar dónde viven los eventos a lo largo de la línea de tiempo

### 2. Consolidación de eventos

**Ejemplo**: Cuatro eventos diferentes relacionados con un pago con tarjeta de crédito:
- `Payment processed`
- `Credit card charged`
- `Payment accepted`
- `Transaction completed`

Todos tienen el mismo tema, pero usan lenguaje diferente. Puede tener sentido:
- **Consolidar** múltiples eventos en un solo evento que represente cierta complejidad
- **Dividir** una pila de eventos que parecen ser lo mismo en múltiples eventos usando lenguaje ligeramente diferente

```
Antes de consolidar:
┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐
│ Payment          │  │ Credit card      │  │ Payment          │  │ Transaction      │
│ processed        │  │ charged          │  │ accepted         │  │ completed        │
└──────────────────┘  └──────────────────┘  └──────────────────┘  └──────────────────┘

Después de consolidar:
┌──────────────────┐
│ Payment          │
│ accepted         │
└──────────────────┘
```

## Manejo de flujos paralelos y ramificaciones lógicas

### Pregunta común
"Hacemos *esto* en algunos casos, pero depende de *aquello*"
"Este evento sucede cuando esta otra cosa falla, y no sucede a menudo, pero es realmente importante"

### Solución: Happy Path vs Sad Path

**Happy Path**: El flujo principal o exitoso del proceso
**Sad Path**: Flujos alternativos, errores o excepciones

**Técnica simple**: Colocar los eventos paralelos **arriba o abajo** del flujo del "happy path"

### Ejemplo: Detección de email duplicado

```
                    ┌──────────────────────┐
    Sad Path   →    │ Duplicate email      │
                    │ detected             │
                    └──────────────────────┘
                              ↓
                    ┌──────────────────────┐
                    │ Security scan        │
                    │ triggered            │
                    └──────────────────────┘

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

                    ┌──────────────────────┐
    Happy Path →    │ Email verified       │
                    └──────────────────────┘
                              ↓
                    ┌──────────────────────┐
                    │ Account created      │
                    └──────────────────────┘
```

### Ejemplo: Código de regalo

```
                    ┌──────────────────────┐
    Sad Path   →    │ Gift code            │
                    │ rejected             │
                    └──────────────────────┘

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

                    ┌──────────────────────┐
    Happy Path →    │ Gift code            │
                    │ redeemed             │
                    └──────────────────────┘
```

## Proceso de secuenciación

### 1. Identificar evento inicial
- Buscar el primer evento lógico del proceso
- Puede haber múltiples puntos de entrada
- Colocar en el extremo izquierdo

### 2. Seguir el happy path
- Identificar la secuencia principal de eventos
- Colocar eventos de izquierda a derecha cronológicamente
- No preocuparse por excepciones todavía

### 3. Agregar sad paths
- Identificar eventos alternativos o de error
- Colocar arriba o abajo del happy path
- Usar líneas o flechas para mostrar conexiones

### 4. Consolidar duplicados
- Identificar eventos que representan lo mismo
- Acordar lenguaje común
- Mantener solo una versión de cada evento único

## Principios fundamentales

### No hay reglas absolutas
**No hay reglas absolutas** con EventStorming y secuenciación. La parte más importante del proceso es que los expertos de dominio:
- Exploren juntos
- Aprendan juntos
- Entiendan juntos

### Nivel de detalle según el tipo de sesión

#### Big Picture Sessions
- Suficiente capturar solo los grandes eventos
- No preocuparse demasiado por todas las diferentes permutaciones y caminos
- Muchos detalles comenzarán a aparecer en sesiones más profundas

#### Process y Detail Level Sessions
- Mayor profundidad en permutaciones
- Exploración detallada de caminos alternativos
- Análisis exhaustivo de casos edge

## Mejores prácticas

### ✅ Hacer
1. **Fomentar el debate**: Las discusiones sobre orden y lenguaje son valiosas
2. **Documentar alternativas**: No descartar los sad paths, documentarlos
3. **Flexibilidad**: Estar dispuesto a reorganizar eventos múltiples veces
4. **Consenso**: Buscar acuerdo del grupo en el lenguaje y secuencia
5. **Visualización clara**: Usar espacio vertical para mostrar flujos paralelos
6. **Iteración**: La secuencia puede cambiar a medida que se descubre más información

### ❌ Evitar
1. **Perfeccionismo**: No buscar la secuencia perfecta inmediatamente
2. **Ignorar sad paths**: Los flujos de error son importantes
3. **Debates infinitos**: Time-box las discusiones, usar hotspots si es necesario
4. **Rigidez**: La secuencia puede cambiar, mantener flexibilidad

## Facilitación efectiva

### Preguntas útiles
- "¿Qué sucede antes de este evento?"
- "¿Qué puede salir mal aquí?"
- "¿Hay casos donde esto no sucede?"
- "¿Todos están de acuerdo con este orden?"
- "¿Qué pasa si...?"

### Manejo de debates
- Permitir discusión pero time-box
- Usar hotspots para temas complejos
- Buscar consenso, no perfección
- Documentar decisiones tomadas

## Validación del paso

Antes de continuar al Step 3, confirma:

**[Answer]: ¿Los eventos están ordenados cronológicamente de izquierda a derecha?**

**[Answer]: ¿Se identificó claramente el happy path principal?**

**[Answer]: ¿Se documentaron los sad paths y flujos alternativos?**

**[Answer]: ¿Se consolidaron los eventos duplicados con lenguaje acordado?**

**[Answer]: ¿El grupo está de acuerdo con la secuencia general?**

**[Answer]: ¿Se capturaron las decisiones de lenguaje importantes?**

## Resultado esperado

Al final de este paso, deberías tener:
- Eventos ordenados cronológicamente en una línea de tiempo
- Lenguaje acordado para eventos similares
- Duplicados identificados y consolidados
- Flujos paralelos y sad paths documentados
- Happy path claramente identificado
- Conversaciones y debates que generan entendimiento compartido
- Base sólida para agregar actores en el siguiente paso

## Próximo paso

Una vez que los eventos están secuenciados y el grupo ha acordado el flujo principal y alternativo, es tiempo de identificar las personas o actores involucrados en el proceso en Step 3: People.
