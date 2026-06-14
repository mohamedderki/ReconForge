# Iteration 2 – Subdomain Discovery

## Feature Specification

In der zweiten Iteration wird die Erkennung von Subdomains implementiert.

Das System durchsucht bekannte DNS-Einträge und sammelt verfügbare Subdomains der Ziel-Domain.

## Requirements

- Erkennung von Subdomains
- Speicherung gefundener Subdomains
- Vermeidung von Duplikaten
- Ausgabe der Ergebnisse über die CLI

## Acceptance Criteria

- Gefundene Subdomains werden angezeigt.
- Doppelte Einträge werden entfernt.
- Die Suche kann für verschiedene Domains durchgeführt werden.

## Validation

Die Funktion wurde mit öffentlichen Test-Domains überprüft.

Testfälle:

- github.com
- microsoft.com
- cloudflare.com

Gefundene Subdomains wurden erfolgreich gesammelt und ausgegeben. Doppelte Einträge wurden korrekt entfernt.