namespace SilkyUIFramework;

public class RenderStates
{
    public BlendState BlendState { get; set; }
    public SamplerState SamplerState { get; set; }
    public DepthStencilState DepthStencilState { get; set; }
    public RasterizerState RasterizerState { get; set; }

    public void Begin(SpriteBatch spriteBatch, SpriteSortMode spriteSortMode, Effect effect, Matrix matrix)
    {
        spriteBatch.Begin(spriteSortMode, BlendState, SamplerState, DepthStencilState, RasterizerState, effect, matrix);
    }

    public static RenderStates CurrentStates(GraphicsDevice device)
    {
        return new RenderStates
        {
            BlendState = device.BlendState,
            SamplerState = device.SamplerStates[0],
            DepthStencilState = device.DepthStencilState,
            RasterizerState = device.RasterizerState
        };
    }
}