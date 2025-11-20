using core.models.descriptor;
using Godot;

namespace Controller
{
    partial class PlayerController : Node
    {
        static readonly PackedScene CHUNK_VISUALISER_SCENE = GD.Load<PackedScene>("res://examples/utils/NaiveChunkVisualiser.tscn");

        // UI elements
        private Label resourceLabel;
        private Label buildingLabel;

        private CommunityManager currentCommunity;
        private ChunkManager chunkManager;
        private NaiveChunkVisualiser chunkVisualiser;
        private readonly Library<ResourceDescriptor> resourceLibrary = Library<ResourceDescriptor>.GetInstance();
        private readonly Library<BuildingDescriptor> buildingLibrary = Library<BuildingDescriptor>.GetInstance();
        private readonly Library<AgentDescriptor> agentLibrary = Library<AgentDescriptor>.GetInstance();
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

        private void HandleLeftClick(InputEventMouseButton eventMouseButton)
        {
            var camera = GetParent<Camera3D>();
            var point = camera.ProjectPosition(eventMouseButton.Position, 1);
            var dir = point - camera.GlobalPosition;
            var rayCast = new ChunkRaycast(camera.GlobalPosition, dir.Normalized(), 150);
            var collision = rayCast.GetClosestHit();

            if (collision != null)
            {
                GD.Print((Vector3I)collision);
                if (Input.IsActionPressed("Control"))
                {
                    var chunk = ChunkManager.GetInstance().GetChunkByPos((Vector3I)collision);
                    if (chunk.IsDetailed)
                    {
                        chunk.ToAbstract();
                    }
                    else
                    {
                        chunk.ToDetailed();
                    }
                    chunkVisualiser.Create(ChunkManager.GetInstance());
                }
                else
                {
                    BuildTask buildTask = new(new Building((Vector3I)collision + new Vector3I(0, 1, 0), chosenBuildingId, currentCommunity));
                    currentCommunity.AddTask(buildTask);
                }
            }
            else
            {
                var chunk = rayCast.GetChunkHit();
                if (chunk != null && Input.IsActionPressed("Control"))
                {
                    if (chunk.IsDetailed)
                    {
                        chunk.ToAbstract();
                    }
                    else
                    {
                        chunk.ToDetailed();
                    }
                    chunkVisualiser.Create(ChunkManager.GetInstance());
                }
            }
        }

        public override void _Ready()
        {
            resourceLabel = GetNode<Label>("VerticalBox/ResourceLabel");
            buildingLabel = GetNode<Label>("VerticalBox/BuildingLabel");

            resourceLibrary.ParseDescriptors("assets/base/resources");
            buildingLibrary.ParseDescriptors("assets/base/buildings");
            agentLibrary.ParseDescriptors("assets/base/agents");

            buildingLabel.Text = $"Current building: {buildingLibrary.GetDescriptorById(chosenBuildingId).Name}";

            chunkVisualiser = CHUNK_VISUALISER_SCENE.Instantiate<NaiveChunkVisualiser>();
            AddChild(chunkVisualiser);
        }

        public override void _Process(double delta)
        {
            if (currentCommunity == null)
            {
                return;
            }

            PrintResources();
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mb
                && mb.ButtonIndex == MouseButton.Left
                && !mb.Pressed)
            {
                // ignore clicks that hit UI controls
                if (GetViewport().GuiGetFocusOwner() != null)
                    return;

                HandleLeftClick(@event as InputEventMouseButton);
            }
        }

        public void SetCommunity(CommunityManager communityManager)
        {
            currentCommunity = communityManager;
        }

        public void SetChunkManager(ChunkManager chunkManager)
        {
            this.chunkManager = chunkManager;
            chunkVisualiser.Create(chunkManager);
        }
    }
}