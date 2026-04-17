# Event Storming Facilitation Guide

## Purpose
This guide provides facilitators with best practices, common pitfalls, and facilitation techniques for running successful Event Storming workshops.

## Core Facilitation Principles

### 1. Diversidad Cross-Funcional es Obligatoria
- **Regla de Oro**: Nunca ejecutar Event Storming con un solo equipo
- **Razón**: El valor principal es el entendimiento compartido entre áreas
- **Participantes Necesarios**: Desarrollo, Negocio, Operaciones, UX, Producto
- **Señal de Alerta**: Si todos son del mismo departamento, posponer el workshop

### 2. Time-Boxing es Crítico
- **Objetivo**: Mantener momentum y energía
- **Técnica**: Usar hotspots para capturar preguntas sin bloquear progreso
- **Regla**: No resolver todo en el momento, documentar para después
- **Duración Típica**: 2-4 horas para workshop completo

### 3. Eventos son Inmutables
- **Formato**: Siempre en tiempo pasado
- **Ejemplo Correcto**: "Usuario Registrado", "Pago Procesado"
- **Ejemplo Incorrecto**: "Registrar Usuario", "Procesar Pago"
- **Razón**: Eventos son hechos que ya ocurrieron, no acciones

### 4. No Asumir Conocimiento
- **Validar**: Comprensión en cada paso
- **Preguntar**: "¿Todos entienden qué es un evento de dominio?"
- **Confirmar**: Antes de avanzar al siguiente paso
- **Aclarar**: Términos técnicos y de negocio

### 5. Capturar Todo
- **Hotspots**: Para preguntas y problemas
- **No Bloquear**: El progreso del workshop
- **Documentar**: Todas las decisiones y respuestas
- **Seguimiento**: Post-workshop para resolver hotspots

## Técnicas de Facilitación por Paso

### Step 0: Invitations
**Objetivo**: Asegurar el equipo correcto

**Preguntas Clave**:
- ¿Quién toma decisiones de negocio?
- ¿Quién implementará el sistema?
- ¿Quién operará el sistema?
- ¿Quién usará el sistema?

**Señales de Éxito**:
- Representación de al menos 3 áreas funcionales
- Mezcla de roles técnicos y de negocio
- Participantes con autoridad para tomar decisiones

### Step 1: Crowdsource Events
**Objetivo**: Generar eventos sin filtro ni orden

**Técnicas**:
- Brainstorming silencioso (5-10 minutos)
- Sticky notes naranjas
- Sin discusión inicial
- Colocar en la pared sin orden

**Señales de Éxito**:
- 30-50+ eventos generados
- Eventos en tiempo pasado
- Mezcla de eventos de negocio y técnicos
- Participación de todos

**Errores Comunes**:
- Eventos como comandos ("Crear Usuario")
- Detalles técnicos ("API llamada")
- Ordenar prematuramente

### Step 2: Sequence Events
**Objetivo**: Crear flujo temporal coherente

**Técnicas**:
- Identificar evento inicial
- Seguir happy path primero
- Agregar sad paths después
- Consolidar duplicados

**Señales de Éxito**:
- Flujo lógico de izquierda a derecha
- Happy path claro
- Sad paths identificados
- Duplicados consolidados

**Errores Comunes**:
- Saltar directamente a excepciones
- No consolidar eventos similares
- Perder el hilo temporal

### Step 3: People
**Objetivo**: Identificar quién inicia eventos

**Técnicas**:
- Sticky notes amarillos
- Colocar encima/al lado del evento
- Identificar evolución de usuarios
- Mapear handoffs entre personas

**Señales de Éxito**:
- Cada evento tiene un actor
- Roles claramente definidos
- Evolución de usuarios visible
- Handoffs identificados

**Errores Comunes**:
- Roles demasiado genéricos ("Usuario")
- No identificar evolución
- Confundir sistemas con personas

### Step 4: Hotspots
**Objetivo**: Capturar preguntas sin bloquear

**Técnicas**:
- Sticky notes rosas
- Capturar rápidamente
- No resolver en el momento
- Priorizar para seguimiento

**Señales de Éxito**:
- 10-20 hotspots capturados
- Preguntas específicas
- Áreas de incertidumbre identificadas
- Workshop mantiene momentum

