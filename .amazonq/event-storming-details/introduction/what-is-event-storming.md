# ¿Qué es EventStorming?

## Definición

EventStorming es un enfoque basado en talleres colaborativos utilizado para desglosar dominios de negocio complejos con el objetivo de alcanzar un entendimiento compartido entre equipos y organizaciones.

## Problema que resuelve

### Desalineación entre equipos
Es común que diferentes equipos dentro de una empresa tengan su propia comprensión del dominio del problema:
- Equipos técnicos tienen una visión
- Equipos de negocio tienen otra visión
- Equipos de operaciones tienen su propia perspectiva

### Silos organizacionales
Cuando expertos de dominio y equipos técnicos operan en silos separados:
- Se pierden detalles importantes
- Se hacen suposiciones incorrectas
- La comunicación es ineficiente

### Resultado de la desalineación
- Productos que no resuelven el problema de negocio real
- Acumulación de deuda técnica
- Frustración organizacional
- Retrabajo constante

## Beneficio clave

> "Es el (mal)entendimiento de los desarrolladores, no el conocimiento de los expertos del dominio, lo que se libera en producción." - Alberto Brandolini, creador de EventStorming

**Solución**: Convertir a tus desarrolladores y equipos en expertos del dominio.

## Explorando Límites (Boundaries)

### Conceptos fundamentales

#### Dominio de negocio
El área en la que tu empresa trabaja para proporcionar valor a los clientes.

#### Subdominios
Áreas específicas dentro del dominio principal.

**Ejemplo en e-commerce**:
- Envíos
- Facturación
- Soporte al cliente
- Gestión de inventario
- Marketing

#### Límites (Boundaries)
Cada subdominio puede contener múltiples límites que necesitan soluciones específicas.

EventStorming ayuda a identificar, definir y desglosar un dominio y sus subdominios para construir las soluciones correctas.

## Cómo funciona EventStorming

### Enfoque colaborativo

1. **Reunir expertos**: Juntar expertos de dominio en la misma sala física o virtual
2. **Mapeo colaborativo**: Mapear un proceso de negocio de forma colaborativa
3. **Hacer preguntas**: Facilitar que las personas hagan preguntas, secuencien eventos y construyan un modelo juntos
4. **Enfoque en entendimiento**: EventStorming se centra en *entender* antes que en *solucionar*

### Uso de eventos de dominio

- Los eventos de dominio se usan como lenguaje común para describir el proceso a modelar
- Son hechos inmutables que ocurrieron en el pasado
- Estos límites y eventos sirven como punto de partida para construir sistemas independientes y débilmente acoplados

### Relación con arquitecturas event-driven

EventStorming no está limitado a arquitecturas event-driven, pero es un ajuste natural:
- Si estás usando arquitecturas event-driven, EventStorming es altamente recomendado
- Si planeas adoptar una estrategia event-driven, comienza con EventStorming
- También es útil para arquitecturas tradicionales que necesitan mejor entendimiento del dominio

## Cuándo usar EventStorming

### Escenarios ideales

- **Aplicaciones monolíticas**: Para entender mejor el dominio antes de refactorizar
- **Microservicios acoplados**: Para identificar límites correctos y reducir acoplamiento
- **Arquitecturas event-driven**: Como punto de partida natural para diseño
- **Desalineación organizacional**: Cuando hay falta de entendimiento compartido
- **Proyectos complejos**: Al inicio de proyectos que requieren entendimiento profundo del dominio
- **Modernización**: Antes de migrar sistemas legacy

### Señales de que necesitas EventStorming

- Equipos técnicos y de negocio hablan lenguajes diferentes
- Frecuentes malentendidos sobre requisitos
- Sistemas con límites poco claros
- Dificultad para escalar o modificar el sistema
- Acumulación de deuda técnica por suposiciones incorrectas

## Recursos adicionales

- [EventStorming Book](https://www.eventstorming.com/book/)
- [AWS EDA on AWS Documentation](https://aws-samples.github.io/eda-on-aws/eventstorming/)
- [Serverless Land - Intro to EDA](https://serverlessland.com/event-driven-architecture)
