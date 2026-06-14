# Iteration 3 – Port Scan

## Feature Specification

Die dritte Iteration erweitert ReconForge um einen einfachen Port-Scanner.

Das System untersucht die häufigsten TCP-Ports eines Hosts und identifiziert offene Netzwerkdienste.

## Requirements

- Durchführung eines TCP-Port-Scans
- Erkennung offener Ports
- Ausgabe der Ergebnisse
- Fehlerbehandlung bei Timeouts

## Acceptance Criteria

- Offene Ports werden erkannt.
- Geschlossene Ports verursachen keine Fehler.
- Der Scan liefert reproduzierbare Ergebnisse.

## Validation

Der Scanner wurde gegen lokale und öffentliche Testsysteme ausgeführt.

Testfälle:

- localhost
- scanme.nmap.org

Die offenen Ports wurden erfolgreich erkannt. Timeouts und Verbindungsfehler wurden korrekt behandelt und protokolliert.