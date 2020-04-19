using System;

namespace GameOfLife
{
    /// <summary>
    /// 该类提供了有关 域 的一些基本操作
    /// </summary>
    public class Field
    {
        /// <summary>
        ///  东北角（右上、第一象限）的 子域 。
        /// </summary>
        public virtual Field Northeast { get; protected set; }

        /// <summary>
        /// 西北角（左上、第二象限）的 子域 。
        /// </summary>
        public virtual Field Northwest { get; protected set; }

        /// <summary>
        /// 西南角（左下、第三象限）的 子域 。
        /// </summary>
        public virtual Field Southwest { get; protected set; }

        /// <summary>
        /// 东南角（右下、第四象限）的 子域 。
        /// </summary>
        public virtual Field Southeast { get; protected set; }

        /// <summary>
        /// 代表当前 域 所处的级别。每个细胞的级别为 0，4×4 大小的 域 （即有四个细胞）的级别为 1。最大的 域 每扩大一次，域 的等级提升一级。
        /// </summary>
        public int Level { get; protected set; }

        /// <summary>
        /// 当前域所拥有的活细胞数
        /// </summary>
        public int Population { get; protected set; }

        /// <summary>
        /// 这个 域 有活细胞吗（如果该对象为细胞，则此值表示该细胞的生命状态）？
        /// </summary>
        public bool IsAlive { get; protected set; }


        /// <summary>
        /// 存储着该域的下一次演化结果
        /// </summary>
        protected Field NextGeneration;

        /// <summary>
        /// 创建一个指定生命状态的细胞
        /// </summary>
        /// <param name="isAlive">细胞的生命状态</param>
        protected Field(bool isAlive)
        {
            //细胞为 最小单位，不可再划分了。
            Northeast = Northwest = Southwest = Southeast = null;
            //参见 Level
            Level = 0;
            //细胞仅代表一个细胞
            IsAlive = isAlive;
            Population = isAlive ? 1 : 0;
        }

        /// <summary>
        /// 创建一个 域 （注意，该域级别基于构建此 域 的 子域，同样，所拥有的存活细胞数也基于子域）
        /// </summary>
        /// <param name="northeast">构建该域的东北角 子域</param>
        /// <param name="northwest">构建该域的西北角 子域</param>
        /// <param name="southwest">构建该域的西南角 子域</param>
        /// <param name="southeast">构建该域的东南角 子域</param>
        protected Field(Field northeast, Field northwest, Field southwest, Field southeast)
        {
            Northeast = northeast;
            Northwest = northwest;
            Southwest = southwest;
            Southeast = southeast;
            Level = northeast.Level + 1;
            Population = Northeast.Population + Northwest.Population + Southwest.Population + Southeast.Population;
            IsAlive = Population > 0 ? true : false;
        }

        /// <summary>
        /// 创建一个 细胞
        /// </summary>
        /// <param name="isAlive">细胞的生存状态</param>
        /// <returns>细胞对象（可转化为 Cell 类型）</returns>
        public virtual Field Create(bool isAlive)
        {
            return new Field(isAlive);
        }

        /// <summary>
        /// 创建一个域
        /// </summary>
        /// <param name="northeast">构建该域的东北角 子域</param>
        /// <param name="northwest">构建该域的西北角 子域</param>
        /// <param name="southwest">构建该域的西南角 子域</param>
        /// <param name="southeast">构建该域的东南角 子域</param>
        /// <returns></returns>
        public virtual Field Create(Field northeast, Field northwest, Field southwest, Field southeast)
        {
            return new Field(northeast, northwest, southwest, southeast);
        }

        /// <summary>
        /// 构建一个 level 级别的 空域
        /// </summary>
        /// <param name="level">空域 的级别</param>
        /// <returns> levle 级别的 空域</returns>
        public virtual Field CreateEmptyField(int level)
        {
            if (level == 0)
                return Create(false);
            Field subField = Create(level - 1);
            //这样做的话岂不是会存在引用不同，而指向相同的情况？
            //这并不重要，重要的是细胞自己是不是活着的——参见 Simulate() 方法
            //并且 Hashlife 算法经常引用串用。要知道，我们只记录状态，而非让每个细胞都独立存活。
            return Create(subField, subField, subField, subField);
        }

        /// <summary>
        /// 创建一个 域
        /// </summary>
        /// <returns>域 的初始级别</returns>
        public static Field Create(int level)
        {
            return (new Field(false)).CreateEmptyField(level);
        }

        /// <summary>
        /// 扩大当前 域（扩大为原先的四倍）
        /// </summary>
        /// <returns>扩大后的新域</returns>
        public virtual Field Expand()
        {
            Field subField = Create(Level - 1);
            return Create(Create(subField, subField, Northeast, subField),
                           Create(subField, subField, subField, Northwest),
                          Create(Southwest, subField, subField, subField),
                          Create(subField, Southeast, subField, subField));
        }

