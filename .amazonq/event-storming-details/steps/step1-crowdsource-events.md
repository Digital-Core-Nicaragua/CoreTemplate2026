# Step 1: Crowdsource Events - Recolección colaborativa de eventos

## Objetivo del paso

El primer paso en cualquier sesión de EventStorming es la recolección colaborativa de eventos. Los individuos hacen una lluvia de ideas sobre eventos y los escriben en notas adhesivas naranjas.

## Cómo comenzar

Para empezar, piensa en el **primer y último evento** en tu dominio de negocio.

**Ejemplo**: Al modelar "onboarding de usuarios"
- **Primer evento**: `Name Entered` (nombre ingresado)
- **Último evento**: `Converted to Active` (convertido a activo)

Habrá muchos más eventos entre estos eventos de inicio y fin. El trabajo de todos es hacer una lluvia de ideas de eventos sin preocuparse por:
- Nombrarlos correctamente
- Duplicados
- Orden

**¡No lo compliques demasiado!** Muchas personas se preocupan por hacer las cosas "correctamente". Lo más importante en esta fase es **"hacer"**.

## ¿Qué es un evento de dominio?

En EventStorming y arquitecturas event-driven, definimos un evento como:

**Evento de dominio** (sustantivo):
1. Algo que le importa a un experto de dominio
2. Un hecho inmutable que ha ocurrido en el pasado

### Características de los eventos

#### Escritos en tiempo pasado
- Los eventos son hechos que ya han ocurrido y no pueden ser cambiados
- Representan algo que **ya sucedió**

**Ejemplos correctos**:
- ✅ `User Registered` (Usuario Registrado)
- ✅ `Payment Processed` (Pago Procesado)
- ✅ `Order Shipped` (Pedido Enviado)

**Ejemplos incorrectos**:
- ❌ `Register User` (Registrar Usuario) - es un comando, no un evento
- ❌ `Process Payment` (Procesar Pago) - es una acción, no un evento
- ❌ `Shipping` (Envío) - es un proceso, no un evento

#### Definidos por expertos de dominio
- Lo que constituye un evento de dominio es definido por ti y tus expertos de dominio
- Lo que puede ser irrelevante en un dominio de negocio puede ser importante en otro

**Ejemplo de contexto**:
- **E-commerce**: ¿Es un evento que el usuario haga scroll hasta el final de una página web? Probablemente no
- **Plataforma de analítica web**: ¿Es un evento que el usuario llegue al final de la página? Muy probablemente sí

Es responsabilidad tuya y de los otros expertos de dominio definir qué es o no es un evento de dominio.

## Representación visual

Los eventos de dominio se representan con **notas adhesivas naranjas** (orange sticky notes).

```
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│  Name Entered   │  │  Email Verified │  │ Converted to    │
│    (Naranja)    │  │    (Naranja)    │  │     Active      │
└─────────────────┘  └─────────────────┘  └─────────────────┘
```

## Proceso de brainstorming

### 1. Brainstorming silencioso (5-10 minutos)
- Cada participante escribe eventos individualmente
- Sin discusión durante esta fase
- Una idea por sticky note
- Usar marcador negro, letra clara

### 2. Colocación en la pared
- Todos colocan sus sticky notes en la pared
- Sin orden específico inicialmente
- Sin discusión sobre duplicados todavía
- Simplemente "dump" de todas las ideas

### 3. Lectura rápida
- Alguien lee todos los eventos en voz alta
- Sin debate, solo lectura
- Identificar eventos que necesitan clarificación

## Gestión del tiempo

### Restricción de tiempo recomendada
- **10-15 minutos** para comenzar el workshop y crear eventos
- Esto es usualmente suficiente para empezar

### Por qué limitar el tiempo
- Lo más importante es la **participación de todos** y construir momentum
- En sesiones Big Picture, más tiempo NO es mejor
- Con más tiempo, las personas pueden sentir que necesitan entrar en más detalle
- Recuerda: una sesión Big Picture es justamente eso... **¡big picture!**
- Puede ser un desafío, pero intenta guardar los detalles para después

## Mejores prácticas

### ✅ Hacer
1. **Participación activa**: Todos deben contribuir con eventos
2. **No juzgar**: No hay eventos "incorrectos" en esta fase
3. **Cantidad sobre calidad**: Genera muchos eventos, refinarás después
4. **Momentum**: Mantén la energía alta y el ritmo activo
5. **Enfoque en hechos**: Los eventos son cosas que ya sucedieron
6. **Lenguaje del negocio**: Usa términos que los expertos de dominio entiendan

### ❌ Evitar
1. **Ordenar prematuramente**: No te preocupes por el orden todavía
2. **Perfeccionar el lenguaje**: Eso viene después
3. **Eliminar duplicados**: Los consolidarás en el siguiente paso
4. **Entrar en detalles técnicos**: Mantén el foco en eventos de negocio
5. **Debates largos**: Guarda las discusiones para después

## Errores comunes

### Eventos como comandos
**Problema**: Escribir acciones en lugar de eventos
**Ejemplo**: "Create User" en lugar de "User Created"
**Solución**: Recordar que los eventos son hechos pasados

### Detalles técnicos
**Problema**: "API Call Made", "Database Updated"
**Solución**: Enfocarse en eventos de negocio que importan al dominio

### Procesos en lugar de eventos
**Problema**: "User Registration Process"
**Solución**: Identificar eventos específicos dentro del proceso

## Facilitación efectiva

### Preguntas útiles para generar eventos
- "¿Qué sucede cuando...?"
- "¿Cómo sabemos que...?"
- "¿Qué cambia cuando...?"
- "¿Qué notificaciones se envían cuando...?"
- "¿Qué se registra cuando...?"

### Mantener el momentum
- Celebrar la participación
- No corregir inmediatamente
- Hacer preguntas abiertas
- Rotar quién habla

## Validación del paso

Antes de continuar al Step 2, confirma:

**[Answer]: ¿Todos los participantes contribuyeron con al menos un evento?**

**[Answer]: ¿Tienes al menos 20-30 eventos generados?**

**[Answer]: ¿Los eventos están escritos en tiempo pasado?**

**[Answer]: ¿Los eventos representan hechos del dominio de negocio (no detalles técnicos)?**

**[Answer]: ¿Hay una buena mezcla de eventos que cubren diferentes partes del proceso?**

## Resultado esperado

Al final de este paso, deberías tener:
- Una colección de notas adhesivas naranjas con eventos de dominio (30-50+ eventos)
- Participación de todos los miembros del equipo
- Un conjunto inicial de eventos que cubren el proceso de negocio
- Momentum y energía para continuar con el siguiente paso
- Eventos escritos en tiempo pasado
- Mezcla de eventos de negocio y técnicos relevantes

## Próximo paso

Una vez que el equipo ha creado suficientes eventos, es tiempo de secuenciarlos. El Step 2 te enseñará cómo ejecutar la secuenciación de eventos y consolidar duplicados.
