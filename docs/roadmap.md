# App Toolkit Roadmap

This roadmap tracks major milestones for evolving App Toolkit while keeping it practical for small-to-medium application work.

## Guiding Intent
- Preserve App Toolkit's opinionated, batteries-included developer experience.
- Improve dependency boundaries so consumers can avoid unrelated dependencies.
- Split components only where dependency boundaries are justified.

## Milestone: AppToolkit.Core
Goal: establish a presentation-neutral and broadly reusable foundation package.

### Checklist
- [ ] Define target API surface for Core (results, validation, core extensions, neutral primitives).
- [ ] Confirm Core has no UI-framework-specific dependencies.
- [ ] Audit candidate types for Core placement versus specialized packages.
- [ ] Ensure logging usage remains standardized on Microsoft.Extensions.Logging abstractions where used.
- [ ] Add/update package metadata and release notes for Core split.
- [ ] Document migration guidance for existing consumers.

## Milestone: AppToolkit.Mvvm
Goal: isolate MVVM-specific behaviors and dependencies from Core.

### Checklist
- [ ] Move `BaseViewModel` and related MVVM helpers into MVVM package boundaries.
- [ ] Evaluate `ObservableCollectionWithSelection` and `FullyObservableCollection` for MVVM ownership.
- [ ] Confirm CommunityToolkit.Mvvm dependency is scoped to this package.
- [ ] Add minimal usage documentation and registration guidance.
- [ ] Add/update package metadata and release notes.

## Milestone: AppToolkit.Hosting (candidate)
Goal: provide host/DI-centric composition patterns only if recurring usage justifies it.

### Checklist
- [ ] Identify repeated host builder/service registration patterns across real apps.
- [ ] Define minimum valuable hosting abstractions (avoid speculative features).
- [ ] Keep hosting package optional and independent from UI-specific packages.
- [ ] Add/update package metadata and release notes if created.

## Milestone: AppToolkit.Data.Sqlite
Goal: isolate SQLite/Dapper persistence concerns behind explicit package boundaries.

### Checklist
- [ ] Move SQLite access interfaces/implementations into a dedicated data package.
- [ ] Keep migration primitives and initializer logic with SQLite data implementation.
- [ ] Review constructor/API contracts for clean dependency injection boundaries.
- [ ] Ensure package clearly advertises Dapper + Microsoft.Data.Sqlite dependency.
- [ ] Add migration notes for consumers currently referencing consolidated package.

## Milestone: AppToolkit.Http
Goal: isolate HTTP integration helpers and prepare optional resilience policies when justified.

### Checklist
- [ ] Move `IHttpRequester`, `HttpRequester`, and related request models into HTTP package boundaries.
- [ ] Keep dependency on Microsoft.Extensions.Http scoped to this package.
- [ ] Define when Microsoft.Extensions.Http.Resilience should be introduced (triggered by multiple real integrations).
- [ ] Add usage documentation for named clients and compression client behavior.
- [ ] Add/update package metadata and release notes.

## Milestone: Messaging Namespace/Package Clarification
Goal: clarify whether messaging stays in consolidated package or becomes its own boundary.

### Checklist
- [ ] Audit real usage of `IEventSystem` / `EventSystem` across consuming apps.
- [ ] Decide: keep in base package vs split to `AppToolkit.Messaging`.
- [ ] If split, ensure no unnecessary transitive dependencies.
- [ ] Document recommended scenarios and non-goals (in-process only, not distributed bus).

## Milestone: File System Namespace/Package Clarification
Goal: remove namespace ambiguity by relocating file system helpers out of general data-access naming.

### Checklist
- [ ] Decide final placement (`Core.IO`, `Storage`, or dedicated package based on dependency boundaries).
- [ ] Rename namespaces to clearly communicate file-system concern.
- [ ] Preserve compatibility strategy (type forwards, deprecations, or migration notes) as needed.
- [ ] Update docs and examples to use final namespace/package.

## Milestone: Consolidated Package Transition Plan
Goal: allow current consumers to adopt split packages with minimal disruption.

### Checklist
- [ ] Decide versioning strategy for split rollout (SemVer plan).
- [ ] Define deprecation timeline for legacy consolidated surface area.
- [ ] Provide package mapping table (old namespace/type -> new package/namespace).
- [ ] Publish upgrade guidance and examples.
- [ ] Validate CI/CD packaging and publish workflow for multi-package repository.
