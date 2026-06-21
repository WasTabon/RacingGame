using System.Collections.Generic;
using UnityEngine;

public static class IconFactory
{
    private static Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
    private const int Size = 128;

    public static Sprite Get(string iconName, Color color)
    {
        string key = iconName + "_" + ColorUtility.ToHtmlStringRGBA(color);
        if (cache.ContainsKey(key)) return cache[key];

        Texture2D tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        Clear(tex);

        switch (iconName)
        {
            case "money": DrawMoney(tex, color); break;
            case "reputation": DrawStar(tex, color, 0.5f, 0.52f, 0.40f, 0.17f, 5); break;
            case "settings": DrawGear(tex, color); break;
            case "play": DrawTriangle(tex, color, 0.36f, 0.22f, 0.36f, 0.78f, 0.78f, 0.5f); break;
            case "continue": DrawContinue(tex, color); break;
            case "calendar": DrawCalendar(tex, color); break;
            case "sfx": DrawSpeaker(tex, color, true); break;
            case "music": DrawMusic(tex, color); break;
            case "mute": DrawSpeaker(tex, color, false); break;
            case "trophy": DrawTrophy(tex, color); break;
            case "speed": DrawSpeed(tex, color); break;
            case "easy": DrawRingIcon(tex, color); break;
            case "normal": DrawBars(tex, color); break;
            case "hard": DrawFlame(tex, color); break;
            case "extreme": DrawSkull(tex, color); break;
            case "flag": DrawFlag(tex, color); break;
            case "close": DrawCross(tex, color); break;
            case "back": DrawArrowLeft(tex, color); break;
            case "staff": DrawStaff(tex, color); break;
            case "research": DrawResearch(tex, color); break;
            case "car": DrawCar(tex, color); break;
            case "facility": DrawFacility(tex, color); break;
            case "home": DrawHome(tex, color); break;
            case "chevron": DrawChevron(tex, color); break;
            default: DrawRingIcon(tex, color); break;
        }

        tex.Apply();
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), 100f);
        cache[key] = sprite;
        return sprite;
    }

    private static void Clear(Texture2D tex)
    {
        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[Size * Size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;
        tex.SetPixels(pixels);
    }

    private static void Blend(Texture2D tex, int x, int y, Color c)
    {
        if (x < 0 || x >= Size || y < 0 || y >= Size) return;
        Color existing = tex.GetPixel(x, y);
        Color blended = Color.Lerp(existing, c, c.a);
        blended.a = Mathf.Max(existing.a, c.a);
        tex.SetPixel(x, y, blended);
    }

    private static void Erase(Texture2D tex, int x, int y)
    {
        if (x < 0 || x >= Size || y < 0 || y >= Size) return;
        tex.SetPixel(x, y, new Color(0, 0, 0, 0));
    }

    private static void FillCircle(Texture2D tex, float cx, float cy, float r, Color color)
    {
        int px = Mathf.RoundToInt(cx * Size);
        int py = Mathf.RoundToInt(cy * Size);
        int pr = Mathf.RoundToInt(r * Size);
        for (int y = py - pr - 1; y <= py + pr + 1; y++)
        {
            for (int x = px - pr - 1; x <= px + pr + 1; x++)
            {
                float dx = x - px;
                float dy = y - py;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist <= pr)
                {
                    float aa = Mathf.Clamp01(pr - dist);
                    Color c = color;
                    c.a *= aa;
                    Blend(tex, x, y, c);
                }
            }
        }
    }

    private static void PunchCircle(Texture2D tex, float cx, float cy, float r)
    {
        int px = Mathf.RoundToInt(cx * Size);
        int py = Mathf.RoundToInt(cy * Size);
        int pr = Mathf.RoundToInt(r * Size);
        for (int y = py - pr - 1; y <= py + pr + 1; y++)
        {
            for (int x = px - pr - 1; x <= px + pr + 1; x++)
            {
                float dx = x - px;
                float dy = y - py;
                if (Mathf.Sqrt(dx * dx + dy * dy) <= pr) Erase(tex, x, y);
            }
        }
    }

    private static void Ring(Texture2D tex, float cx, float cy, float r, float thickness, Color color)
    {
        int px = Mathf.RoundToInt(cx * Size);
        int py = Mathf.RoundToInt(cy * Size);
        int pr = Mathf.RoundToInt(r * Size);
        int th = Mathf.RoundToInt(thickness * Size);
        for (int y = py - pr - th - 1; y <= py + pr + th + 1; y++)
        {
            for (int x = px - pr - th - 1; x <= px + pr + th + 1; x++)
            {
                float dx = x - px;
                float dy = y - py;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist <= pr + th * 0.5f && dist >= pr - th * 0.5f) Blend(tex, x, y, color);
            }
        }
    }

    private static void FillRect(Texture2D tex, float x0, float y0, float x1, float y1, Color color)
    {
        int px0 = Mathf.RoundToInt(x0 * Size);
        int py0 = Mathf.RoundToInt(y0 * Size);
        int px1 = Mathf.RoundToInt(x1 * Size);
        int py1 = Mathf.RoundToInt(y1 * Size);
        for (int y = py0; y <= py1; y++)
            for (int x = px0; x <= px1; x++)
                Blend(tex, x, y, color);
    }

    private static void PunchRect(Texture2D tex, float x0, float y0, float x1, float y1)
    {
        int px0 = Mathf.RoundToInt(x0 * Size);
        int py0 = Mathf.RoundToInt(y0 * Size);
        int px1 = Mathf.RoundToInt(x1 * Size);
        int py1 = Mathf.RoundToInt(y1 * Size);
        for (int y = py0; y <= py1; y++)
            for (int x = px0; x <= px1; x++)
                Erase(tex, x, y);
    }

    private static void Line(Texture2D tex, float x0, float y0, float x1, float y1, float thickness, Color color)
    {
        int steps = Mathf.CeilToInt(Vector2.Distance(new Vector2(x0, y0), new Vector2(x1, y1)) * Size);
        if (steps < 1) steps = 1;
        float r = thickness * Size * 0.5f;
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            float fx = Mathf.Lerp(x0, x1, t) * Size;
            float fy = Mathf.Lerp(y0, y1, t) * Size;
            int cx = Mathf.RoundToInt(fx);
            int cy = Mathf.RoundToInt(fy);
            int ir = Mathf.CeilToInt(r) + 1;
            for (int yy = cy - ir; yy <= cy + ir; yy++)
                for (int xx = cx - ir; xx <= cx + ir; xx++)
                {
                    float dd = Mathf.Sqrt((xx - fx) * (xx - fx) + (yy - fy) * (yy - fy));
                    if (dd <= r) Blend(tex, xx, yy, color);
                }
        }
    }

    private static void RectOutline(Texture2D tex, float x0, float y0, float x1, float y1, float thickness, Color color)
    {
        Line(tex, x0, y0, x1, y0, thickness, color);
        Line(tex, x1, y0, x1, y1, thickness, color);
        Line(tex, x1, y1, x0, y1, thickness, color);
        Line(tex, x0, y1, x0, y0, thickness, color);
    }

    private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Sign(p, a, b);
        float d2 = Sign(p, b, c);
        float d3 = Sign(p, c, a);
        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        return !(hasNeg && hasPos);
    }

    private static void DrawTriangle(Texture2D tex, Color color, float ax, float ay, float bx, float by, float cx, float cy)
    {
        Vector2 a = new Vector2(ax * Size, ay * Size);
        Vector2 b = new Vector2(bx * Size, by * Size);
        Vector2 c = new Vector2(cx * Size, cy * Size);
        int minX = Mathf.FloorToInt(Mathf.Min(a.x, b.x, c.x));
        int maxX = Mathf.CeilToInt(Mathf.Max(a.x, b.x, c.x));
        int minY = Mathf.FloorToInt(Mathf.Min(a.y, b.y, c.y));
        int maxY = Mathf.CeilToInt(Mathf.Max(a.y, b.y, c.y));
        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
                if (PointInTriangle(new Vector2(x, y), a, b, c)) Blend(tex, x, y, color);
    }

    private static void DrawStar(Texture2D tex, Color color, float cx, float cy, float outerR, float innerR, int points)
    {
        List<Vector2> verts = new List<Vector2>();
        float angleStep = Mathf.PI / points;
        float startAngle = Mathf.PI / 2f;
        for (int i = 0; i < points * 2; i++)
        {
            float r = (i % 2 == 0) ? outerR : innerR;
            float ang = startAngle + i * angleStep;
            verts.Add(new Vector2(cx + Mathf.Cos(ang) * r, cy + Mathf.Sin(ang) * r));
        }
        for (int i = 0; i < verts.Count; i++)
        {
            Vector2 v0 = verts[i];
            Vector2 v1 = verts[(i + 1) % verts.Count];
            DrawTriangle(tex, color, cx, cy, v0.x, v0.y, v1.x, v1.y);
        }
    }

    private static void Arc(Texture2D tex, float cx, float cy, float r, float from, float to, Color color)
    {
        int segments = 48;
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            float ang = Mathf.Lerp(from, to, t);
            float x = cx + Mathf.Cos(ang) * r;
            float y = cy + Mathf.Sin(ang) * r;
            FillCircle(tex, x, y, 0.02f, color);
        }
    }

    private static void DrawMoney(Texture2D tex, Color color)
    {
        FillCircle(tex, 0.5f, 0.5f, 0.42f, color);
        PunchCircle(tex, 0.5f, 0.5f, 0.33f);
        Ring(tex, 0.5f, 0.5f, 0.33f, 0.018f, color);
        Line(tex, 0.5f, 0.28f, 0.5f, 0.72f, 0.055f, color);
        Line(tex, 0.40f, 0.40f, 0.60f, 0.40f, 0.05f, color);
        Line(tex, 0.40f, 0.60f, 0.60f, 0.60f, 0.05f, color);
    }

    private static void DrawGear(Texture2D tex, Color color)
    {
        FillCircle(tex, 0.5f, 0.5f, 0.30f, color);
        int teeth = 8;
        for (int i = 0; i < teeth; i++)
        {
            float ang = (Mathf.PI * 2f / teeth) * i;
            float tx = 0.5f + Mathf.Cos(ang) * 0.38f;
            float ty = 0.5f + Mathf.Sin(ang) * 0.38f;
            FillCircle(tex, tx, ty, 0.085f, color);
        }
        PunchCircle(tex, 0.5f, 0.5f, 0.13f);
    }

    private static void DrawContinue(Texture2D tex, Color color)
    {
        Line(tex, 0.30f, 0.26f, 0.52f, 0.5f, 0.075f, color);
        Line(tex, 0.52f, 0.5f, 0.30f, 0.74f, 0.075f, color);
        Line(tex, 0.52f, 0.26f, 0.74f, 0.5f, 0.075f, color);
        Line(tex, 0.74f, 0.5f, 0.52f, 0.74f, 0.075f, color);
    }

    private static void DrawCalendar(Texture2D tex, Color color)
    {
        RectOutline(tex, 0.22f, 0.18f, 0.78f, 0.72f, 0.04f, color);
        Line(tex, 0.22f, 0.56f, 0.78f, 0.56f, 0.04f, color);
        FillRect(tex, 0.33f, 0.72f, 0.39f, 0.82f, color);
        FillRect(tex, 0.61f, 0.72f, 0.67f, 0.82f, color);
        for (int r = 0; r < 2; r++)
            for (int c = 0; c < 3; c++)
            {
                float x = 0.30f + c * 0.14f;
                float y = 0.28f + r * 0.14f;
                FillRect(tex, x, y, x + 0.07f, y + 0.07f, color);
            }
    }

    private static void DrawSpeaker(Texture2D tex, Color color, bool on)
    {
        FillRect(tex, 0.18f, 0.40f, 0.30f, 0.60f, color);
        FillRect(tex, 0.30f, 0.40f, 0.46f, 0.60f, color);
        DrawTriangle(tex, color, 0.46f, 0.50f, 0.30f, 0.28f, 0.30f, 0.72f);
        if (on)
        {
            Arc(tex, 0.50f, 0.5f, 0.13f, -Mathf.PI / 3f, Mathf.PI / 3f, color);
            Arc(tex, 0.50f, 0.5f, 0.21f, -Mathf.PI / 3f, Mathf.PI / 3f, color);
        }
        else
        {
            Line(tex, 0.58f, 0.38f, 0.80f, 0.62f, 0.05f, color);
            Line(tex, 0.80f, 0.38f, 0.58f, 0.62f, 0.05f, color);
        }
    }

    private static void DrawMusic(Texture2D tex, Color color)
    {
        FillCircle(tex, 0.36f, 0.30f, 0.10f, color);
        FillCircle(tex, 0.64f, 0.24f, 0.10f, color);
        FillRect(tex, 0.44f, 0.30f, 0.48f, 0.74f, color);
        FillRect(tex, 0.72f, 0.24f, 0.76f, 0.68f, color);
        FillRect(tex, 0.44f, 0.68f, 0.76f, 0.74f, color);
    }

    private static void DrawTrophy(Texture2D tex, Color color)
    {
        FillRect(tex, 0.34f, 0.55f, 0.66f, 0.78f, color);
        DrawTriangle(tex, color, 0.34f, 0.55f, 0.66f, 0.55f, 0.50f, 0.38f);
        Ring(tex, 0.30f, 0.66f, 0.07f, 0.022f, color);
        Ring(tex, 0.70f, 0.66f, 0.07f, 0.022f, color);
        FillRect(tex, 0.47f, 0.30f, 0.53f, 0.40f, color);
        FillRect(tex, 0.38f, 0.24f, 0.62f, 0.31f, color);
    }

    private static void DrawSpeed(Texture2D tex, Color color)
    {
        Arc(tex, 0.5f, 0.42f, 0.34f, Mathf.PI * 0.12f, Mathf.PI * 0.88f, color);
        Line(tex, 0.5f, 0.42f, 0.66f, 0.62f, 0.045f, color);
        FillCircle(tex, 0.5f, 0.42f, 0.05f, color);
    }

    private static void DrawRingIcon(Texture2D tex, Color color)
    {
        FillCircle(tex, 0.5f, 0.5f, 0.34f, color);
        PunchCircle(tex, 0.5f, 0.5f, 0.20f);
    }

    private static void DrawBars(Texture2D tex, Color color)
    {
        FillRect(tex, 0.24f, 0.28f, 0.40f, 0.50f, color);
        FillRect(tex, 0.42f, 0.28f, 0.58f, 0.62f, color);
        FillRect(tex, 0.60f, 0.28f, 0.76f, 0.74f, color);
    }

    private static void DrawFlame(Texture2D tex, Color color)
    {
        DrawTriangle(tex, color, 0.30f, 0.32f, 0.70f, 0.32f, 0.50f, 0.80f);
        FillCircle(tex, 0.50f, 0.40f, 0.18f, color);
        PunchCircle(tex, 0.50f, 0.44f, 0.08f);
    }

    private static void DrawSkull(Texture2D tex, Color color)
    {
        FillCircle(tex, 0.5f, 0.54f, 0.26f, color);
        FillRect(tex, 0.37f, 0.30f, 0.63f, 0.56f, color);
        PunchCircle(tex, 0.42f, 0.56f, 0.07f);
        PunchCircle(tex, 0.58f, 0.56f, 0.07f);
        PunchRect(tex, 0.455f, 0.30f, 0.485f, 0.42f);
        PunchRect(tex, 0.515f, 0.30f, 0.545f, 0.42f);
    }

    private static void DrawFlag(Texture2D tex, Color color)
    {
        Line(tex, 0.30f, 0.18f, 0.30f, 0.82f, 0.045f, color);
        FillRect(tex, 0.30f, 0.52f, 0.74f, 0.80f, color);
        PunchRect(tex, 0.37f, 0.59f, 0.44f, 0.66f);
        PunchRect(tex, 0.51f, 0.59f, 0.58f, 0.66f);
        PunchRect(tex, 0.44f, 0.66f, 0.51f, 0.73f);
        PunchRect(tex, 0.58f, 0.66f, 0.65f, 0.73f);
        PunchRect(tex, 0.65f, 0.59f, 0.72f, 0.66f);
        PunchRect(tex, 0.37f, 0.73f, 0.44f, 0.80f);
    }

    private static void DrawCross(Texture2D tex, Color color)
    {
        Line(tex, 0.32f, 0.32f, 0.68f, 0.68f, 0.075f, color);
        Line(tex, 0.68f, 0.32f, 0.32f, 0.68f, 0.075f, color);
    }

    private static void DrawArrowLeft(Texture2D tex, Color color)
    {
        DrawTriangle(tex, color, 0.28f, 0.5f, 0.54f, 0.28f, 0.54f, 0.72f);
        FillRect(tex, 0.50f, 0.44f, 0.74f, 0.56f, color);
    }

    private static void DrawStaff(Texture2D tex, Color color)
    {
        FillCircle(tex, 0.5f, 0.66f, 0.15f, color);
        DrawTriangle(tex, color, 0.24f, 0.24f, 0.76f, 0.24f, 0.5f, 0.52f);
        FillRect(tex, 0.28f, 0.24f, 0.72f, 0.36f, color);
    }

    private static void DrawResearch(Texture2D tex, Color color)
    {
        DrawTriangle(tex, color, 0.5f, 0.62f, 0.26f, 0.22f, 0.74f, 0.22f);
        FillRect(tex, 0.44f, 0.58f, 0.56f, 0.80f, color);
        FillRect(tex, 0.40f, 0.78f, 0.60f, 0.84f, color);
        PunchRect(tex, 0.36f, 0.30f, 0.64f, 0.38f);
    }

    private static void DrawCar(Texture2D tex, Color color)
    {
        FillRect(tex, 0.16f, 0.40f, 0.84f, 0.54f, color);
        DrawTriangle(tex, color, 0.34f, 0.54f, 0.66f, 0.54f, 0.58f, 0.66f);
        FillRect(tex, 0.34f, 0.54f, 0.60f, 0.66f, color);
        FillCircle(tex, 0.32f, 0.38f, 0.10f, color);
        FillCircle(tex, 0.68f, 0.38f, 0.10f, color);
        PunchCircle(tex, 0.32f, 0.38f, 0.045f);
        PunchCircle(tex, 0.68f, 0.38f, 0.045f);
    }

    private static void DrawFacility(Texture2D tex, Color color)
    {
        FillRect(tex, 0.24f, 0.20f, 0.76f, 0.74f, color);
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
            {
                float x = 0.30f + c * 0.15f;
                float y = 0.30f + r * 0.13f;
                PunchRect(tex, x, y, x + 0.08f, y + 0.07f);
            }
        PunchRect(tex, 0.44f, 0.20f, 0.56f, 0.30f);
    }

    private static void DrawHome(Texture2D tex, Color color)
    {
        DrawTriangle(tex, color, 0.18f, 0.54f, 0.82f, 0.54f, 0.5f, 0.82f);
        FillRect(tex, 0.28f, 0.22f, 0.72f, 0.56f, color);
        PunchRect(tex, 0.45f, 0.22f, 0.55f, 0.42f);
        PunchRect(tex, 0.33f, 0.40f, 0.43f, 0.50f);
        PunchRect(tex, 0.57f, 0.40f, 0.67f, 0.50f);
    }

    private static void DrawChevron(Texture2D tex, Color color)
    {
        Line(tex, 0.42f, 0.28f, 0.62f, 0.5f, 0.07f, color);
        Line(tex, 0.62f, 0.5f, 0.42f, 0.72f, 0.07f, color);
    }
}
