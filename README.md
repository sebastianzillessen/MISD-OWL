# MISD-OWL
### Master Infrastructure Situation Display Observing Windows and Linux
----


Diese Spezifikation beschreibt das Projekt MISD OWL der Universität Stuttgart.
Im Rahmen des Studienprojekts 2012 des Instituts VISUS soll ein System zur Überwachung von vernetzten Rechnern erstellt werden.
Das entwickelte System soll in der Lage sein, verschiedene Systeme (Workstations und Cluster) mithilfe von  exibel erweiterbaren Plugins zu überwachen. Diese Überwachung soll zentral ver- waltet und gespeichert werden und anschlieÿend sowohl auf Desktops sowie auf den Powerwalls des Instituts visualisiert werden.


## Allgemeines

Das MISD OWL-System ist eine verteilte Software, die zur zentralen Überwachung von Netzwerken eingesetzt werden soll. Die zentrale Komponente hierbei ist der MISD OWL-Server, der als Sammelstelle der Netzwerkinformationen dient. Er speichert Daten, die eine Vielzahl von Workstations, mit unterschiedlichen Betriebssystemen zu ihm senden. Auÿerdem kann der Server Informationen über im Netzwerk be ndliche Cluster sammeln. Zur Visualisierung der gesammelten Daten gibt es eine Desktop-Ober äche, mit der das System auch kon guriert werden kann, und eine Powerwall-Ober äche, die eine ansprechende Visualisierung ermöglichen soll.

## Server
Der Server speichert die erhobenen Informationen und Einstellungen verschiedenster Art in einer Datenbank. Auÿerdem verwaltet und verteilt er Plugins, die zur Datenbescha ung auf unterschiedlichen Systemen benötigt werden. Zur Netz- werkkommunikation mit den beteiligten Geräten stellt der Server Schnittstellen zur Verfügung. Diese sollen jeweils als WCF Web Services realisiert werden.

## Client
Als Clients werden diejenigen Netzwerkrechner bezeichnet, welche die Zustandsdaten der zu überwachender Rechner aus der Datenbank des Servers visualisieren. Die Visualisierung  ndet entweder auf einem Desktops oder auf einer Powerwall statt. Detaileinstellungen des Systems können nur auf einem Desktops vorgenommen werden. Zur Nutzung werden die Windows Anmeldedaten benutzt.


## Workstation
Eine Workstation ist ein zu überwachender Rechner im Netzwerk, auf dem der MISD OWL-Dienst läuft. Der Dienst auf einer Workstation soll beim Start des Systems automatisch gestartet werden. Beim Start wird der Stand der Plugins mit dem Server abgeglichen, sodass die Workstation immer auf dem aktuellen Stand sind. Außerdem werden beim Start die Aktualisierungsintervalle aller Kenngrößen erneuert. Die Workstation sendet dann in regelmäÿigen Abständen die aktuellen Werte der Kenngröÿen an den Server. So soll eine durchgängige und aussagekräftige Überwachung der Workstations ermöglicht werden.

## Cluster
Neben Workstations können auch sich in Cluster befindlichen Rechner von MISD OWL über- wacht werden. Die Daten über die sich im Cluster befindlichen zu überwachender Rechner werden direkt vom Server ermittelt. Sämtliche zu überwachende Rechner, die sich in Clustern befinden, werden als eine Organisationseinheit aufgefasst und sind somit optische einfach dem entsprechenden Cluster zuordenbar.
