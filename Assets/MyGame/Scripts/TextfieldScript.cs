using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Tuerschild
{
    public class Textfield : MonoBehaviour
    {
        public string teacherlinks;
        Regex teacherLinkRegex = new Regex
        Regex teacherRegex = new Regex(
        "<a href=\"(?<link>/lehrer/[^\"]*?room=(?<room>[A-Z0-9]+))\">(?<name>[^<]+)</a>");
        private async void Start()
    {
        await LoadTeachers();
    }

        void Update()
        {

        }

        private async Task LoadTeachers()
        {
            string baseUrl = "https://www.htl-salzburg.ac.at";
            string listUrl = baseUrl + "/lehrerliste";

            HttpClient client = new HttpClient();
            string html = await client.GetStringAsync(listUrl);

            // Regex für Lehrer + Raum
            Regex teacherRegex = new Regex(
                "<a href=\"(?<link>/lehrer/[^\"]*?room=(?<room>[A-Z0-9]+))\">(?<name>[^<]+)</a>"
            );

            var matches = teacherRegex.Matches(html);

            // Türschild beginnt mit dem Raum
            string result = $"{targetRoom}\n\n";

            foreach (Match m in matches)
            {
                string room = m.Groups["room"].Value;

                if (room != targetRoom)
                    continue;

                string link = m.Groups["link"].Value;

                // Detailseite laden
                string teacherHtml = await client.GetStringAsync(baseUrl + link);

                // Name
                string fullName = Regex.Match(teacherHtml, "<h1>(?<name>[^<]+)</h1>").Groups["name"].Value;

                // E-Mail
                string mail = Regex.Match(teacherHtml, "mailto:(?<mail>[^\"]+)").Groups["mail"].Value;

                // Sprechstunde
                Match sprechstundeMatch = Regex.Match(teacherHtml, "Sprechstunde:?\\s*([^<]+)");
                string sprechstunde = sprechstundeMatch.Success ? sprechstundeMatch.Groups[1].Value.Trim() : "Keine Sprechstunde";

                // Telefon (HTL-Seite hat oft: Tel.: 0664 1234567)
                Match telMatch = Regex.Match(teacherHtml, "Tel\\.?[: ]+([0-9 +/]+)");
                string tel = telMatch.Success ? telMatch.Groups[1].Value.Trim() : "Keine Telefonnummer";

                // Format wie gefordert:
                result += $"{fullName}\n";
                result += $"{sprechstunde}\n";
                result += $"{mail}\n";
                result += $"{tel}\n\n";
            }
        }
    }
}