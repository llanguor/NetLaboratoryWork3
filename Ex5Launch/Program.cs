using Ex5Assembly;
using Ex5GenerateRandomPersons;
using LogProject;
using System.Timers;
//new InfectionGenerateRandom().GenerateFile(path: "..\\..\\..\\Resources\\InputRandom.txt", count: 50, probability: (float)0.2);



//string path = "..\\..\\..\\Resources\\InputRandom50.txt";
string path = "..\\..\\..\\Resources\\InputRandom100.txt";
//string path = "..\\..\\..\\Resources\\InputRandom10000.txt";
using var infect = new Infection(
    path: path,
    probabilityOfRecovery: (float)0.15,
    probabilityOfGettingSick: (float)0.3,
    logger: new Logger(".\\Infection.txt"));
await infect.SimulateAnInfection(-1);

