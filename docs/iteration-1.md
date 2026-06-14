# Iteration 1 – Domain Scan

## Feature Specification

Das erste Feature von ReconForge ist die Analyse einer Ziel-Domain.

Der Benutzer gibt einen Domainnamen ein. Das System überprüft die Eingabe, führt eine DNS-Abfrage durch und sammelt grundlegende Informationen über die Domain.

## Requirements

- Eingabe einer Domain über die CLI
- Validierung der Domain
- Durchführung einer DNS-Abfrage
- Anzeige der Ergebnisse
- Speicherung der Ergebnisse

## Acceptance Criteria

- Eine gültige Domain wird akzeptiert.
- DNS-Daten werden erfolgreich ermittelt.
- Die Ergebnisse werden in der Konsole angezeigt.
- Fehlerhafte Eingaben werden erkannt.

## Validation

Die Funktion wurde mit verschiedenen Domains getestet.

Testfälle:

- google.com
- github.com
- bht-berlin.de

Die DNS-Abfrage lieferte für alle gültigen Domains Ergebnisse. Ungültige Eingaben wurden korrekt erkannt und abgewiesen.