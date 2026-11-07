using UnityEngine;

namespace DjVisuals
{
    /// <summary>Small material helpers. HDR colors (components above 1) feed bloom.</summary>
    public static class Mats
    {
        static Shader Get(string name)
        {
            var s = Shader.Find(name);
            if (s == null) Debug.LogError("[DjVisuals] Shader not found: " + name);
            return s;
        }

        public static Material Additive(Color hdr, float soft = 1.5f)
        {
            var m = new Material(Get("DjVisuals/Additive"));
            m.SetColor("_Color", hdr);
            m.SetFloat("_Soft", soft);
            return m;
        }

        public static GameObject Prim(PrimitiveType type, Transform parent, string name, Vector3 localPos, Vector3 localScale, Material mat)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            Object.Destroy(go.GetComponent<Collider>());
            return go;
        }
    }
}
