using Ex6Assembly;
using LogProject;

int size = 10000;
Multiplication.Muiltiple(size, size, getVals1, size, size, getVals2, "..\\..\\..\\Resources\\Output.txt");
Multiplication.Muiltiple("..\\..\\..\\Resources\\Input1.txt", "..\\..\\..\\Resources\\Input2.txt", "..\\..\\..\\Resources\\Output2.txt");


//интерфейс. один файл, другой фукнцией. 
decimal getVals1(int i, int j)
{
    return i;
}
decimal getVals2(int i, int j)
{
    return i;
}