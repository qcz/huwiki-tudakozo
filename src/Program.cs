using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using DotNetWikiBot;
using System.IO;

namespace TudakozóDotNet
{
	class Program: Bot
	{

		static void Main(string[] args)
		{
			var loginDataPath = Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), "Cache", "Defaults.dat");
			string[] loginContents = null;
			try
			{
				loginContents = File.ReadAllText(loginDataPath)
					.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
			}
			catch (Exception)
			{
				Console.WriteLine("Nem sikerült beolvasni a bejelentkezési adatokat.");
			}

			if (loginContents == null || loginContents.Length < 3)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Hiba: hiányzik a Cache/Defaults.dat fájl");
				Console.ReadKey();
				return;
			}

			if (loginContents[0].StartsWith("https://") == false)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Hiba: a Cache/Defaults.dat fájlban a wikipédia elérési útja nem HTTPS-sel kezdődik\n  (pl. https://hu.wikipedia.org/ )");
				Console.ReadKey();
				return;
			}

			Site huwiki = new Site(loginContents[0], loginContents[1], loginContents[2]);
			
			string ma = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");

			Page maiArch = new Page(huwiki, "Wikipédia:Tudakozó/Archívum/" + ma);
			maiArch.Load();
			if (maiArch.Exists())
			{
				Console.WriteLine("Ma már történt archiválás....");
			}
			else
			{
				Page tudMost = new Page(huwiki, "Wikipédia:Tudakozó");
				tudMost.Load();
				tudMost.RenameTo(maiArch.title, "Bot: Tudakozó napi archiválása");
				if (Regex.IsMatch(tudMost.text, @"\{\{/Fejrész\}\}")) {
					tudMost.text = Regex.Replace(tudMost.text, @"\{\{/Fejrész\}\}", @"<noinclude>{{Tudakozó-keretes}}</noinclude>");
					tudMost.Save(tudMost.text, "Bot: fejrész cseréje az archívumsablonra", false);
				}

				Page ujMost = new Page(huwiki, "Wikipédia:Tudakozó");
				ujMost.Save("{{/Fejrész}}", "Bot: új, üres oldal elhelyezése", true);
			}
		}
	}
}
