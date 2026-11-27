using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace MiloShop.Web.Validation;

/// <summary>Accepts static SVG artwork before it is written to a public directory.</summary>
internal static partial class SvgIcon
{
    public const int MaxBytes = 128 * 1024;
    private const string SvgNamespace = "http://www.w3.org/2000/svg";

    private static readonly HashSet<string> Elements = new(StringComparer.Ordinal)
    {
        "svg", "g", "defs", "title", "desc", "path", "rect", "circle", "ellipse",
        "line", "polyline", "polygon", "linearGradient", "radialGradient", "stop"
    };

    private static readonly HashSet<string> Attributes = new(StringComparer.Ordinal)
    {
        "id", "viewBox", "preserveAspectRatio", "width", "height", "x", "y", "x1", "x2", "y1", "y2",
        "cx", "cy", "r", "rx", "ry", "fx", "fy", "fr", "d", "points", "transform", "opacity",
        "fill", "fill-opacity", "fill-rule", "stroke", "stroke-width", "stroke-opacity",
        "stroke-linecap", "stroke-linejoin", "stroke-miterlimit", "stroke-dasharray", "stroke-dashoffset",
        "vector-effect", "gradientUnits", "gradientTransform", "spreadMethod", "offset",
        "stop-color", "stop-opacity", "color"
    };

    [GeneratedRegex(@"^url\(#[A-Za-z_][A-Za-z0-9_.:-]*\)$", RegexOptions.CultureInvariant)]
    private static partial Regex LocalPaint();

    [GeneratedRegex(@"^[a-zA-Z0-9#.,%+()\s-]+$", RegexOptions.CultureInvariant)]
    private static partial Regex Paint();

    public static XDocument Read(Stream stream)
    {
        using var reader = XmlReader.Create(stream, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            MaxCharactersInDocument = MaxBytes,
            IgnoreComments = true
        });
        var document = XDocument.Load(reader);
        if (document.Root?.Name != XName.Get("svg", SvgNamespace) ||
            document.DescendantNodes().OfType<XProcessingInstruction>().Any())
            throw new XmlException("Expected a static SVG icon.");

        foreach (var element in document.Descendants())
        {
            if (element.Ancestors().Skip(32).Any() || element.Name.NamespaceName != SvgNamespace || !Elements.Contains(element.Name.LocalName))
                throw new XmlException("Unsupported SVG element.");

            foreach (var attribute in element.Attributes())
            {
                if (attribute.IsNamespaceDeclaration)
                    continue;

                var name = attribute.Name.LocalName;
                if (attribute.Name.Namespace != XNamespace.None || !Attributes.Contains(name))
                    throw new XmlException("Unsupported SVG attribute.");

                if (name is "fill" or "stroke" or "color" or "stop-color")
                {
                    var value = attribute.Value.Trim();
                    if (!LocalPaint().IsMatch(value) &&
                        (!Paint().IsMatch(value) || value.Contains("url", StringComparison.OrdinalIgnoreCase)))
                        throw new XmlException("Unsupported SVG paint reference.");
                }
            }
        }

        return document;
    }
}
