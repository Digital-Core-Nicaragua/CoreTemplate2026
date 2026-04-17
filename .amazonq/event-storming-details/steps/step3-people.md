# Step 3: People - Identificación de personas/actores

## Objetivo del paso

Después de una secuencia inicial de eventos, es tiempo de comenzar a agregar personas. La idea es pensar quién es el objetivo de un evento dado.

## Terminología

Puedes ver "people" (personas) llamadas de diferentes formas en diferentes recursos de EventStorming:
- **Actors** (Actores)
- **Agents** (Agentes)
- **Personas**
- **Users** (Usuarios)

La idea es la misma: identificar quién está involucrado en cada evento.

## Representación visual

Las personas/actores se representan con **notas adhesivas amarillas** (yellow sticky notes).

```
┌─────────────────┐
│  Potential User │  ← Nota amarilla
└─────────────────┘
        ↓
┌─────────────────┐
│  Name Entered   │  ← Nota naranja (evento)
└─────────────────┘
```

## Preguntas clave para identificar personas

### ¿Quién es el objetivo del evento?
- Puede o no ser la misma persona que crea el evento
- Piensa en quién **sirve** el evento

### Analogía de película
Usa una película como analogía:
- En cualquier escena, hay un personaje central que es la estrella
- A veces hay múltiples personajes centrales en una escena particular
- ¿Quién es el "protagonista" de este evento?

## Cuándo agregar actores

**Regla importante**: Agrega actores cuando **aparecen o cambian**, no en cada evento.

## Ejemplo: Proceso de registro de usuario

### Evolución del usuario a través del proceso

```
Inicio del proceso:
┌─────────────────┐
│ Potential User  │  ← No sabemos nada excepto username/email
└─────────────────┘
        ↓
┌─────────────────┐
│ Name entered    │
└─────────────────┘
        ↓
┌─────────────────┐
│ Email verified  │
└─────────────────┘

Después de información básica:
┌─────────────────┐
│   Known User    │  ← Ahora tenemos información verificada
└─────────────────┘
        ↓
┌─────────────────┐
│ Credit card     │
│ entered         │
└─────────────────┘

Final del proceso:
┌─────────────────┐
│   Full User     │  ← Usuario completamente registrado
└─────────────────┘
        ↓
┌─────────────────┐
│ Account         │
│ activated       │
└─────────────────┘
```

## Importancia de la especificidad

### Agregar adjetivos a las personas
No te quedes solo con `user` o `customer`. Pregunta: **¿Qué tipo de usuario?**

**Ejemplo de evolución**:
1. **Potential User** (Usuario potencial)
   - No sabemos nada excepto username y email
   - No han hecho ningún compromiso de registrarse
   - No han proporcionado información completa

2. **Known User** (Usuario conocido)
   - Han verificado su email
   - Han proporcionado información básica
   - Aún no han completado el registro

3. **Full User** (Usuario completo)
   - Han completado todo el proceso de registro
   - Tienen acceso completo al servicio
   - Cuenta totalmente activada

## Perspectiva técnica (para equipos de desarrollo)

### Mapeo a entidades de datos
Esta evolución de personas puede mapear directamente a diferentes entidades en tu base de datos:

- **Potential User** → Tabla `prospects` o `leads`
- **Known User** → Tabla `users` con `status = 'pending'`
- **Full User** → Tabla `users` con `status = 'active'`

### Permisos y autorización
Diferentes tipos de usuarios pueden tener diferentes permisos:
- **Potential User**: Solo puede ver página de registro
- **Known User**: Puede acceder a perfil básico
- **Full User**: Acceso completo a todas las funcionalidades

## Identificación de handoffs

### ¿Qué es un handoff?
Un handoff ocurre cuando la responsabilidad o el control pasa de una persona/sistema a otro.

### Ejemplo: Proceso de soporte al cliente

```
┌─────────────────┐
│    Customer     │
└─────────────────┘
        ↓
┌─────────────────┐
│ Support ticket  │
│ created         │
└─────────────────┘
        ↓
┌─────────────────┐
│ Support Agent   │  ← Handoff: de Customer a Support Agent
└─────────────────┘
        ↓
┌─────────────────┐
│ Ticket assigned │
└─────────────────┘
        ↓
┌─────────────────┐
│ Technical       │  ← Handoff: de Support Agent a Technical Expert
│ Expert          │
└─────────────────┘
        ↓
┌─────────────────┐
│ Issue resolved  │
└─────────────────┘
```

