using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

using YamlDotNet.Serialization;

namespace MasterData
{
    public class SkillMaster: ReadOnlyDictionary<int, Skill>
    {
        protected SkillMaster(IDictionary<int, Skill> dictionary) : base(dictionary)
        {
        }

        public static SkillMaster Load(TextReader reader)
        {
            var builder = new DeserializerBuilder();
            var deserializer = builder.Build();

            var skills = deserializer.Deserialize<Skill[]>(reader)
                .ToDictionary(e => e.SkillID);
            return new SkillMaster(skills);
        }
    }

    public class Skill
    {
        public int SkillID { get; set; }

        public int DelayTime { get; set; }

        public float Radius { get; set; }

        public float PositionX { get; set; }

        public float PositionZ { get; set; }
    }
}
