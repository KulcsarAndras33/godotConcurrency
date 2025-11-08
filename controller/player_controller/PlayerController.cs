using Godot;

namespace Controller
{
    partial class PlayerController : Control
    {
        // UI elements
        private Label resourceLabel;

        private CommunityManager currentCommunity;

        private void PrintResources()
        {
            var resources = currentCommunity.storage.GetAllResources();
            string text = "";
            foreach (var res in resources)
            {
                text += $"{res.Key}: {res.Value} ";
            }

            resourceLabel.Text = text;
        }


        public override void _Ready()
        {
            resourceLabel = GetNode<Label>("VerticalBox/ResourceLabel");
        }

        public override void _Process(double delta)
        {
            if (currentCommunity == null)
            {
                return;
            }

            PrintResources();
        }

        public void SetCommunity(CommunityManager communityManager)
        {
            currentCommunity = communityManager;
        }
    }
}