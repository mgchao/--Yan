using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GameOfLife
{
    /// <summary>
    /// 基于 域 的散列域
    /// </summary>
    public sealed class HashField : Field
    {
        /// <summary>
        /// 获得 散列细胞 对象
        /// </summary>
        /// <param name="isAlive">细胞的生存状态</param>
        private HashField(bool isAlive) : base(isAlive)
        {
        }
        /// <summary>
        /// 获得 散列域 对象
        /// </summary>
        /// <param name="northeast">构建该域的东北角 子域</param>
        /// <param name="northwest">构建该域的西北角 子域</param>
        /// <param name="southwest">构建该域的西南角 子域</param>
        /// <param name="southeast">构建该域的东南角 子域</param>
        private HashField(Field northeast, Field northwest, Field southwest, Field southeast) : base(northeast, northwest, southwest, southeast)
        {
        }

        /// <summary>
        /// 存储着 共享域
        /// </summary>
        //Canonical Fields，规范域、典范域。命名由 CanonicalTreeNode 而来
        private static Dictionary<Field, Field> CanonicalFields = new Dictionary<Field, Field>(100000);

        /// <summary>
        /// 获得 散列域 的散列码
        /// </summary>
        /// <returns>散列域 的散列码</returns>
        public override int GetHashCode()
        {
            if (Level == 0) return Population;
            //并不清楚这个散列码的性能如何
            /*       参考代码是这样实现的（Java）看不懂
             *       return System.identityHashCode(nw) +
             *       11 * System.identityHashCode(ne) +
             *       101 * System.identityHashCode(sw) +
             *       1007 * System.identityHashCode(se) ;
             */
            return (RuntimeHelpers.GetHashCode(Northeast) * Level) ^ (RuntimeHelpers.GetHashCode(Northwest) << 4) ^ (RuntimeHelpers.GetHashCode(Southwest) << 7) ^ (RuntimeHelpers.GetHashCode(Southeast) << 10);
        }

        /// <summary>
        /// 判断 域 是否是相同
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public override bool Equals(object field)
        {
            Field temp;
            if (field is Field)
                temp = (Field)field;
            else
                return false;
            if (Level != temp.Level)
                return false;
            if (Level == 0)
                return IsAlive == temp.IsAlive;
            return Northwest == temp.Northwest && Northeast == temp.Northeast && Southwest == temp.Southwest && Southeast == temp.Southeast;
        }

        /// <summary>
        /// 将 域 添加到共享域中
        /// </summary>
        /// <returns>加入到散列表中的 共享域</returns>
        //命名有待考究
        private Field GetCanonicalField()
        {
            if (CanonicalFields.ContainsKey(this))
                return CanonicalFields[this];
            CanonicalFields.Add(this, this);
            return this;
        }

        /// <summary>
        /// 将域分成 2×2 个部分，两个域看作 4×2 的域，返回位于该域正中的 2×2 部分所构成的域。
        /// </summary>
        /// <param name="west">4×2 的域的左半部分</param>
        /// <param name="east">4×2 的域的右半部分</param>
        /// <returns>两个域正中的 2×2 部分所构成的域</returns>
        public Field GetHorizontalCenteredSubfieldForward(Field west, Field east)
        {
            return Create(east.Northwest, west.Northeast, west.Southeast, east.Southwest).Evolve();
        }

        /// <summary>
        /// 将域分成 2×2 个部分，两个域看作 2×4 的域，返回位于该域正中的 2×2 部分所构成的 域 的演化结果。
        /// </summary>
        /// <param name="north">构成 2×4 的域的上半部分</param>
        /// <param name="north">构成 2×4 的域的下半部分</param>
        /// <returns>两个域正中的 2×2 部分所构成的 域 的演化结果</returns>
        public Field GetVerticalCenteredSubFieldForward(Field north, Field south)
        {
            return Create(north.Southeast, north.Southwest, south.Northwest, south.Northeast).Evolve();
        }

        /// <summary>
        /// 将域分成 4×4 个部分，返回位于该域正中的 2×2 部分所构成的 域 。
        /// </summary>
        /// <returns>该域正中的 2×2 部分所构成的 域 的演化结果</returns>
        public Field GetCenteredSubfieldForward()
        {
            return Create(Northeast.Southwest, Northwest.Southeast, Southwest.Northeast, Southeast.Northwest).Evolve();
        }

        /// <summary>
        /// 演化下一代
        /// </summary>
        /// <returns>演化的结果</returns>
        public override Field Evolve()
        {
            //调用之前演化过了的结果
            if (NextGeneration != null)
                return NextGeneration;
            // 快速跳过空白区域
            if (Population == 0)
                return Northeast;//由于活细胞数为零，返回结果仅需要该域子域即可（保证树结构稳定）
            if (Level == 2)
                return Simulate();
            /* 将域看做 4×4 个部分
             * 以 n00 举例                         n11
             * 00 00 00 00 88 88 88 88      88 88 01 01 01 01 88 88
             * 00 00 00 00 88 88 88 88      88 88 01 01 01 01 88 88
             * 00 00 00 00 88 88 88 88      88 88 01 01 01 01 88 88
             * 00 00 00 00 88 88 88 88      88 88 01 01 01 01 88 88
             * 88 88 88 88 88 88 88 88      88 88 88 88 88 88 88 88
             * 88 88 88 88 88 88 88 88      88 88 88 88 88 88 88 88
             * 88 88 88 88 88 88 88 88      88 88 88 88 88 88 88 88
             * 88 88 88 88 88 88 88 88      88 88 88 88 88 88 88 88
             * 为什么可以这样？
             * 不要忘了，Simulate 方法返回域的等级是调用此方法的域的 （level - 1）
             * 也就是说 n00 调用后返回结果的大小比是：
             * 88 88 88 88 88 88 88 88
             * 88 00 00 88 88 88 88 88
             * 88 00 00 88 88 88 88 88
             * 88 88 88 88 88 88 88 88
             * 88 88 88 88 88 88 88 88
             * 88 88 88 88 88 88 88 88
             * 88 88 88 88 88 88 88 88
             * 88 88 88 88 88 88 88 88
            */
            Field n00 = Northwest.Evolve(),
                     n01 = GetHorizontalCenteredSubfieldForward(Northwest, Northeast),
                     n02 = Northeast.Evolve(),
                     n10 = GetVerticalCenteredSubFieldForward(Northwest, Southwest),
                     n11 = GetCenteredSubfieldForward(),
                     n12 = GetVerticalCenteredSubFieldForward(Northeast, Southeast),
                     n20 = Southwest.Evolve(),
                     n21 = GetHorizontalCenteredSubfieldForward(Southwest, Southeast),
                     n22 = Southeast.Evolve();
            return NextGeneration = Create(Create(n02, n01, n11, n12).Evolve(),
                          Create(n01, n00, n10, n11).Evolve(),
                          Create(n11, n10, n20, n21).Evolve(),
                          Create(n12, n11, n21, n22).Evolve());
        }
        /// <summary>
        ///   创建一个 散列细胞
        /// </summary>
        /// <param name="living">散列细胞的初始状态</param>
        /// <returns></returns>
        public override Field Create(bool living)
        {
            return new HashField(living).GetCanonicalField();
        }

        /// <summary>
        /// 创建一个散列域
        /// </summary>
        /// <param name="northeast">构建该域的东北角 子域</param>
        /// <param name="northwest">构建该域的西北角 子域</param>
        /// <param name="southwest">构建该域的西南角 子域</param>
        /// <param name="southeast">构建该域的东南角 子域</param>
        /// <returns>创建的 散列域</returns>
        public override Field Create(Field northeast, Field northwest, Field southwest, Field southeast)
        {
            return new HashField(northeast, northwest, southwest, southeast).GetCanonicalField();
        }

        /// <summary>
        /// 扩大当前 域（扩大为原先的四倍）
        /// </summary>
        /// <returns>扩大后的新域</returns>
        public override Field Expand()
        {
            //把原来的域分开弄成新域的中心即可
            Field subField = Create(Level - 1);
            return Create(Create(subField, subField, Northeast, subField),
                           Create(subField, subField, subField, Northwest),
                          Create(Southwest, subField, subField, subField),
                          Create(subField, Southeast, subField, subField));
        }

        /// <summary>
        /// 在指定等级创建一个新的域
        /// </summary>
        /// <param name="level">创建的 域 的等级</param>
        /// <returns>创建的 域</returns>
        public new static Field Create(int level)
        {
            return new HashField(false).CreateEmptyField(level);
        }

        /// <summary>
        /// 建立一个空的 散列域
        /// </summary>
        /// <param name="level">散列域的等级</param>
        /// <returns>创建的散列域</returns>
        public override Field CreateEmptyField(int level)
        {
            if (level == 0)
                return Create(false);
            Field subField = Create(level - 1);
            //这样做的话岂不是会存在引用不同，而指向相同的情况？
            //这并不重要，重要的是细胞自己是不是活着的——参见 Simulate() 方法
            //并且 Hashlife 算法经常引用串用。要知道，我们只记录状态，而非让每个细胞都独立存活。
            return Create(subField, subField, subField, subField);
        }
    }
}
