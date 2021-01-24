# FlowerPot

Versionsunterschiede:

Full 	- Extended Tracking mit Animationen
Limited - Limited Tracking mit Animationen
Minimal - Limited Tracking ohne Animationen und in-screen Watering Reminder


Projekt lässt sich in Unity importieren.
Optionen befinden sich im ARCamera-Skript Controller. Der Code ist stellenweise kommentiert.
Die Vuforia Markergenerierung geschieht im Bereich AR/Vuforia.
Nachdem Marker über den angepassten TrackableEventHandler (VuforiaTargetEvents.cs) erkannt wurden, läuft die weitere Interface-Logik über das Icon-Skript.

Die Fotos der Pflanzen werden unter dem Application persistent Data Path (/android/data/com.vuforia.engine.VuforiaEngine/files) gespeichert.
Zum Speichern werden die Pfade der Bilder zusammen mit ihrem Namen in eine JSON geschrieben.
Beim Laden werden je Pflanze ein neues DataSet angelegt und die Infos zur der Pflanze geladen.
Das ist ineffizient. DataSets und Dictonaries müssen noch serialisiert werden.

Bekannte Fehler:

- Bei manchen Smartphones wird die Taschenlampe nicht erkannt
- Bei weniger als sechs gespeicherten Pflanzen stimmen die Bilder im Journal nicht
- Löschen der ersten Planze einer Seite löscht mehrere Pflanzen