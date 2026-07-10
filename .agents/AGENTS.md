# Workspace-Specific Customization Rules

These rules apply universally to all tasks and agents working inside the FTD TechZone project.

## Documenting Changes

You MUST update the comprehensive project documentation file `PROJECT_COMPLETE_DOCUMENTATION.md` at the root of the project whenever you:
- Modify the database schema, seeds, or identity configurations.
- Create, rename, or delete API controller endpoints in `FTD.Api`.
- Modify business service interfaces in `FTD.Application/Interfaces`.
- Change payment/checkout flows or shipping calculations.
- Introduce new config keys in `appsettings.json`.

Ensure that the documentation accurately reflects the current state of the codebase.