### Importancia de los handoffs
- Identifican puntos de fricción potencial
- Muestran donde puede perderse información
- Revelan oportunidades de automatización
- Ayudan a definir bounded contexts

## Roles vs Personas específicas

### Usar roles, no nombres específicos
- ✅ `Customer Support Agent`
- ✅ `Product Manager`
- ✅ `System Administrator`
- ❌ `John from Marketing`
- ❌ `Sarah the Developer`

### Diferentes niveles de especificidad

#### Nivel genérico
- `User`
- `Customer`
- `Employee`

#### Nivel específico (recomendado)
- `Premium Customer`
- `Trial User`
- `System Administrator`
- `Customer Support Agent`
- `Product Owner`

## Múltiples actores en un evento

### Cuándo un evento afecta a múltiples personas
Algunos eventos pueden involucrar múltiples actores:

```
┌─────────────────┐  ┌─────────────────┐
│    Customer     │  │ Support Agent   │
└─────────────────┘  └─────────────────┘
        ↓                    ↓
        └────────────────────┘
                 ↓
┌─────────────────────────────────────┐
│ Support conversation started        │
└─────────────────────────────────────┘
```

## Proceso de identificación

### 1. Revisar cada evento
- Ir evento por evento en la secuencia
- Preguntar: "¿Quién está involucrado aquí?"
- No todos los eventos necesitan un actor visible

### 2. Identificar cambios de actor
- Marcar donde aparece un nuevo tipo de persona
- Identificar handoffs entre personas
- Documentar evolución de roles

### 3. Validar con el grupo
- Confirmar que los roles son correctos
- Asegurar que todos entienden las diferencias
- Acordar nombres/términos para cada tipo de actor

## Mejores prácticas

### ✅ Hacer
1. **Ser específico**: Usar adjetivos descriptivos para tipos de usuarios
2. **Mostrar evolución**: Cómo cambian las personas a través del proceso
3. **Identificar handoffs**: Puntos donde cambia la responsabilidad
4. **Usar roles**: No nombres específicos de personas
5. **Validar comprensión**: Asegurar que todos entienden cada rol

### ❌ Evitar
1. **Roles demasiado genéricos**: "Usuario" sin más especificidad
2. **Nombres específicos**: Usar nombres de personas reales
3. **Ignorar handoffs**: No documentar cambios de responsabilidad
4. **Asumir roles**: Validar con expertos de dominio

## Facilitación efectiva

### Preguntas útiles
- "¿Quién es el protagonista de este evento?"
- "¿Qué tipo de usuario es en este punto?"
- "¿Cuándo cambia el tipo de usuario?"
- "¿Quién tiene la responsabilidad aquí?"
- "¿Hay múltiples personas involucradas?"

### Técnicas de facilitación
- Usar colores diferentes para diferentes tipos de actores
- Dibujar líneas para mostrar handoffs
- Agrupar eventos por actor cuando sea útil
- Celebrar cuando se identifiquen handoffs importantes

## Validación del paso

Antes de continuar al Step 4, confirma:

**[Answer]: ¿Se identificaron los actores principales en el proceso?**

**[Answer]: ¿Los actores tienen nombres específicos y descriptivos (no genéricos)?**

**[Answer]: ¿Se documentó la evolución de usuarios a través del proceso?**

**[Answer]: ¿Se identificaron los handoffs entre diferentes actores?**

**[Answer]: ¿Todos los participantes entienden los diferentes roles?**

**[Answer]: ¿Se usaron roles en lugar de nombres específicos de personas?**

## Resultado esperado

Al final de este paso, deberías tener:
- Actores claramente identificados con sticky notes amarillos
- Roles específicos y descriptivos (no genéricos)
- Evolución de usuarios documentada a través del proceso
- Handoffs entre actores identificados y marcados
- Comprensión compartida de quién hace qué en cada punto
- Base para identificar bounded contexts en pasos posteriores

## Próximo paso

Una vez que los actores están identificados y los handoffs son claros, es tiempo de capturar preguntas y problemas en Step 4: Hotspots.
