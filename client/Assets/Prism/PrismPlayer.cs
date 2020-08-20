
using ExitGames.Client.Photon;

using Photon.Realtime;

namespace Prism
{
    public sealed class PrismPlayer: Player
    {
        public PrismPlayer(string nickName, int actorNumber, bool isLocal, Hashtable playerProperties)
            : base(nickName, actorNumber, isLocal, playerProperties)
        {

        }
    }
}
