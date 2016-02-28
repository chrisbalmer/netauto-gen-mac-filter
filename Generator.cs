using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace generateaironetmacfilter
{


	public class Generator
	{
		private const string ConfigFile = "config.txt";
		private const string VendorFile = "vendors.txt";
		
		private string tempFile = string.Empty;
		private string pathToVendorList = string.Empty;
		private int aclNumber = 0;
		private string allowAccess = string.Empty;
		private Dictionary<string,List<string>> vendors;
		
		public Generator ()
		{
			this.vendors = new Dictionary<string, List<string>>();
			
			this.GetVendors();
			this.GetConfiguration();
		}
		
		public void Generate()
		{
			this.DownloadFile();
			this.ParseFile();
			this.StartOutput();
			this.DisplayVendorMatches();
			this.FinishOutput();
		}
		
		private void GetVendors()
		{
			using (StreamReader vendorReader = new StreamReader(VendorFile))
			{
				string line;
				while ((line = vendorReader.ReadLine()) != null)
					this.vendors.Add(line, new List<string>());
			}
		}
		
		private void GetConfiguration()
		{
			using (StreamReader config = new StreamReader(ConfigFile))
			{
				string line;
				pathToVendorList = config.ReadLine();
				tempFile = config.ReadLine();
				aclNumber = int.Parse(config.ReadLine());
				while ((line = config.ReadLine()) != null)
					if (line.Substring(0,1) != "!")
						allowAccess += "access-list " + this.aclNumber + " permit " + line + " 0000.0000.0000\n";
			}
		}

		private void StartOutput()
		{
			Console.WriteLine("conf t");
			Console.WriteLine("no access-list " + this.aclNumber);
			Console.WriteLine("no dot11 association mac-list " + this.aclNumber);
			Console.WriteLine(this.allowAccess);
		}

		private void FinishOutput()
		{
			Console.WriteLine("access-list " + this.aclNumber + " permit 0000.0000.0000   ffff.ffff.ffff");
			Console.WriteLine("dot11 association mac-list " + this.aclNumber);
		}

		private void DisplayVendorMatches()
		{
			int x = 0;
			foreach(string vendor in this.vendors.Keys)
			{
				Console.WriteLine("! " + vendor);
				foreach(string result in this.vendors[vendor])
				{
					Console.WriteLine(result);
					x++;
				}
				
			}
			Console.WriteLine("! " + x.ToString() + " MAC groups blocked.");
		}

		private void ParseFile()
		{
			using (StreamReader reader = new StreamReader(this.tempFile))
			{
				String line;
				Regex regex = new Regex(@"^[A-Fa-f0-9]{6}\s{5}\(base 16\)\s{2}.+$");
				Match match;
				while ((line = reader.ReadLine()) != null)
				{
					match = regex.Match(line);
					if (match.Success)
						foreach(string vendor in this.vendors.Keys)
							if (line.ToLower().IndexOf("\t" + vendor.ToLower()) >= 0)
								this.vendors[vendor].Add(this.ConvertToConfigString(line));
				}
			}
		}

		private string ConvertToConfigString(string line)
		{
			string config = "access-list ";
			config += this.aclNumber.ToString();
			config += " deny ";
			config += line.Substring(0,4);
			config += ".";
			config += line.Substring(4,2);
			config += "00.0000 0000.00FF.FFFF";
			
			return config;
		}

		private void DownloadFile()
		{
			WebClient client = new WebClient();
			client.DownloadFile(new Uri(this.pathToVendorList), this.tempFile);
		}

	}
}
