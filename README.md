#  Lehrertürschilder  

## Ziel des Projekts

Dieses Projekt erstellt in Unity ein **digitales Türschild**, das automatisch die Lehrer anzeigt, die in einem bestimmten Raum der HTBLuVA Salzburg unterrichten.

Das Türschild zeigt:

```
C205

Mag. Müller
Freitag 10:30 – 11:30
mueller@htl-salzburg.ac.at
0664 1234567
```

Alle Daten werden **automatisch von der HTBLuVA‑Lehrerliste‑Webseite geladen**.

---

# Grundlagen

## HttpClient

`HttpClient` ist eine C#‑Klasse, mit der man Webseiten herunterladen kann.

- Baut eine Verbindung zur Webseite auf  
- Lädt den HTML‑Text herunter  
- Gibt ihn als `string` zurück  

Beispiel:

```csharp
HttpClient client = new HttpClient();
string html = await client.GetStringAsync(url);
```

---

## async / await

Unity darf nicht einfrieren, während wir Webseiten laden.  
Darum verwenden wir **asynchronen Code**.

### `async`
Markiert eine Funktion als „asynchron“.

### `await`
Wartet auf eine Aufgabe, **ohne Unity zu blockieren**.

Beispiel:

```csharp
await LoadTeachers();
```

Unity läuft weiter → das Programm bleibt flüssig.

---

## Regex (Reguläre Ausdrücke)

Regex ist ein Werkzeug, um Text zu durchsuchen.

Beispiel:

```regex
<h1>(?<name>[^<]+)</h1>
```

Das bedeutet:

- Suche `<h1>`
- Nimm alles bis zum nächsten `<`
- Speichere es als `name`

Wir verwenden Regex, um HTML‑Daten aus der HTL‑Webseite zu extrahieren.

---

# Aufbau der HTBLuVA‑Lehrerliste

Die Lehrerliste enthält Links wie:

```
/lehrer/123?room=C205
```

Darin steckt:

- Lehrer‑ID  
- Raum  
- Link zur Detailseite  

Auf der Detailseite stehen:

- Name  
- E‑Mail  
- Sprechstunde  
- Telefon  

Diese Informationen extrahieren wir mit Regex.

---

# Finaler Unity‑C#‑Code 

```csharp
using System;
using System.Net.Http;               // Ermöglicht das Laden von Webseiten
using System.Text.RegularExpressions; // Ermöglicht das Durchsuchen von Text mit Regex
using System.Threading.Tasks;         // Für async/await
using TMPro;                          
using UnityEngine;                    

public class TuerSchildController : MonoBehaviour
{
    public string targetRoom = "C205"; 

    public TextMeshProUGUI outputText;

    private async void Start()
    {
        await LoadTeachers();
    }

    // Lädt Lehrer aus dem Internet
    private async Task LoadTeachers()
    {
        string baseUrl = "https://www.htl-salzburg.ac.at";
        string listUrl = baseUrl + "/lehrerliste";

        HttpClient client = new HttpClient();

        // HTML der Lehrerliste laden
        string html = await client.GetStringAsync(listUrl);

        // Regex zum Finden der Lehrer und ihrer Räume
        Regex teacherRegex = new Regex(
            "<a href=\"(?<link>/lehrer/[^\"]*?room=(?<room>[A-Z0-9]+))\">(?<name>[^<]+)</a>"
        );

        var matches = teacherRegex.Matches(html);

        // Türschild beginnt mit dem Raum
        string result = $"{targetRoom}\n\n";

        foreach (Match m in matches)
        {
            string room = m.Groups["room"].Value;

            // Nur Lehrer im gewünschten Raum anzeigen
            if (room != targetRoom)
                continue;

            string link = m.Groups["link"].Value;

            // Detailseite des Lehrers laden
            string teacherHtml = await client.GetStringAsync(baseUrl + link);

            // Name extrahieren
            string fullName = Regex.Match(teacherHtml, "<h1>(?<name>[^<]+)</h1>").Groups["name"].Value;

            // E-Mail extrahieren
            string mail = Regex.Match(teacherHtml, "mailto:(?<mail>[^\"]+)").Groups["mail"].Value;

            // Sprechstunde extrahieren
            Match sprechstundeMatch = Regex.Match(teacherHtml, "Sprechstunde:?\\s*([^<]+)");
            string sprechstunde = sprechstundeMatch.Success ? sprechstundeMatch.Groups[1].Value.Trim() : "Keine Sprechstunde";

            // Telefon extrahieren
            Match telMatch = Regex.Match(teacherHtml, "Tel\\.?[: ]+([0-9 +/]+)");
            string tel = telMatch.Success ? telMatch.Groups[1].Value.Trim() : "Keine Telefonnummer";

            // Format wie ein echtes Türschild
            result += $"{fullName}\n";
            result += $"{sprechstunde}\n";
            result += $"{mail}\n";
            result += $"{tel}\n\n";
        }

        // Ausgabe in Unity
        outputText.text = result;
    }
}
```

---

# Beispielausgabe

```
C205

Mag. Müller
Freitag 10:30 – 11:30
mueller@htl-salzburg.ac.at
0664 1234567

Ing. Steiner
Montag 08:00 – 09:00
steiner@htl-salzburg.ac.at
0676 9876543
```

---
