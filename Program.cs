using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Steamworks;

namespace FAFLauncher {
	
	class Program {
		static Assembly resolver(object sender, ResolveEventArgs e) {
			var resourceName = "ForgedAlliance.libraries." + new AssemblyName(e.Name).Name + ".dll";
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)) {
				Byte[] assemblyData = new byte[stream.Length];
				stream.Read(assemblyData, 0, assemblyData.Length);
				return Assembly.Load(assemblyData);
			}
		}
     
		public static void Main(string[] args) {
			AppDomain.CurrentDomain.AssemblyResolve += resolver;
			runGame(args);
		}

		const string appID = "9420"; //AppID Supreme Commander: Forged Alliance
		const string fafDir = "c:\\ProgramData\\FAForever\\bin";
		const string fafExecutable = "ForgedAlliance.bexe";
		const string FAFFullPath = fafDir + "\\" + fafExecutable;

		static void runGame(String[] args) {
			Directory.SetCurrentDirectory(fafDir);

			var pi = new ProcessStartInfo();
			pi.FileName = FAFFullPath;  
			pi.WorkingDirectory = fafDir;
			pi.Arguments = String.Join(" ", args);
			pi.UseShellExecute = false;
			
			var game = new Process();
			game.StartInfo = pi;

			//we can turn-off Steam with /nosteam. Practical for launching dev environment and keep this launcher in-place.
			var wantUseSteam = !args.Contains("/nosteam");
			var isSteamApiLoaded = false;

			try {
				if (wantUseSteam) {
					Environment.SetEnvironmentVariable("SteamAppID", appID);
					var init = SteamAPI.Init(); 
					if (!init) {
						File.WriteAllText("steam_appid.txt", appID);
						SteamAPI.Init();
						File.Delete("steam_appid.txt");
					}
					isSteamApiLoaded = true;
				}
			} catch {
				//ignore any Steam fail
				try {
					SteamAPI.Shutdown();
				} catch {
				}
			}

			//launch the game with Steam or not
			if (isSteamApiLoaded) {
				Thread.Sleep(100); //give some time to Steam catch-in
				game.Start(); //start game
				game.WaitForInputIdle();
				game.WaitForExit();
				SteamAPI.Shutdown(); //terminate Steam Api when game ends
			} else
				//we don't have or wanted Steam, the wrapper launch the game and exits
				game.Start();
		}
	}
}
