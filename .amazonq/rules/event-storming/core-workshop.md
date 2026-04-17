# PRIORITY: Event Storming Workshop Workflow
# When user requests Event Storming workshop facilitation, ALWAYS follow this workflow

## Override Instructions
Always follow this workflow for Event Storming workshops. Never skip steps.

## MANDATORY: Rule Details Loading
**CRITICAL**: When performing any step, you MUST read and use relevant content from rule detail files in `.amazonq/event-storming-details/` directory. Do not summarize or paraphrase - use the complete content as written.

## MANDATORY: Custom Welcome Message
**CRITICAL**: When starting ANY Event Storming workshop request, you MUST begin with this exact message:

"¡Bienvenido al Workshop de Event Storming Guiado!

Este workflow te guiará paso a paso a través de un workshop completo de Event Storming:

📋 Preparación: Identificar participantes y objetivos
🟧 Crowdsource: Brainstorming de eventos del dominio
📊 Sequence: Ordenar eventos y flujos
👥 People: Identificar actores y roles
🔴 Hotspots: Capturar preguntas y problemas
🔗 External Systems: Mapear dependencias externas
⭐ Pivotal Events: Definir bounded contexts

Este enfoque estructurado asegura que captures todo el conocimiento del dominio
y identifiques los límites naturales de tu sistema. ¡Comencemos!"

# Event Storming Workshop - Core Workflow

## Overview
When the user requests facilitation of an Event Storming workshop, follow this structured approach through all 7 steps (Step 0-6).

## Welcome
1. **Display Custom Welcome Message**: Show the Event Storming welcome message above
2. Load all content from `introduction/what-is-event-storming.md`
3. Load all content from `introduction/benefits-and-outcomes.md`
4. Load all content from `introduction/session-types.md`
5. Present the loaded content to explain Event Storming fundamentals
6. **Ask for Confirmation and WAIT**: Ask: "¿Comprendes qué es Event Storming y estás listo para comenzar con la Preparación del Workshop?" - DO NOT PROCEED until user confirms

## Initial Setup
1. Load all steps from `steps/initial-setup.md`
2. Execute the steps loaded from `steps/initial-setup.md`
3. **Ask for Confirmation and WAIT**: Ask: "Setup completo. ¿Estás listo para comenzar con Step 0: Invitations?" - DO NOT PROCEED until user confirms

## Step 0: Invitations (Preparación)
1. Load all steps from `steps/step0-invitations.md`
2. Execute the steps loaded from `steps/step0-invitations.md`
3. **MANDATORY VALIDATION**: Review all answers for:
   - Vague participant descriptions
   - Missing functional areas
   - Unclear workshop objectives
   - Ambiguous scope definitions
4. **MANDATORY Follow-up**: Add clarification questions with [Answer]: tags if needed
5. **Ask for Confirmation and WAIT**: Ask: "Step 0 completo. ¿Estás listo para comenzar con Step 1: Crowdsource Events?" - DO NOT PROCEED until user confirms

## Step 1: Crowdsource Events
1. Load all steps from `steps/step1-crowdsource-events.md`
2. Execute the steps loaded from `steps/step1-crowdsource-events.md`
3. **MANDATORY VALIDATION**: Review all events for `event-storming/artifacts/step1.md`:
   - Events not in past tense
   - Technical implementation details instead of domain events
   - Missing key business events
4. **MANDATORY Follow-up**: Add clarification questions with [Answer]: tags if needed in `event-storming/artifacts/step1.md`
5. **Ask for Confirmation and WAIT**: Ask: "Step 1 completo. ¿Estás listo para comenzar con Step 2: Sequence Events?" - DO NOT PROCEED until user confirms

## Step 2: Sequence Events
1. Load all steps from `steps/step2-sequence-events.md`
2. Execute the steps loaded from `steps/step2-sequence-events.md`
3. **MANDATORY VALIDATION**: Review sequence for:
   - Logical gaps in event flow
   - Missing sad path scenarios
   - Unclear event ordering
   - Duplicate events not consolidated
4. **MANDATORY Follow-up**: Add clarification questions with [Answer]: tags if needed
5. **Ask for Confirmation and WAIT**: Ask: "Step 2 completo. ¿Estás listo para comenzar con Step 3: People?" - DO NOT PROCEED until user confirms

## Step 3: People (Actors/Personas)
1. Load all steps from `steps/step3-people.md`
2. Execute the steps loaded from `steps/step3-people.md`
3. **MANDATORY VALIDATION**: Review actors for:
   - Missing key personas
   - Unclear role definitions
   - Ambiguous responsibilities
   - Missing user evolution stages
