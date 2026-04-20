using System.Collections.Generic;
using CFixer.Helpers;
using CrapFixer;

public class FeatureNode
{
    public string Name { get; set; }
    public bool IsCategory => Feature == null;
    public FeatureBase Feature { get; }
    public List<FeatureNode> Children { get; set; } = new List<FeatureNode>();

    public bool DefaultChecked { get; set; } = true;

    public FeatureNode(string name)
    {
        Name = Localization.T(name);
    }

    public FeatureNode(FeatureBase feature)
    {
        Feature = feature;
        Name = Localization.T(feature.ID());
    }
}
