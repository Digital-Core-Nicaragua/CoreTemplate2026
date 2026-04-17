# Tipos de Sesiones EventStorming

EventStorming puede ejecutarse en diferentes niveles de detalle dependiendo del objetivo y la etapa del proyecto. Cada tipo de sesión tiene características específicas en cuanto a participantes, enfoque y resultados esperados.

## 1. Big Picture (Vista General)

### Objetivo
Mapear un sistema o proceso grande y complejo a alto nivel para obtener una visión general del dominio.

### Características

#### Nivel de detalle
- **Alto nivel**: Vista panorámica del sistema completo
- **Amplitud sobre profundidad**: Cubrir todo el alcance sin entrar en detalles
- **Exploración inicial**: Primera aproximación al dominio

#### Participantes
- **Mayoría de expertos de dominio**: Independiente de la familia de trabajo
- **Representación cross-funcional**: Negocio, producto, operaciones, soporte
- **Menos técnicos**: Foco en conocimiento del negocio

#### Enfoque temporal
- **Estado actual**: Cómo funciona el sistema hoy
- **As-is**: Mapeo de la realidad actual sin proponer cambios
- **Descubrimiento**: Entender qué existe antes de proponer mejoras

#### Resultado esperado
- Identificación de subdominios principales
- Límites de negocio visibles
- Vista completa del flujo end-to-end
- Áreas de oportunidad identificadas
- Base para sesiones más detalladas

#### Duración típica
2-3 horas

### Cuándo usar Big Picture
- Al inicio de un proyecto nuevo
- Para entender un dominio desconocido
- Cuando hay desalineación organizacional
- Antes de decidir arquitectura
- Para identificar áreas que necesitan profundización

---

## 2. Process (Proceso)

### Objetivo
Profundizar en detalles de subdominios específicos después de identificar áreas de interés en la sesión Big Picture.

### Características

#### Nivel de detalle
- **Nivel intermedio**: Balance entre amplitud y profundidad
- **Foco en procesos específicos**: Un subdominio o flujo particular
- **Más granularidad**: Eventos más detallados que en Big Picture

#### Participantes
- **Mezcla equilibrada**: Expertos de dominio e ingenieros
- **Especialistas del área**: Personas con conocimiento profundo del proceso
- **Colaboración técnica-negocio**: Ambas perspectivas son críticas

#### Enfoque temporal
- **Combina actual y futuro**: As-is + to-be
- **Estado actual**: Cómo funciona hoy
- **Estado futuro**: Cómo debería funcionar
- **Identificación de mejoras**: Oportunidades de optimización

#### Resultado esperado
- Comprensión detallada de procesos específicos
- Happy paths y sad paths bien definidos
- Variantes y excepciones identificadas
- Oportunidades de mejora claras
- Base para diseño técnico

#### Duración típica
3-4 horas

### Cuándo usar Process
- Después de una sesión Big Picture
- Para profundizar en un subdominio específico
- Cuando se necesita entender variantes del proceso
- Antes de comenzar diseño técnico
- Para optimizar procesos existentes

---

## 3. Design (Diseño)

### Objetivo
Preparación técnica para construcción de un sistema específico, con nivel de detalle suficiente para comenzar desarrollo.

### Características

#### Nivel de detalle
- **Nivel técnico**: Detalle suficiente para implementar
- **Foco en construcción**: Lo que se va a construir
- **Decisiones técnicas**: Arquitectura, tecnologías, APIs

#### Participantes
- **Equipo técnico de entrega**: Desarrolladores, arquitectos, DevOps
- **Menos stakeholders no técnicos**: Foco en implementación
- **Product Owner presente**: Para validar decisiones

#### Enfoque temporal
- **Estado futuro**: Lo que se va a construir
- **To-be exclusivamente**: No se mapea el estado actual
- **Implementación**: Foco en cómo construir la solución

#### Resultado esperado
- Historias de usuario listas para desarrollo
- Planificación de sprint
- Template para POC o primera iteración
- APIs y contratos definidos
- Bounded contexts técnicos claros
- Decisiones arquitectónicas documentadas

#### Duración típica
4-6 horas (puede requerir múltiples sesiones)

### Cuándo usar Design
- Después de sesiones Big Picture y Process
- Cuando el equipo está listo para comenzar desarrollo
- Para definir arquitectura técnica
- Al planificar un sprint o release
- Para crear POC o MVP

---

## Comparación de tipos de sesión

| Aspecto | Big Picture | Process | Design |
|---------|-------------|---------|--------|
| **Nivel** | Alto | Intermedio | Detallado |
| **Participantes** | Mayormente negocio | Mezcla | Mayormente técnicos |
| **Enfoque** | Estado actual | Actual + Futuro | Estado futuro |
| **Duración** | 2-3 horas | 3-4 horas | 4-6 horas |
| **Resultado** | Subdominios | Procesos detallados | Historias + Arquitectura |
| **Cuándo** | Inicio | Después de Big Picture | Antes de desarrollo |

## Progresión típica

### Flujo recomendado

```
1. Big Picture
   ↓
   Identificar subdominios y áreas de interés
   ↓
2. Process (para cada subdominio prioritario)
   ↓
   Entender procesos en detalle
   ↓
3. Design (para cada área a implementar)
   ↓
   Preparar para desarrollo
```

### Iteración

No es necesario completar todos los niveles de una vez:
- Puedes hacer Big Picture y esperar antes de Process
- Puedes hacer Process solo para áreas prioritarias
- Puedes hacer Design de forma incremental por bounded context

## Selección del tipo de sesión

### Preguntas para decidir

1. **¿Cuál es tu objetivo principal?**
   - Entender el dominio → Big Picture
   - Optimizar un proceso → Process
   - Comenzar desarrollo → Design

2. **¿Qué tan bien conoces el dominio?**
   - Poco conocimiento → Big Picture
   - Conocimiento general → Process
   - Conocimiento detallado → Design

3. **¿Quiénes pueden participar?**
   - Mayormente negocio → Big Picture
   - Mezcla equilibrada → Process
   - Mayormente técnicos → Design

4. **¿Cuánto tiempo tienes?**
   - 2-3 horas → Big Picture
   - 3-4 horas → Process
   - 4-6 horas → Design

5. **¿Qué necesitas como resultado?**
   - Vista general → Big Picture
   - Procesos detallados → Process
   - Historias de usuario → Design

## Adaptaciones

Cada tipo de sesión puede adaptarse según:
- Tamaño de la organización
- Complejidad del dominio
- Experiencia del equipo con EventStorming
- Restricciones de tiempo
- Modalidad (presencial vs remoto)

Lo importante es mantener el enfoque colaborativo y el objetivo de entendimiento compartido en todos los tipos de sesión.
