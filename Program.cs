using System;
using System.Threading;


namespace AreYouGoingBot
{
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("Enter bot token:");
            var token = Console.ReadLine();
            
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            
            
            try
            {
                var handlers = new Bot.Handlers(token, cancellationToken);
                Console.ReadLine();
                
                cts.Cancel(); // stop receiving updates
            }
            catch (Exception)
            {
                Console.WriteLine("Invalid token.");
            }
        }
    }
}