# UML-Diagramme – Dokumentation

## Überblick

Im Rahmen des Projekts **ReconForge** wurden mehrere UML-Diagramme erstellt, um die Anforderungen, Struktur, Abläufe und Architektur des Systems zu modellieren und zu dokumentieren.

Die Diagramme wurden schrittweise entwickelt und kontinuierlich verfeinert, um eine konsistente Sicht auf das Gesamtsystem zu gewährleisten.

---

# 1. Use Case Diagram

Das Use Case Diagram beschreibt die funktionalen Anforderungen aus Sicht des Benutzers (Security Researcher).

Folgende Hauptfunktionen wurden modelliert:

* Perform Domain Scan
* Discover Subdomains
* Resolve Domain IP Address
* Perform Port Scan
* Export Scan Results
* View Scan History
* Manage Scan Configuration

Zusätzlich wurden Include-Beziehungen verwendet, um wiederverwendbare Teilfunktionen wie die Domain-Validierung oder das Laden von Konfigurationen darzustellen.

Ziel des Diagramms war die Darstellung aller Anwendungsfälle und deren Beziehungen zum Benutzer.

---

# 2. Class Diagram

Das Klassendiagramm beschreibt die statische Struktur des Systems.

Es wurden die wichtigsten fachlichen Klassen modelliert:

* Domain
* Subdomain
* IpAddress
* Port
* DnsRecord
* ScanTask
* ScanResult
* ScanConfiguration

Darüber hinaus wurden Beziehungen, Kardinalitäten und Datentypen definiert.

Beispiele:

* Eine Domain besitzt mehrere Subdomains.
* Eine Domain kann mehrere IP-Adressen besitzen.
* Eine IP-Adresse kann mehrere Ports besitzen.
* Eine ScanTask erzeugt genau ein ScanResult.

Aufgrund der Größe des Projekts wurde im Bericht ein Auszug der wichtigsten Klassen dargestellt.

---

# 3. Activity Diagram

Das Aktivitätsdiagramm beschreibt den Ablauf der wichtigsten Systemfunktionen.

Es wurden drei Hauptabläufe modelliert:

## Scan starten

* Domain eingeben
* Domain validieren
* Konfiguration laden
* Subdomains ermitteln
* IP-Adressen auflösen
* Port-Scan durchführen
* DNS-Einträge sammeln
* Scan-Ergebnis erzeugen
* Ergebnisse anzeigen
* Ergebnisse exportieren

## Konfiguration bearbeiten

* Konfiguration laden
* Konfiguration anpassen
* Konfiguration speichern

## Scan-Verlauf anzeigen

* Scan-Historie laden
* Ergebnisse anzeigen

Das Diagramm zeigt die logische Reihenfolge der Aktivitäten sowie Entscheidungen innerhalb des Systems.

---

# 4. Sequence Diagram

Das Sequenzdiagramm beschreibt die zeitliche Interaktion zwischen den Komponenten des Systems.

Beteiligte Objekte:

* Security Researcher
* CLI
* DomainValidator
* DomainScanner
* SubdomainScanner
* IpResolver
* PortScanner
* DnsCollector
* ScanResult
* ResultExporter

Der Ablauf zeigt die Kommunikation während eines vollständigen Reconnaissance-Scans vom Start bis zum Export der Ergebnisse.

---

# 5. Component Diagram

Das Komponentendiagramm beschreibt die Architektur des Systems auf hoher Ebene.

Folgende Komponenten wurden modelliert:

* CLI Component
* Scanning Component
* DNS Component
* IP Component
* Port Component
* Result Component
* Export Component
* Config Component

Die Komponenten kommunizieren über definierte Abhängigkeiten und ermöglichen eine modulare Erweiterung des Systems.

Das Diagramm dient der Darstellung der Systemarchitektur und der Trennung von Verantwortlichkeiten.

---

# Fazit

Die UML-Diagramme wurden verwendet, um unterschiedliche Sichten auf das System darzustellen:

* Use Case Diagram → Anforderungen
* Class Diagram → Struktur
* Activity Diagram → Abläufe
* Sequence Diagram → Interaktionen
* Component Diagram → Architektur

Durch die Kombination dieser Diagramme konnte das System sowohl aus funktionaler als auch aus technischer Sicht vollständig dokumentiert werden.
