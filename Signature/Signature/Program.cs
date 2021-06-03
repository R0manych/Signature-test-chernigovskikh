using Signature.SignatureService;
using System;
using System.Diagnostics;

namespace Signature
{
    class Program
    {
        private static SignatureBuilder _signatureBuilder;

        static int Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(CancelKeyPress);
            try
            {
                Console.WriteLine("Started. Please wait for a while");
                _signatureBuilder = new SignatureBuilder(args[0], Convert.ToInt32(args[1]), new EncrypterSHA256(), new ConsoleWriter());
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                _signatureBuilder.Launch();
                var result = _signatureBuilder.CallBackResult();
                stopWatch.Stop();
                if (result == 0)
                    Console.WriteLine($"Success. Elapsed time = {stopWatch.Elapsed.TotalSeconds} sec.");
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                    Console.WriteLine(ex.InnerException.StackTrace);
                }
                return 0;
            }
        }

        static void CancelKeyPress(object sender, ConsoleCancelEventArgs _args)
        {
            if (_args.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                _signatureBuilder.Cancel();
                Console.WriteLine("\nCancelling...");
                _args.Cancel = true;
            }
        }
    }
}
