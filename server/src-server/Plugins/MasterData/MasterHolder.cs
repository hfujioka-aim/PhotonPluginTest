using System.IO;

namespace MasterData
{
    public class MasterHolder
    {
        public PhotonViewPrefabMaster PhotonView { get; private set; }

        public SkillMaster Skill { get; private set; }

        public static MasterHolder Load(string basePath)
        {
            var master = new MasterHolder();
            using (var reader = new StreamReader(Path.Combine(basePath, $"{nameof(PhotonViewPrefabMaster)}.yaml"))) {
                master.PhotonView = PhotonViewPrefabMaster.Load(reader);
            }
            using (var reader = new StreamReader(Path.Combine(basePath, $"{nameof(SkillMaster)}.yaml"))) {
                master.Skill = SkillMaster.Load(reader);
            }
            return master;
        }
    }
}
