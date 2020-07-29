using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;

using YamlDotNet.Serialization;

namespace MasterData
{
    public class PhotonViewPrefabMaster: ReadOnlyDictionary<string, PhotonView>
    {
        protected PhotonViewPrefabMaster(IDictionary<string, PhotonView> dictionary): base(dictionary)
        {
        }

        public static PhotonViewPrefabMaster Load(TextReader reader)
        {
            var views = Assembly.GetExecutingAssembly().GetTypes()
                .Where(typeof(PhotonViewComponent).IsAssignableFrom);

            var builder = new DeserializerBuilder();
            foreach (var view in views) {
                builder.WithTagMapping($"!{view.Name}", view);
            }
            var deserializer = builder.Build();

            var photonViews = deserializer.Deserialize<PhotonView[]>(reader)
                .ToDictionary(e => e.Name);
            return new PhotonViewPrefabMaster(photonViews);
        }
    }

    public class PhotonView
    {
        public string Name { get; set; }

        public PhotonViewComponent[] Components { get; set; }
    }

    public abstract class PhotonViewComponent
    {
        public string TypeName { get; set; }
    }
}
