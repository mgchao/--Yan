using System;

namespace GameOfLife
{
    public class Universe
    {
        /// <summary>
        /// 实例化宇宙
        /// </summary>
        /// <param name="level">宇宙的初始级别</param>
        public Universe(int level)
        {
            Level = level;
            Field = Field.Create(level);
        }
        /// <summary>
        /// 宇宙的初始级别
        /// </summary>
        protected int Level = 0;

        /// <summary>
        /// 演化的次数
        /// </summary>
        public int GenerationCount { get; protected set; }

        /// <summary>
        /// 构成这个宇宙的 基本域
        /// </summary>
        public Field Field { get;protected set; }

        /// <summary>
        /// 添加位于以该宇宙中心为原点的坐标系上的一点的活细胞
        /// </summary>
        /// <param name="x">活细胞的横轴坐标</param>
        /// <param name="y">活细胞的纵轴坐标</param>
        /// <returns>添加的细胞</returns>
        public void AddCell(int x, int y)
        {
            //确保宇宙足够大
            while (true)
            {
                int maxCoordinate = 1 << (Field.Level - 1);
                if (-maxCoordinate <= x && x <= maxCoordinate - 1 &&
                    -maxCoordinate <= y && y <= maxCoordinate - 1)
                    break;
                Field = Field.Expand();
            }
            //添加细胞
            Field = Field.AddCell(x, y);
        }

        /// <summary>
        /// 演化一次
        /// </summary>
        public virtual void Evolve()
        {
            if (Field.Population == 0)
                return;
            //确保宇宙足够大
            while (Field.Level < Level ||
                   Field.Northwest.Population != Field.Northwest.Southeast.Southeast.Population ||
                   Field.Northeast.Population != Field.Northeast.Southwest.Southwest.Population ||
                   Field.Southwest.Population != Field.Southwest.Northeast.Northeast.Population ||
                   Field.Southeast.Population != Field.Southeast.Northwest.Northwest.Population)
                Field = Field.Expand();
            Field = Field.Evolve();
            GenerationCount++;
        }

        /// <summary>
        /// 搜索该宇宙的所有活细胞
        /// 使用左手坐标系
        /// </summary>
        /// <param name="onSearched">搜索到活细胞时调用该事件（第一个参数表示该细胞于宇宙中横轴位置，第二个表示纵轴）</param>
        public void SearchAliveCells(Action<int,int> onSearched)
        {
            Field.SearchAliveCells(onSearched, Field);
        }

        /// <summary>
        /// 返回当前宇宙的演化次数和细胞数
        /// </summary>
        /// <returns>以字符串的形式表示的宇宙的演化次数和细胞数</returns>
        public override string ToString()
        {
            return $"演化次数：{GenerationCount};细胞数：{Field.Population}";
        }
    }
}
