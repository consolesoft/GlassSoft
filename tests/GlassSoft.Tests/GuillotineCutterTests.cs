using GlassSoft.Application.Services.Production;

namespace GlassSoft.Tests;

public class GuillotineCutterTests
{
    [Fact]
    public void RemainingPieces_AreNotExpandedAgainOnNextPlate()
    {
        var pieces = new List<CutPiece> { new() { Width = 40, Height = 40, Quantity = 3 } };

        var firstPlate = new GuillotineCutter(50, 50, 0);
        var firstRemaining = firstPlate.Pack(pieces);
        var secondPlate = new GuillotineCutter(50, 50, 0);
        var secondRemaining = secondPlate.Pack(firstRemaining);

        Assert.Single(firstPlate.PlacedPieces);
        Assert.Equal(2, firstRemaining.Count);
        Assert.All(firstRemaining, piece => Assert.Equal(1, piece.Quantity));
        Assert.Single(secondPlate.PlacedPieces);
        Assert.Single(secondRemaining);
    }

    [Fact]
    public void Pack_PreservesOrderLineAndProducesNonOverlappingPlacement()
    {
        var cutter = new GuillotineCutter(100, 100, 2);
        var remaining = cutter.Pack([
            new CutPiece { Width = 40, Height = 60, OrderLineId = 11 },
            new CutPiece { Width = 30, Height = 30, OrderLineId = 22 }
        ]);

        Assert.Empty(remaining);
        Assert.Equal([11, 22], cutter.PlacedPieces.Select(p => p.OrderLineId).Order().ToArray());
        var first = cutter.PlacedPieces[0];
        var second = cutter.PlacedPieces[1];
        var overlaps = first.X < second.X + second.Width && first.X + first.Width > second.X &&
                       first.Y < second.Y + second.Height && first.Y + first.Height > second.Y;
        Assert.False(overlaps);
    }
}
