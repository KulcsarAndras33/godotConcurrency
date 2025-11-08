using Controller;
using Godot;

namespace Example
{
    public partial class PlayerControllerEg : Node
    {
        public async override void _Ready()
        {
            var communityManager = new CommunityManager();
            communityManager.storage.TryStore(0, 200);

            var playerController = GetNode<PlayerController>("PlayerController");
            playerController.SetCommunity(communityManager);
        }

    }
}