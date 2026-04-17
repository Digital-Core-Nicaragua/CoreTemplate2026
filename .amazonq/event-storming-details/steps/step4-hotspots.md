# Step 4: Hotspots - Identificación de puntos críticos

## Objetivo del paso

Las preguntas o desafíos pueden ser capturados usando notas adhesivas rosadas. Estas son conocidas como "hotspots" (puntos críticos). Siempre que surja algo que no puedas responder ahora, o que sepas que es un problema, captúralo como una nota adhesiva rosada y colócala en el tablero cerca del evento al que se relaciona.

## Representación visual

Los hotspots se representan con **notas adhesivas rosadas/magenta** (pink sticky notes).

```
┌─────────────────┐
│  Known User     │  ← Nota amarilla (persona)
└─────────────────┘
        ↓
┌─────────────────┐
│  Credit card    │  ← Nota naranja (evento)
│  entered        │
└─────────────────┘
        ↓
┌─────────────────┐
│  How do we      │  ← Nota rosada (hotspot)
│  handle PCI     │
│  compliance?    │
└─────────────────┘
```

## ¿Qué es un hotspot?

Un hotspot puede ser:
- **Una pregunta sin respuesta**: Algo que el equipo no puede responder en este momento
- **Un problema conocido**: Algo que sabes que es un desafío o punto de dolor
- **Una incertidumbre**: Áreas donde hay falta de claridad o consenso
- **Un riesgo**: Algo que podría causar problemas en el futuro
- **Una dependencia externa**: Algo que requiere información o decisión de otros
- **Un conflicto**: Diferentes opiniones sobre cómo funciona algo
- **Una limitación técnica**: Restricciones conocidas del sistema actual

## Propósito de los hotspots

### 1. Herramienta de time-boxing
Los hotspots son una buena herramienta para limitar el tiempo de discusiones que aparentemente no tienen fin.

**Escenario típico**:
- Una discusión comienza sobre un tema complejo
- Después de unos minutos, no hay resolución a la vista
- El facilitador pide a los participantes capturar la pregunta como un hotspot
- El equipo continúa con el siguiente tema

### 2. Captura de conocimiento
- Las conversaciones son valiosas
- Pero una conversación que dura 30 minutos sin resolución o decisión no ayuda
- Los hotspots permiten capturar el tema para revisarlo después

### 3. Seguimiento post-workshop
El equipo necesitará volver a las preguntas en los hotspots después del workshop.

## Ejemplos de hotspots

### Preguntas técnicas
```
┌─────────────────────────────┐
│ How do we handle payment    │
│ processing failures?        │
└─────────────────────────────┘

┌─────────────────────────────┐
│ What happens if the email   │
│ service is down?            │
└─────────────────────────────┘

┌─────────────────────────────┐
│ How do we ensure GDPR       │
│ compliance for user data?   │
└─────────────────────────────┘
```

### Preguntas de negocio
```
┌─────────────────────────────┐
│ What's the refund policy    │
│ for cancelled orders?       │
└─────────────────────────────┘

┌─────────────────────────────┐
│ Who approves discount       │
│ codes over 50%?             │
└─────────────────────────────┘

┌─────────────────────────────┐
│ How long do we keep         │
│ inactive user accounts?     │
└─────────────────────────────┘
```

### Problemas conocidos
```
┌─────────────────────────────┐
│ Current system takes 5 mins │
│ to process - too slow!      │
└─────────────────────────────┘

┌─────────────────────────────┐
│ Legacy database can't       │
│ handle concurrent updates   │
└─────────────────────────────┘

┌─────────────────────────────┐
│ Integration with X system   │
│ fails 20% of the time       │
└─────────────────────────────┘
```

### Conflictos y desacuerdos
```
┌─────────────────────────────┐
│ Marketing says users can    │
│ change email, IT says no    │
└─────────────────────────────┘

┌─────────────────────────────┐
│ Different teams have        │
│ different user definitions  │
└─────────────────────────────┘
```

## Señal de alerta: Más rosado que naranja

### Situación problemática
Es posible que un equipo comience a escribir y secuenciar eventos y termine con más notas adhesivas rosadas que naranjas.

### ¿Qué significa esto?
Esta es una señal clara de que el equipo tiene mucho más trabajo que hacer antes de ejecutar una sesión exitosa de EventStorming.

