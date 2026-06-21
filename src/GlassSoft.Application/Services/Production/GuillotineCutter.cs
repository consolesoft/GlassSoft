using System.Globalization;

namespace GlassSoft.Application.Services.Production;

/// <summary>
/// Guillotine Bin Packing algoritması - Cam kesim optimizasyonu.
/// double kullanarak yüksek performanslı hesaplama yapar.
/// </summary>
public class GuillotineCutter
{
    public double PlateWidth { get; }
    public double PlateHeight { get; }
    public double BladeThickness { get; }

    private readonly List<FreeRect> _freeRects;
    private const double MinUsableSize = 10.0; // 10mm altı parçalar atılır

    public GuillotineCutter(double plateWidth, double plateHeight, double bladeThickness = 3.0)
    {
        PlateWidth = plateWidth;
        PlateHeight = plateHeight;
        BladeThickness = bladeThickness;
        _freeRects = new List<FreeRect>(32) { new(0, 0, plateWidth, plateHeight) };
    }

    public List<PlacedPiece> PlacedPieces { get; } = new();

    /// <summary>
    /// Parçaları plakaya yerleştirir. Yerleşemeyen parçaları döndürür.
    /// </summary>
    public List<CutPiece> Pack(List<CutPiece> pieces)
    {
        // Girdi her çağrıda normalize edilir. Yerleşemeyen parçaların Quantity değeri
        // daima 1'dir; aksi halde sonraki plaka turunda miktar tekrar expand edilirdi.
        var sorted = new List<CutPiece>();
        foreach (var p in pieces
                     .Where(p => p.Width > 0 && p.Height > 0 && p.Quantity > 0)
                     .OrderByDescending(p => p.Width * p.Height)
                     .ThenByDescending(p => Math.Max(p.Width, p.Height)))
        {
            for (int i = 0; i < p.Quantity; i++)
                sorted.Add(p with { Quantity = 1 });
        }

        var notPlaced = new List<CutPiece>();

        foreach (var piece in sorted)
        {
            if (!TryPlace(piece))
                notPlaced.Add(piece);
        }

        return notPlaced;
    }

    private bool TryPlace(CutPiece piece)
    {
        int bestIndex = -1;
        bool bestRotated = false;
        double bestAreaDiff = double.MaxValue;
        var pieceArea = piece.Width * piece.Height;

        var count = _freeRects.Count;
        for (int i = 0; i < count; i++)
        {
            var fr = _freeRects[i];

            // Normal yerleşim
            if (piece.Width <= fr.Width && piece.Height <= fr.Height)
            {
                var areaDiff = (fr.Width * fr.Height) - pieceArea;
                if (areaDiff < bestAreaDiff)
                {
                    bestAreaDiff = areaDiff;
                    bestIndex = i;
                    bestRotated = false;
                }
            }

            // Döndürülmüş yerleşim
            if (piece.CanRotate && piece.Height <= fr.Width && piece.Width <= fr.Height)
            {
                var areaDiff = (fr.Width * fr.Height) - pieceArea;
                if (areaDiff < bestAreaDiff)
                {
                    bestAreaDiff = areaDiff;
                    bestIndex = i;
                    bestRotated = true;
                }
            }
        }

        if (bestIndex < 0) return false;

        var freeRect = _freeRects[bestIndex];
        var w = bestRotated ? piece.Height : piece.Width;
        var h = bestRotated ? piece.Width : piece.Height;

        PlacedPieces.Add(new PlacedPiece
        {
            X = freeRect.X,
            Y = freeRect.Y,
            Width = w,
            Height = h,
            IsRotated = bestRotated,
            OrderLineId = piece.OrderLineId
        });

        // Guillotine split: boş alanı ikiye böl
        _freeRects.RemoveAt(bestIndex);
        SplitFreeRect(freeRect, w, h);

        return true;
    }

