using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(RectTransform))]
public class UIParticleSystem : MaskableGraphic
{
    private ParticleSystem pSystem;
    private ParticleSystem.Particle[] particles;
    private Transform t;
    private ParticleSystemRenderer pRenderer;
    private Material currentMaterial;

    protected override void Awake()
    {
        base.Awake();
        t = transform;
        pSystem = GetComponent<ParticleSystem>();
        pRenderer = GetComponent<ParticleSystemRenderer>();

        if (pRenderer != null)
        {
            currentMaterial = pRenderer.sharedMaterial;
            if (currentMaterial != null)
                material = currentMaterial;
        }
    }

    private void Update()
    {
        if (pSystem == null) return;

        // Fuerza a la UI a redibujar las partículas en cada fotograma
        SetVerticesDirty();

        if (currentMaterial != null && material != currentMaterial)
        {
            material = currentMaterial;
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (pSystem == null) return;

        if (particles == null || particles.Length < pSystem.main.maxParticles)
        {
            particles = new ParticleSystem.Particle[pSystem.main.maxParticles];
        }

        int numParticles = pSystem.GetParticles(particles);
        bool isWorldSpace = pSystem.main.simulationSpace == ParticleSystemSimulationSpace.World;

        for (int i = 0; i < numParticles; i++)
        {
            ParticleSystem.Particle p = particles[i];

            Vector3 pos = isWorldSpace ? t.InverseTransformPoint(p.position) : p.position;
            float size = p.startSize * 0.5f;
            Color32 color = p.GetCurrentColor(pSystem);

            UIVertex[] quad = new UIVertex[4];

            quad[0].position = pos + new Vector3(-size, -size, 0);
            quad[0].color = color;
            quad[0].uv0 = new Vector2(0, 0);

            quad[1].position = pos + new Vector3(-size, size, 0);
            quad[1].color = color;
            quad[1].uv0 = new Vector2(0, 1);

            quad[2].position = pos + new Vector3(size, size, 0);
            quad[2].color = color;
            quad[2].uv0 = new Vector2(1, 1);

            quad[3].position = pos + new Vector3(size, -size, 0);
            quad[3].color = color;
            quad[3].uv0 = new Vector2(1, 0);

            vh.AddUIVertexQuad(quad);
        }
    }

    
    public override Texture mainTexture
    {
        get
        {
            if (currentMaterial != null && currentMaterial.mainTexture != null)
                return currentMaterial.mainTexture;
            return base.mainTexture;
        }
    }
}