### Posibles causas
1. **Falta de expertos de dominio**: Tener más preguntas que respuestas podría ser una señal de que necesitas más expertos de dominio en la sala
2. **Dominio poco entendido**: El equipo no tiene suficiente conocimiento del proceso de negocio
3. **Proceso mal definido**: El proceso de negocio en sí no está bien definido o documentado
4. **Preparación insuficiente**: El equipo no hizo suficiente trabajo previo antes del workshop

### Acciones correctivas
- Invitar a más expertos de dominio
- Realizar sesiones de investigación previas
- Dividir el workshop en sesiones más pequeñas y específicas
- Posponer hasta tener mejor preparación

## Cuándo crear hotspots

### Durante discusiones largas
- Cuando una discusión supera los 5-10 minutos sin resolución
- Cuando hay desacuerdo y no se puede resolver inmediatamente
- Cuando se necesita información externa

### Al identificar problemas
- Cuando alguien menciona "esto siempre falla"
- Cuando se identifica un punto de dolor conocido
- Cuando hay limitaciones técnicas evidentes

### Al encontrar lagunas de conocimiento
- Cuando nadie sabe cómo funciona algo
- Cuando hay incertidumbre sobre reglas de negocio
- Cuando se necesita validar suposiciones

## Mejores prácticas para hotspots

### ✅ Hacer
1. **Ser específico**: Escribir preguntas claras y específicas
2. **Capturar rápidamente**: No perder tiempo perfeccionando la redacción
3. **Ubicar cerca del evento**: Colocar el hotspot cerca del evento relacionado
4. **Incluir contexto**: Suficiente información para entender después
5. **Priorizar después**: No todas las preguntas son igual de importantes

### ❌ Evitar
1. **Intentar resolver todo**: Los hotspots son para capturar, no resolver
2. **Preguntas demasiado genéricas**: "¿Cómo funciona esto?" es muy vago
3. **Ignorar hotspots**: No seguir adelante sin capturar preguntas importantes
4. **Debates infinitos**: Usar hotspots para time-box discusiones

## Tipos de hotspots por categoría

### Técnicos
- Limitaciones de rendimiento
- Problemas de integración
- Cuestiones de seguridad
- Escalabilidad
- Manejo de errores

### Negocio
- Reglas de negocio poco claras
- Políticas no definidas
- Procesos de aprobación
- Excepciones al flujo normal
- Métricas y KPIs

### Organizacionales
- Responsabilidades poco claras
- Procesos de comunicación
- Escalación de problemas
- Autoridad para tomar decisiones
- Coordinación entre equipos

### Regulatorios/Compliance
- Requisitos legales
- Auditoría y trazabilidad
- Privacidad de datos
- Retención de información
- Reportes regulatorios

## Facilitación de hotspots

### Frases útiles del facilitador
- "Esa es una gran pregunta, capturémosla como hotspot"
- "Veo que hay desacuerdo aquí, pongamos un hotspot"
- "Esto suena como algo que necesitamos investigar después"
- "Marquemos esto como un problema conocido"

### Manejo de resistencia
Algunos participantes pueden querer resolver todo inmediatamente:
- Explicar el valor de time-boxing
- Asegurar que los hotspots serán revisados
- Mantener el momentum del workshop
- Recordar los objetivos de la sesión

## Validación del paso

Antes de continuar al Step 5, confirma:

**[Answer]: ¿Se capturaron las preguntas importantes que surgieron durante la discusión?**

**[Answer]: ¿Los hotspots están ubicados cerca de los eventos relacionados?**

**[Answer]: ¿Las preguntas son específicas y comprensibles?**

**[Answer]: ¿Se evitaron debates largos usando hotspots para time-boxing?**

**[Answer]: ¿Hay un balance razonable entre eventos (naranja) y hotspots (rosado)?**

**[Answer]: ¿Se identificaron tanto problemas técnicos como de negocio?**

## Resultado esperado

Al final de este paso, deberías tener:
- Hotspots claramente identificados con sticky notes rosados
- Preguntas específicas y comprensibles
- Problemas conocidos documentados
- Áreas de incertidumbre capturadas
- Conflictos y desacuerdos marcados para resolución posterior
- Lista priorizada de temas para seguimiento post-workshop
- Workshop con momentum mantenido (no bloqueado por debates largos)

## Próximo paso

Una vez que los hotspots están capturados y el equipo ha mantenido el momentum, es tiempo de identificar sistemas externos en Step 5: External Systems.
