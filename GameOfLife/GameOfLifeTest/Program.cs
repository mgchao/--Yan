using GameOfLife;
using System;

namespace GameOfLifeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //创建一个基于 散列域 的散列宇宙
            HashUniverse universe = new HashUniverse(4);
            //创建一个普通宇宙
            //Universe universe = new Universe(4);
            //向宇宙中添加活细胞
            for (int i = -16; i < 16; i++)
            {
                universe.AddCell(i, 0);
            }
            //演化至少 256 代
            while (universe.GenerationCount < 256)
            {
                universe.Evolve();
                Console.WriteLine(universe.ToString());
            }
            //输出局部结果
            for (int i = -32; i < 32; i++)
            {
                for (int j = -32; j < 32; j++)
                {
                    Console.Write(universe.Field.GetCell(j, i).IsAlive ? " o" : "  ");
                }
                Console.WriteLine();
            }
            Console.ReadKey();
        }
    }
}