4. **MANDATORY Follow-up**: Add clarification questions with [Answer]: tags if needed
5. **Ask for Confirmation and WAIT**: Ask: "Step 3 completo. ¿Estás listo para comenzar con Step 4: Hotspots?" - DO NOT PROCEED until user confirms

## Step 4: Hotspots
1. Load all steps from `steps/step4-hotspots.md`
2. Execute the steps loaded from `steps/step4-hotspots.md`
3. **MANDATORY VALIDATION**: Review hotspots for:
   - Critical questions not captured
   - Blocking issues that need immediate resolution
   - Missing areas of uncertainty
4. **MANDATORY Follow-up**: Add clarification questions with [Answer]: tags if needed
5. **Ask for Confirmation and WAIT**: Ask: "Step 4 completo. ¿Estás listo para comenzar con Step 5: External Systems?" - DO NOT PROCEED until user confirms

## Step 5: External Systems
1. Load all steps from `steps/step5-external-systems.md`
2. Execute the steps loaded from `steps/step5-external-systems.md`
3. **MANDATORY VALIDATION**: Review external systems for:
   - Missing critical dependencies
   - Unclear control/responsibility boundaries
   - Ambiguous integration points
4. **MANDATORY Follow-up**: Add clarification questions with [Answer]: tags if needed
5. **Ask for Confirmation and WAIT**: Ask: "Step 5 completo. ¿Estás listo para comenzar con Step 6: Pivotal Events?" - DO NOT PROCEED until user confirms

## Step 6: Pivotal Events (Bounded Contexts)
1. Load all steps from `steps/step6-pivotal-events.md`
2. Execute the steps loaded from `steps/step6-pivotal-events.md`
3. **MANDATORY VALIDATION**: Review pivotal events for:
   - Unclear bounded context boundaries
   - Missing pivotal event indicators
   - Ambiguous context responsibilities
4. **MANDATORY Follow-up**: Add clarification questions with [Answer]: tags if needed
5. **Ask for Confirmation and WAIT**: Ask: "Step 6 completo. Workshop finalizado. ¿Quieres revisar los artefactos generados?" - DO NOT PROCEED until user confirms

## CRITICAL: Progress Tracking

### MANDATORY RULES FOR WORKSHOP EXECUTION
1. **NEVER complete any step without updating checkboxes in event-storming-state.md**
2. **IMMEDIATELY after completing ANY step, mark that step [x]**
3. **This must happen in the SAME interaction where the work is completed**
4. **Update "Current Step" section after each step completion**
5. **NO EXCEPTIONS**: Every step completion MUST be tracked

### Progress Tracking System
- **Location**: event-storming-artifacts/event-storming-state.md
- **When to Update**: Mark steps [x] as you complete each step
- **Current Status**: Always update after any progress
- **Same Interaction**: All updates in SAME interaction as work completion

## Key Principles
- Always explain the purpose of each step before executing
- Use visual representations (ASCII diagrams) when helpful
- Validate understanding at each checkpoint
- Capture all decisions and answers with [Answer]: tags
- Time-box discussions - use hotspots for deep dives
- Focus on domain events, not technical implementation
- Ensure cross-functional participation
- **MANDATORY**: Update event-storming-state.md after each step

## Directory Structure
```
event-storming-artifacts/
├── preparation/
│   ├── participant-list.md
│   └── workshop-objectives.md
├── events/
│   ├── domain-events.md
│   └── event-sequence.md
├── actors/
│   ├── personas.md
│   └── user-evolution.md
├── issues/
│   ├── hotspots.md
│   └── decisions-pending.md
├── integrations/
│   └── external-systems.md
├── bounded-contexts/
│   ├── pivotal-events.md
│   └── context-map.md
└── event-storming-state.md
```

## File Naming Convention
- Workshop State: event-storming-artifacts/event-storming-state.md
- Participant List: event-storming-artifacts/preparation/participant-list.md
- Workshop Objectives: event-storming-artifacts/preparation/workshop-objectives.md
- Domain Events: event-storming-artifacts/events/domain-events.md
- Event Sequence: event-storming-artifacts/events/event-sequence.md
- Personas: event-storming-artifacts/actors/personas.md
- User Evolution: event-storming-artifacts/actors/user-evolution.md
- Hotspots: event-storming-artifacts/issues/hotspots.md
- Decisions Pending: event-storming-artifacts/issues/decisions-pending.md
- External Systems: event-storming-artifacts/integrations/external-systems.md
- Pivotal Events: event-storming-artifacts/bounded-contexts/pivotal-events.md
- Context Map: event-storming-artifacts/bounded-contexts/context-map.md
