using core.models.descriptor;
using Godot;

namespace Example
{
    public partial class LibraryEg : Node
    {
        public async override void _Ready()
        {
            var buildingLibrary = Library<BuildingDescriptor>.GetInstance();
            buildingLibrary.ParseDescriptors("assets/base/buildings");
        }

    }
}