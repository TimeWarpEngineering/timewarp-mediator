# Kanban Board

This Kanban board manages and tracks epics and tasks for the project using a simple folder structure.
Each item is represented by a Markdown file, and the status of the item is indicated by the folder it is in.

## Folders

1. **Backlog**: Contains tasks that are not yet ready to be worked on. These tasks have a temporary backlog scoped unique identifier.
   a. **Scratch** - contains epics or tasks that are works in progress or ideas.  They can be stored in folders under users names if needed.
2. **ToDo**: Contains tasks that are ready to be worked on. When a task from the Backlog becomes ready, it is assigned a unique identifier and moved to this folder.
3. **InProgress**: Contains tasks that are currently being worked on.
4. **Done**: Contains tasks that have been completed.

## File Naming Convention

### Tasks
- For tasks in the Backlog folder, use a short description with a 'B' prefix followed by a three-digit identifier,
such as `B001_Research-Authentication-Methods.md` or `B002_Design-Game-Rules.md`.
- When a task becomes "Ready," assign it a unique identifier (without the 'B' prefix) and move it to the ToDo folder, e.g.,
`001_Implement-User-Registration.md` or `002_Create-Game-Logic.md`.
- <3 digit Id>_<Short-Description-Separated-By-Hyphens>
