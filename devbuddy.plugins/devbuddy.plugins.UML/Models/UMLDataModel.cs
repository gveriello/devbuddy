using devbuddy.common.Applications;
using System;
using System.Collections.Generic;

namespace devbuddy.plugins.UML.Models
{
    public class UMLDataModel : CustomDataModelBase
    {
        public List<SavedDiagram> SavedDiagrams { get; set; } = new List<SavedDiagram>();
    }

    public class SavedDiagram
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public DateTime LastModified { get; set; }
    }
}