**Errores Comunes**:
- Intentar resolver todo
- Perder tiempo en debates
- No documentar preguntas

## Manejo de Situaciones Comunes

### Participante Dominante
**Síntoma**: Una persona habla constantemente
**Solución**: 
- Usar brainstorming silencioso
- Rotar quién explica eventos
- Preguntar directamente a otros

### Debate Técnico Prolongado
**Síntoma**: Discusión técnica detallada
**Solución**:
- Crear hotspot
- Time-box la discusión
- Mover a seguimiento post-workshop

### Confusión sobre Eventos
**Síntoma**: Eventos como comandos o procesos
**Solución**:
- Re-explicar con ejemplos
- Mostrar formato correcto
- Practicar con evento simple

### Falta de Participación
**Síntoma**: Algunos no contribuyen
**Solución**:
- Preguntar directamente
- Asignar áreas específicas
- Validar comprensión

### Scope Creep
**Síntoma**: Workshop se expande sin control
**Solución**:
- Re-confirmar objetivos
- Crear hotspot para scope adicional
- Mantener foco en alcance original

## Materiales Necesarios

### Físico (Presencial)
- Pared grande o superficie larga
- Sticky notes: Naranjas (eventos), Amarillos (personas), Rosas (hotspots/sistemas)
- Marcadores negros
- Cinta adhesiva
- Espacio para 5-15 personas

### Virtual (Remoto)
- Herramienta de colaboración (Miro, Mural, etc.)
- Templates preparados
- Breakout rooms opcionales
- Grabación de sesión

## Checklist Pre-Workshop

- [ ] Participantes confirmados (cross-funcional)
- [ ] Objetivos claros definidos
- [ ] Alcance acordado
- [ ] Materiales preparados
- [ ] Espacio reservado (2-4 horas)
- [ ] Introducción preparada
- [ ] Ejemplos listos para explicar conceptos

## Checklist Post-Workshop

- [ ] Fotografiar/exportar resultado
- [ ] Documentar eventos en formato digital
- [ ] Listar hotspots priorizados
- [ ] Asignar responsables para seguimiento
- [ ] Agendar sesión de revisión
- [ ] Compartir artefactos con participantes

## Métricas de Éxito

### Durante el Workshop
- Participación activa de todos
- Debates constructivos
- Momentum mantenido
- Hotspots capturados (no resueltos)

### Post-Workshop
- Bounded contexts identificados
- Eventos documentados (30-100+)
- Hotspots priorizados
- Participantes comprenden el dominio
- Artefactos utilizables para diseño

## Antipatrones a Evitar

### ❌ Workshop con un Solo Equipo
**Problema**: Pierde el valor principal de entendimiento compartido
**Solución**: Posponer hasta tener participación cross-funcional

### ❌ Resolver Todos los Hotspots
**Problema**: Pierde momentum, workshop se extiende indefinidamente
**Solución**: Capturar y priorizar para seguimiento

### ❌ Saltar a Soluciones Técnicas
**Problema**: Pierde foco en dominio de negocio
**Solución**: Mantener conversación en eventos de dominio

### ❌ No Consolidar Duplicados
**Problema**: Ruido visual, dificulta identificar pivotal events
**Solución**: Consolidar activamente en Step 2

### ❌ Facilitador como Experto
**Problema**: Participantes no se apropian del resultado
**Solución**: Facilitador guía proceso, participantes son expertos

## Adaptaciones por Tipo de Sesión

### Big Picture (Exploración)
- Foco: Amplitud sobre profundidad
- Duración: 2-3 horas
- Resultado: Vista general del dominio

### Process Modeling (Detalle)
- Foco: Profundidad en proceso específico
- Duración: 3-4 horas
- Resultado: Flujo detallado con variantes

### Software Design (Implementación)
- Foco: Bounded contexts y APIs
- Duración: 4-6 horas
- Resultado: Diseño listo para implementar

## Recursos Adicionales

### Lecturas Recomendadas
- "Introducing EventStorming" - Alberto Brandolini
- AWS Event Storming Documentation
- Domain-Driven Design - Eric Evans

### Comunidad
- EventStorming Google Group
- DDD Community
- AWS Architecture Blog
