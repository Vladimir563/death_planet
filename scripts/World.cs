using Godot;

public partial class World : Node2D
{

    public override void _PhysicsProcess(double delta)
    {
        DecreaseFogDensity();
    }

    public void DecreaseFogDensity()
	{
		if(Input.IsActionPressed("decrease_fog_density"))
		{
			var fog = GetNode<ParallaxBackground>("fog") as ParallaxBackground;
			var parallaxLayer = fog.GetChild<ParallaxLayer>(0) as ParallaxLayer;
			var colorRect = parallaxLayer.GetChild<ColorRect>(0) as ColorRect;

            var fogShaderMaterial = colorRect.Material as ShaderMaterial;

            var currentDensity = (float)fogShaderMaterial.GetShaderParameter("density");
            var newDensity = Mathf.Max(currentDensity - 0.01f, 0); // Плавное уменьшение
            fogShaderMaterial.SetShaderParameter("density", newDensity);
		}	
	}
}