    private void SplitFreeRect(FreeRect rect, double usedW, double usedH)
    {
        var rightW = rect.Width - usedW - BladeThickness;
        var bottomH = rect.Height - usedH - BladeThickness;

        if (rightW > MinUsableSize && bottomH > MinUsableSize)
        {
            if (rightW * rect.Height >= rect.Width * bottomH)
            {
                _freeRects.Add(new FreeRect(rect.X + usedW + BladeThickness, rect.Y, rightW, rect.Height));
                _freeRects.Add(new FreeRect(rect.X, rect.Y + usedH + BladeThickness, usedW, bottomH));
            }
            else
            {
                _freeRects.Add(new FreeRect(rect.X, rect.Y + usedH + BladeThickness, rect.Width, bottomH));
                _freeRects.Add(new FreeRect(rect.X + usedW + BladeThickness, rect.Y, rightW, usedH));
            }
        }
        else if (rightW > MinUsableSize)
        {
            _freeRects.Add(new FreeRect(rect.X + usedW + BladeThickness, rect.Y, rightW, rect.Height));
        }
        else if (bottomH > MinUsableSize)
        {
            _freeRects.Add(new FreeRect(rect.X, rect.Y + usedH + BladeThickness, rect.Width, bottomH));
        }
        // MinUsableSize altı parçalar atılır - free rect listesinin şişmesini önler
    }

    public double GetUsedArea()
    {
        double sum = 0;
        foreach (var p in PlacedPieces) sum += p.Width * p.Height;
        return sum;
    }

    public double GetPlateArea() => PlateWidth * PlateHeight;

    public double GetWastePercentage()
    {
        var plateArea = GetPlateArea();
        return plateArea == 0 ? 0 : (1.0 - GetUsedArea() / plateArea) * 100.0;
    }

    private static readonly CultureInfo IC = CultureInfo.InvariantCulture;

    public string GenerateCsv()
    {
        var lines = new List<string>(PlacedPieces.Count + 1) { "X,Y,Width,Height,Rotated,OrderLineId" };
        foreach (var p in PlacedPieces)
            lines.Add(string.Format(IC, "{0:F1},{1:F1},{2:F1},{3:F1},{4},{5}", p.X, p.Y, p.Width, p.Height, p.IsRotated ? 1 : 0, p.OrderLineId));
        return string.Join("\n", lines);
    }

    public string GenerateDxf()
    {
        var sb = new System.Text.StringBuilder(256 + PlacedPieces.Count * 200);
        sb.AppendLine("0\nSECTION\n2\nENTITIES");
        AppendDxfRect(sb, 0, 0, PlateWidth, PlateHeight, 1);
        foreach (var p in PlacedPieces)
            AppendDxfRect(sb, p.X, p.Y, p.Width, p.Height, 2);
        sb.AppendLine("0\nENDSEC\n0\nEOF");
        return sb.ToString();
    }

    private static void AppendDxfRect(System.Text.StringBuilder sb, double x, double y, double w, double h, int layer)
    {
        sb.AppendLine(string.Format(IC, "0\nLWPOLYLINE\n8\n{0}\n90\n4\n70\n1", layer));
        sb.AppendLine(string.Format(IC, "10\n{0:F2}\n20\n{1:F2}", x, y));
        sb.AppendLine(string.Format(IC, "10\n{0:F2}\n20\n{1:F2}", x + w, y));
        sb.AppendLine(string.Format(IC, "10\n{0:F2}\n20\n{1:F2}", x + w, y + h));
        sb.AppendLine(string.Format(IC, "10\n{0:F2}\n20\n{1:F2}", x, y + h));
    }
}

public record FreeRect(double X, double Y, double Width, double Height);

public record CutPiece
{
    public double Width { get; init; }
    public double Height { get; init; }
    public int Quantity { get; init; } = 1;
    public bool CanRotate { get; init; } = true;
    public int? OrderLineId { get; init; }
}

public class PlacedPiece
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public bool IsRotated { get; set; }
    public int? OrderLineId { get; set; }
}
