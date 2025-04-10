using devbuddy.common.Applications;

namespace devbuddy.plugins.UML.Models
{
    public class UmlDesignerDataModel : CustomDataModelBase
    {
        public List<UmlDiagram> SavedDiagrams { get; set; } = [];
    }

    public class UmlDiagram
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "Untitled Diagram";
        public string Description { get; set; } = "";
        public string DiagramType { get; set; } = "class"; // class, sequence, activity, etc.
        public string XmlContent { get; set; } = "";
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
    }
}