        /// <summary>
        /// 添加位于以该域中心为原点的坐标系上的一点的活细胞
        /// 使用左手坐标系
        /// </summary>
        /// <param name="x">活细胞的横轴坐标（ 0 代表正 1，1 代表 2）</param>
        /// <param name="y">活细胞的纵轴坐标（ 0 代表正 1，1 代表 2）</param>
        /// <returns>细胞</returns>
        public Field AddCell(int x, int y)
        {
            if (Level == 0)
                return new Field(true);
            //此节点的中心到子节点的中心的距离为该节点大小的四分之一。
            //每个 域 大小为 2^Level，所以，四分之一为 2^(Level - 2)
            int offset = 1 << (Level - 2);
            if (x >= 0)
            {
                if (y >= 0)
                    return Create(Northeast.AddCell(x - offset, y - offset), Northwest, Southwest, Southeast);
                else
                    return Create(Northeast, Northwest, Southwest, Southeast.AddCell(x - offset, y + offset));
            }
            else
            {
                if (y >= 0)
                    return Create(Northeast, Northwest.AddCell(x + offset, y - offset), Southwest, Southeast);
                else
                    return Create(Northeast, Northwest, Southwest.AddCell(x + offset, y + offset), Southeast);
            }
        }

        /// <summary>
        /// 获得以该域中心为原点的坐标系上的一点对应的细胞
        /// </summary>
        /// <param name="x">细胞的横轴坐标（ 0 代表正 1，1 代表 2）</param>
        /// <param name="y">细胞的纵轴坐标（ 0 代表正 1，1 代表 2）</param>
        /// <returns>细胞</returns>
        public Field GetCell(int x, int y)
        {
            if (Level == 0)
                return this;
            //此节点的中心到子节点的中心的距离为该节点大小的四分之一。
            //每个 域 大小为 2^Level，所以，四分之一为 2^(Level - 2)
            //使用左手坐标系
            int offset = 1 << (Level - 2);
            if (x >= 0)
            {
                if (y >= 0)
                    return Northeast.GetCell(x - offset, y - offset);
                else
                    return Southeast.GetCell(x - offset, y + offset);
            }
            else
            {
                if (y >= 0)
                    return Northwest.GetCell(x + offset, y - offset);
                else
                    return Southwest.GetCell(x + offset, y + offset);
            }
        }

        /// <summary>
        /// 将域分成 4×4 个部分，返回位于该域正中的 2×2 部分所构成的域。
        /// </summary>
        /// <returns>该域正中的 2×2 部分所构成的域</returns>
        public Field GetCenteredSubfield()
        {
            return Create(Northeast.Southwest, Northwest.Southeast, Southwest.Northeast, Southeast.Northwest);
        }

        /// <summary>
        /// 将域分成 4×4 个部分，两个域看作 8×4 的域，返回位于该域正中的 2×2 部分所构成的域。
        /// </summary>
        /// <param name="west">8×4 的域的左半部分</param>
        /// <param name="east">8×4 的域的右半部分</param>
        /// <returns>两个域正中的 2×2 部分所构成的域</returns>
        public Field GetHorizontalCenteredSubfield(Field west, Field east)
        {
            return Create(east.Northwest.Southwest, west.Northeast.Southeast, west.Southeast.Northeast, east.Southwest.Northwest);
        }

        /// <summary>
        /// 将域分成 4×4 个部分，两个域看作 4×8 的域，返回位于该域正中的 2×2 部分所构成的域。
        /// </summary>
        /// <param name="north">构成 4×8 的域的上半部分</param>
        /// <param name="north">构成 4×8 的域的下半部分</param>
        /// <returns>两个域正中的 2×2 部分所构成的域</returns>
        public Field GetVerticalCenteredSubField(Field north, Field south)
        {
            return Create(north.Southeast.Southwest, north.Southwest.Southeast, south.Northwest.Northeast, south.Northeast.Northwest);
        }

        /// <summary>
        /// 将域分成 8×8 个部分，返回位于该域正中的 2×2 部分所构成的域。
        /// </summary>
        /// <param name="north">构成 4×8 的域的上半部分</param>
        /// <param name="north">构成 4×8 的域的下半部分</param>
        /// <returns>两个域正中的 2×2 部分所构成的域</returns>
        public Field GetCenteredSubSubfield()
        {
            return Create(Northeast.Southwest.Southwest, Northwest.Southeast.Southeast, Southwest.Northeast.Northeast, Southeast.Northwest.Northwest);
        }

