using core.models.descriptor;
using Godot;

namespace Controller
{
    partial class PlayerController : Control
    {
        // UI elements
        private Label resourceLabel;
        private Label buildingLabel;

        private CommunityManager currentCommunity;
        private readonly Library<ResourceDescriptor> resourceLibrary = new("Resource library");
        private readonly Library<BuildingDescriptor> buildingLibrary = new("Building library");
        private int chosenBuildingId = 0;

        private void PrintResources()
        {
            var resources = currentCommunity.storage.GetAllResources();
            string text = "";
            foreach (var res in resources)
            {
                text += $"{resourceLibrary.GetDescriptorById(res.Key).Name}: {res.Value} ";
            }

            resourceLabel.Text = text;
        }


        public override void _Ready()
        {
            resourceLabel = GetNode<Label>("VerticalBox/ResourceLabel");
            buildingLabel = GetNode<Label>("VerticalBox/BuildingLabel");

            resourceLibrary.ParseDescriptors("assets/base/resources");
            buildingLibrary.ParseDescriptors("assets/base/buildings");

            buildingLabel.Text = $"Current building: {buildingLibrary.GetDescriptorById(chosenBuildingId).Name}";
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