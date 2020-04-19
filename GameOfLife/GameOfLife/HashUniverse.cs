using System;
using System.Collections.Generic;
using System.Text;

namespace GameOfLife
{
    public class HashUniverse : Universe
    {
        public HashUniverse(int level) :base(level)
        {
            Level = level;
            Field = HashField.Create(level);
        }
        public override void Evolve()
        {
            //确保宇宙足够大
            while (Field.Level < Level ||
                   Field.Northwest.Population != Field.Northwest.Southeast.Southeast.Population ||
                   Field.Northeast.Population != Field.Northeast.Southwest.Southwest.Population ||
                   Field.Southwest.Population != Field.Southwest.Northeast.Northeast.Population ||
                   Field.Southeast.Population != Field.Southeast.Northwest.Northwest.Population)
               Field = Field.Expand();
            Field = Field.Evolve();
            GenerationCount += 2 << (Field.Level - 2);
        }
    }
}
