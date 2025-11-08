using core.models.descriptor;
using Godot;

namespace Example
{
    public partial class LibraryEg : Node
    {
        public async override void _Ready()
        {
            var buildingLibrary = new Library<BuildingDescriptor>("Building library");
            buildingLibrary.ParseDescriptors("assets/base/buildings");
        }

    }
}