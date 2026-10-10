## Ziel
Dieses Unity-Projekt lädt automatisch alle LehrerInnen der HTL Salzburg, filtert jene nach einem bestimmten Raum (z. B. `C205`) und zeigt folgende Informationen an:

- Name  
- Raumnummer  
- Sprechstunde  

Die Daten stammen direkt von der offiziellen Website:  
`https://www.htl-salzburg.ac.at/lehrerinnen.html`

---

## Erklärung der Coroutine `IEnumerator LoadTeachers()`

Die Methode `LoadTeachers()` ist eine **Coroutine**.  
Coroutinen in Unity sind spezielle Methoden, die **nicht sofort vollständig ausgeführt werden**, sondern **Schritt für Schritt**, während Unity zwischen den Schritten weiterläuft.  
Das erlaubt, **auf Webanfragen zu warten**, ohne das Spiel oder die Benutzeroberfläche zu blockieren.

---

## Ablauf der Coroutine

### 1. Aufbau der URLs

```csharp
string baseUrl = "https://www.htl-salzburg.ac.at";
string listUrl = baseUrl + "/lehrerinnen.html";
```

Hier wird die Basis-URL der Website definiert und mit dem Pfad zur LehrerInnen-Liste kombiniert.  
Das ergibt die vollständige Adresse der Seite, die alle LehrerInnen enthält.

---

### 2. Laden der LehrerInnen-Liste

```csharp
UnityWebRequest req = UnityWebRequest.Get(listUrl);
yield return req.SendWebRequest();
```

**Funktionsweise:**

- `UnityWebRequest.Get(listUrl)` erstellt eine HTTP-GET-Anfrage an die Website.  
- `req.SendWebRequest()` startet die Anfrage.  
- Das Schlüsselwort `yield return` sorgt dafür, dass Unity **wartet**, bis die Anfrage abgeschlossen ist, bevor der nächste Code ausgeführt wird.  
  Währenddessen bleibt das Programm reaktionsfähig.

---

### 3. Fehlerbehandlung

```csharp
if (req.result != UnityWebRequest.Result.Success)
{
    outputText.text = "Fehler beim Laden der Lehrerliste!";
    yield break;
}
```

Wenn die Anfrage fehlschlägt (z. B. keine Internetverbindung oder Serverfehler), wird die Coroutine mit `yield break` beendet.  
Das verhindert, dass der restliche Code ausgeführt wird.

---

### 4. HTML-Code speichern

```csharp
string html = req.downloadHandler.text;
```

Nach erfolgreichem Laden wird der gesamte HTML-Quelltext der Seite in einer Zeichenkette gespeichert.  
Dieser Text enthält alle LehrerInnen-Einträge, die später mit Regex durchsucht werden.

---

### 5. LehrerInnen-Einträge finden

```csharp
Regex teacherRegex = new Regex(
    "<a[^>]*href=\"(?<link>/lehrerinnen-details/[^\"]+)\"[^>]*>\\s*<span[^>]*>(?<name>[^<]+)</span>",
    RegexOptions.IgnoreCase
);
var matches = teacherRegex.Matches(html);
```

Hier wird eine **Reguläre Ausdrucks-Suche (Regex)** verwendet, um alle LehrerInnen-Links und Namen aus dem HTML zu extrahieren.

**Ablauf:**
- `teacherRegex.Matches(html)` durchsucht den gesamten HTML-Text.  
- Jeder Treffer enthält zwei Gruppen:
  - `link`: den Pfad zur Detailseite des Lehrers  
  - `name`: den sichtbaren Namen des Lehrers  

---

### 6. Schleife über alle LehrerInnen

```csharp
foreach (Match m in matches)
```

Die Schleife geht jeden gefundenen LehrerInnen-Eintrag durch und verarbeitet ihn einzeln.

---

### 7. Raum aus dem Link extrahieren

```csharp
Match roomMatch = Regex.Match(
    link,
    "-(?<letter>[a-h])-(?<number>[0-9]+)\\.html",
    RegexOptions.IgnoreCase
);
```

Diese Regex sucht im Dateinamen nach dem Raum.  
Beispiel: `schweiberer-franz-prof-dipl-ing-c-205.html`

**Erklärung:**
- `-(?<letter>[a-h])` → Buchstabe A–H (Gebäudebereich)  
- `-(?<number>[0-9]+)` → Raumnummer  
- `\\.html` → Dateiendung  

Ergebnis: `C205`

---

### 8. Filterung nach Zielraum

```csharp
if (room != targetRoom)
    continue;
```

Wenn der Raum nicht dem gesuchten Raum entspricht, wird der aktuelle Lehrer übersprungen.

---

### 9. Detailseite laden

```csharp
UnityWebRequest detailReq = UnityWebRequest.Get(baseUrl + link);
yield return detailReq.SendWebRequest();
```

Hier wird die individuelle Detailseite des Lehrers geladen.  
Das `yield return` sorgt wieder dafür, dass Unity wartet, bis die Seite vollständig geladen ist.

---

### 10. Sprechstunde aus Detailseite extrahieren

```csharp
Match sprechstundeMatch = Regex.Match(
    teacherHtml,
    "Sprechstunde:</div>\\s*<div class=\"value\">\\s*<span class=\"text\">(?<sprechstunde>[^<]+)</span>",
    RegexOptions.IgnoreCase
);
```

Diese Regex sucht gezielt nach dem HTML-Block, der die Sprechstunde enthält.

**Ablauf:**
1. Findet den Text `Sprechstunde:`  
2. Springt zum nächsten `<div class="value">`  
3. Liest den Inhalt des `<span class="text">…</span>`  
4. Speichert ihn in der Gruppe `sprechstunde`

Beispielausgabe: `Freitag 09:35 - 10:35 Uhr`

---

### 11. Ausgabe zusammenbauen

```csharp
string sprechstunde = sprechstundeMatch.Success
    ? sprechstundeMatch.Groups["sprechstunde"].Value.Trim()
    : "Keine Sprechstunde";

result += $"{name}\n";
result += $"Raum: {room}\n";
result += $"Sprechstunde: {sprechstunde}\n";
result += $"-------------------------\n";
```

Wenn die Regex erfolgreich war, wird die Sprechstunde übernommen.  
Ansonsten wird „Keine Sprechstunde“ angezeigt.  
Alle Informationen werden in der Variable `result` gesammelt.

---

### 12. Ergebnis anzeigen

```csharp
outputText.text = result;
```

Am Ende wird der gesamte Text im TextMeshPro-Objekt angezeigt.  
Damit erscheinen alle LehrerInnen des gewünschten Raums mit ihren Sprechstunden auf dem Bildschirm.

---

## Zusammenfassung der Coroutine-Logik

1. Lade die LehrerInnen-Liste von der Website.  
2. Extrahiere alle LehrerInnen-Einträge mit Regex.  
3. Lies den Raum aus dem Dateinamen.  
4. Filtere nach dem gewünschten Raum.  
5. Lade die Detailseite jedes passenden Lehrers.  
6. Extrahiere die Sprechstunde mit Regex.  
7. Zeige Name, Raum und Sprechstunde im Textfeld an.  

Die Coroutine ermöglicht, dass Unity während der Webanfragen weiterläuft, ohne zu blockieren.  
Dadurch bleibt die Anwendung flüssig, auch wenn mehrere Seiten geladen werden.
```
