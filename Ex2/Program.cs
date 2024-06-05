using Ex2Assembly;

namespace Ex2Launch
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var p = new Polyclinic(
                maxReceiptTime: 13000, 
                doctorsCount:4,
                viewingCapacity:5,
                standingNappingTime:4000,
                generateMaxNappingTime:6000,
                infectedNappingTime:4000,
                logger: new LogProject.Logger(".\\Polyclinic")
                );
            p.StartExecution();
       
            Thread.Sleep(15000);
          
        }
    }
}