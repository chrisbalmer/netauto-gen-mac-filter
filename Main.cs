using System;

namespace generateaironetmacfilter
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Generator generator = new Generator();
			generator.Generate();
			Console.WriteLine("! Finished");
		}
	}
}
