using UnityEngine;
using UnityEngine.Rendering.Universal;

// ShadowCaster2D di dinding + pintu supaya lampu ruangan tidak tembus.
public static class LightOccluders2D
{
    static bool installed;

    public static void Install()
    {
        if (installed) return;
        installed = true;

        GameObject wall = GameObject.Find("Wall");
        if (wall != null)
            AddOnRenderers(wall.GetComponentsInChildren<SpriteRenderer>(true));

        foreach (Door door in Object.FindObjectsByType<Door>())
            AddOnRenderers(door.GetComponentsInChildren<SpriteRenderer>(true));
    }

    static void AddOnRenderers(SpriteRenderer[] renderers)
    {
        if (renderers == null) return;

        foreach (SpriteRenderer sr in renderers)
        {
            if (sr == null || sr.sprite == null) continue;
            if (sr.color.a < 0.2f) continue;
            if (sr.GetComponent<Light2D>() != null) continue;
            if (sr.GetComponent<ShadowCaster2D>() != null) continue;

            ShadowCaster2D caster = sr.gameObject.AddComponent<ShadowCaster2D>();
            caster.castsShadows = true;
            caster.selfShadows = false;
        }
    }
}