        /// <summary>
        /// 演化下一代
        /// </summary>
        /// <returns>演化的结果</returns>
        public virtual Field Evolve()
        {
            // 快速跳过空白区域
            if (Population == 0)
                return Northeast;//由于活细胞数为零，返回结果仅需要该域子域即可
            if (Level == 2)
                return Simulate();
            /* 将域看做 8×8 个部分
             * 88 88 88 88 88 88 88 88
             * 88 00 00 01 01 02 02 88
             * 88 00 00 01 01 02 02 88
             * 88 10 10 11 11 12 12 88
             * 88 10 10 11 11 12 12 88
             * 88 20 20 21 21 22 22 88
             * 88 20 20 21 21 22 22 88
             * 88 88 88 88 88 88 88 88
             * 共分为 9 个部分。
            */
            Field n00 = Northwest.GetCenteredSubfield(),
                     n01 = GetHorizontalCenteredSubfield(Northwest, Northeast),
                     n02 = Northeast.GetCenteredSubfield(),
                     n10 = GetVerticalCenteredSubField(Northwest, Southwest),
                     n11 = GetCenteredSubSubfield(),
                     n12 = GetVerticalCenteredSubField(Northeast, Southeast),
                     n20 = Southwest.GetCenteredSubfield(),
                     n21 = GetHorizontalCenteredSubfield(Southwest, Southeast),
                     n22 = Southeast.GetCenteredSubfield();
            //能运行到这说明 NextGeneration 是空的
            return Create(Create(n02, n01, n11, n12).Evolve(),
                          Create(n01, n00, n10, n11).Evolve(),
                          Create(n11, n10, n20, n21).Evolve(),
                          Create(n12, n11, n21, n22).Evolve());
        }

        /// <summary>
        /// 通过计算模拟演化（供级别为 2 的域使用）
        /// </summary>
        /// <returns></returns>
        protected Field Simulate()
        {
            /// <summary>
            /// 根据周围的细胞的存活数返回结果
            /// </summary>
            /// <param name="count"></param>
            /// <returns></returns>
            Field GetNextGeneration(int bitmask)
            {
                if (bitmask == 0)
                    return Create(false);
                int count = 0;
                bool isAlive = ((bitmask >> 5) & 1) != 0;
                bitmask &= 0b1111111111011111;
                while (bitmask != 0)
                {
                    count++;
                    bitmask &= bitmask - 1;
                }
                //按照生命游戏的规则
                if (count == 3 || (count == 2 && isAlive))
                    return Create(true);
                else
                    return Create(false);
            }
            int allbits = 0;
            //每一位表示一个人口，共 16 个细胞
            allbits |= GetCell(-2, -2).Population;
            allbits |= GetCell(-1, -2).Population << 1;
            allbits |= GetCell(0, -2).Population << 2;
            allbits |= GetCell(1, -2).Population << 3;
            allbits |= GetCell(-2, -1).Population << 4;
            allbits |= GetCell(-1, -1).Population << 5;
            allbits |= GetCell(0, -1).Population << 6;
            allbits |= GetCell(1, -1).Population << 7;
            allbits |= GetCell(-2, 0).Population << 8;
            allbits |= GetCell(-1, 0).Population << 9;
            allbits |= GetCell(0, 0).Population << 10;
            allbits |= GetCell(1, 0).Population << 11;
            allbits |= GetCell(-2, 1).Population << 12;
            allbits |= GetCell(-1, 1).Population << 13;
            allbits |= GetCell(0, 1).Population << 14;
            allbits |= GetCell(1, 1).Population << 15;
            return Create(GetNextGeneration((allbits & 0b1110111011100000) >> 5), GetNextGeneration((allbits & 0b0111011101110000) >> 4), GetNextGeneration(allbits & 0b0000011101110111), GetNextGeneration((allbits & 0b0000111011101110) >> 1));
        }

        /// <summary>
        /// 搜索 域 的所有活细胞
        /// </summary>
        /// <param name="onSearched">搜索到活细胞时调用该事件</param>
        /// <param name="field">待搜索的 域</param>
        public static void SearchAliveCells(Action<int, int> onSearched, Field field)
        {
            SearchAliveCellsHelper(onSearched, field, 0, 0);
        }

        private static void SearchAliveCellsHelper(Action<int, int> onSearched, Field field, int x, int y)
        {
            if (field.Population == 0)
                return;
            //还是挺简单的
            if (field.Level == 0)
            {
                if (field.IsAlive)
                    onSearched(x, y);
                return;
            }
            //每个 域 大小为 2^Level，所以，四之一为 2^(Level - 2)
            int offset = (1 << (field.Level)) / 4;
            if (offset == 0)
            {
                SearchAliveCellsHelper(onSearched, field.Northeast, x, y);
                SearchAliveCellsHelper(onSearched, field.Northwest, x -1, y);
                SearchAliveCellsHelper(onSearched, field.Southwest, x - 1, y - 1);
                SearchAliveCellsHelper(onSearched, field.Southeast, x , y -1);
                return;
            }
            SearchAliveCellsHelper(onSearched, field.Northeast, x + offset, y + offset);
            SearchAliveCellsHelper(onSearched, field.Northwest, x - offset, y + offset);
            SearchAliveCellsHelper(onSearched, field.Southwest, x - offset, y - offset);
            SearchAliveCellsHelper(onSearched, field.Southeast, x + offset, y - offset);
        }
    }
}
